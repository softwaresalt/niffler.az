using GenericParsing;
using Microsoft.Extensions.Logging;
using Niffler.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Niffler
{
	/// <summary>
	/// Helps the ServiceManager map tabulated string content into a DTO or ATS object.
	/// </summary>
	public class ServiceTypeMappingHelper
	{
		public FieldMap fieldMap;
		public ServiceType serviceType;
		private readonly ILogger Logger;
		public const string SetRowKeyMethod = "SetRowKey";

		public ServiceTypeMappingHelper(ILogger log)
		{
			this.Logger = log;
		}

		public ServiceTypeMappingHelper(ILogger log, ServiceType svcType)
		{
			this.Logger = log;
			fieldMap = new FieldMap(svcType);
			serviceType = svcType;
		}

		/// <summary>
		/// Inserts a new field to the CSV|TSV content with constant value across all rows.
		/// </summary>
		/// <param name="content">Original content</param>
		/// <param name="delimiter"></param>
		/// <param name="firstRowHasHeader"></param>
		/// <returns>Original content + new field/value inlined.</returns>
		public string AddFieldValueToContent(string content, char delimiter = ',', bool firstRowHasHeader = true)
		{
			TextReader rdr = new StringReader(content);
			StringWriter contentPlus = new StringWriter();
			string line; bool isHeaderModified = false;
			List<string> fields = new List<string>();
			List<string> values = new List<string>();
			//char textQualifier = '"';
			foreach (var kvp in fieldMap.FieldValues)
			{
				fields.Add(kvp.Key);
				values.Add(kvp.Value);
			}

			while (rdr.Peek() > 0)
			{
				line = rdr.ReadLine();
				if (firstRowHasHeader && !isHeaderModified)
				{
					contentPlus.WriteLine(line + delimiter + String.Join(delimiter.ToString(), fields));
					isHeaderModified = true;
				}
				else
				{
					contentPlus.WriteLine(line + delimiter + String.Join(delimiter.ToString(), values));
				}
			}
			return contentPlus.ToString();
		}

		/// <summary>
		/// Uses FieldMap key/value pairs w/ GenericParser to parse CSV|TSV data sets
		/// into generic list of type T; key=source field name; value= T type property name.
		/// Method assumes ATS object with inherited PartitionKey and RowKey properties;
		/// Sets RowKey from dynamically invoked SetRowKey method on object T.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="entityType">DTO or ATS object type to parse values into</param>
		/// <param name="content">CSV|TSV content to parse</param>
		/// <param name="workflowID">Parameter used in setting ATS partition key.</param>
		/// <param name="queueDate">Parameter used in setting ATS partition key.</param>
		/// <param name="topNCount">Max number of rows to return.</param>
		/// <param name="delimiter">The delimiter to use in parsing row data from string content.</param>
		/// <returns>Generic List of type T objects populated from CSV|TSV data set.</returns>
		public async Task<List<T>> GenerateEntityList<T>(Type entityType, string content, long workflowID, DateTime queueDate, int topNCount = -1, char delimiter = ',')
		{
			int rowCount = 0;
			string value;
			string partitionKey = Util.ConformedKey(workflowID.ToString(), queueDate);
			List<T> entityList = new List<T>();
			TextReader rdr = new StringReader(content);
			using (GenericParser parser = new GenericParser(rdr))
			{
				parser.FirstRowHasHeader = true;
				parser.SkipEmptyRows = true;
				parser.ColumnDelimiter = delimiter;
				parser.TextQualifier = '\"';
				while (parser.Read())
				{
					ConstructorInfo ctor = entityType.GetConstructor(new[] { typeof(string) });
					object entity = ctor.Invoke(new object[] { partitionKey });
					foreach (var kvp in fieldMap.ColumnMap)
					{
						//Look up the CSV value with the key
						//value = (parser[kvp.Key] != null) ? parser[kvp.Key].Replace("%", "").Replace("--", "0").Trim() : null;
						value = parser[kvp.Key]?.Replace("%", "").Replace("--", "0").Trim();
						if (value == string.Empty)
						{
							value = null;
						}
						//Set the value in the DataRow field
						PropertyInfo info = entityType.GetProperty(kvp.Value);
						if (info != null)
						{
							try
							{
								info.SetValue(entity, Util.ChangeType(value, info.PropertyType));
							}
							catch (Exception e)
							{
								string formatter = "Unable to convert value ({value}) to data type ({type}) for the property ({property}).#Error Message: {error}#Stack Trace: {trace}".Replace("#", Environment.NewLine);
								this.Logger.LogWarning(formatter, value, info.PropertyType, info.Name, e.Message, e.StackTrace);
							}
						}
					}
					MethodInfo method = entityType.GetMethod(SetRowKeyMethod);
					method.Invoke(entity, null);
					entityList.Add((T)entity);
					rowCount++;
					if (rowCount == topNCount) { break; }
				}
			}
			return entityList;
		}
	}
}