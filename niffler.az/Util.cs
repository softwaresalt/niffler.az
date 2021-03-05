using Alpaca.Markets;
using IdGen;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Niffler.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Niffler
{
	public class Util
	{
		public const string ESTZoneName = "Eastern Standard Time";
		public static TimeZoneInfo TimezoneEST { get; } = TimeZoneInfo.FindSystemTimeZoneById(ESTZoneName);

		public static TimeSpan CurrentEST
		{
			get { return CurrentDateTimeEST.TimeOfDay; }
		}

		public static string CurrentESTString
		{
			get { return CurrentDateTimeEST.ToString("HHmmss"); }
		}

		public static DateTime CurrentDateEST
		{
			get { return CurrentDateTimeEST.Date; }
		}

		public static string CurrentDateESTString
		{
			get { return CurrentDateTimeEST.ToString("yyyyMMdd"); }
		}

		public static string ConformedDateString(DateTime date)
		{
			return date.ToString("yyyyMMdd");
		}

		public static string ConformedTimeString(TimeSpan? span)
		{
			return (span.HasValue) ? span.Value.ToString("hhmmss") : null;
		}

		public static string ConformedKey(string value, DateTime date)
		{
			return ConformedKey(value, ConformedDateString(date));
		}

		public static string ConformedKey(DateTime date, string value)
		{
			return ConformedKey(ConformedDateString(date), value);
		}

		public static string ConformedKey(string value1, string value2, DateTime date)
		{
			return ConformedKey(new string[] { value1, value2, ConformedDateString(date) });
		}

		public static string ConformedKey(string value, TimeSpan time) => ConformedKey(value, ConformedTimeString(time));

		public static string ConformedKey(string value1, string value2, char separator = ':')
		{
			return ConformedKey(new string[] { value1, value2 }, separator);
		}

		public static string ConformedKey(string value1, string value2, string value3)
		{
			return ConformedKey(new string[] { value1, value2, value3 });
		}

		public static string ConformedKey(string[] elements, char separator = ':')
		{
			return String.Join(separator, elements);
		}

		public static string ConformedDateTimeEST
		{
			get
			{
				return ConformAsDateTimeString(CurrentDateTimeEST);
			}
		}

		public static string ConformAsDateTimeString(DateTime dt)
		{
			return dt.ToString("yyyy-MM-ddTHH:mm:ss");
		}

		public static DateTime CurrentDateTimeEST
		{
			get
			{
				return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimezoneEST);
			}
		}

		public static DateTime MarketOpenEST
		{
			get
			{
				return CurrentDateEST.Add(Cache.TimeBoundary[Boundary.T0930]);
			}
		}

		public static DateTime MarketClosePositionsByEST
		{
			get
			{
				return CurrentDateEST.Add(Cache.TimeBoundary[Boundary.T1550]);
			}
		}

		public static DateTime MarketClosePositionsByUTC
		{
			get
			{
				return ConvertESTtoUTC(MarketClosePositionsByEST);
			}
		}

		public static DateTime MarketOpenUTC
		{
			get
			{
				return ConvertESTtoUTC(MarketOpenEST);
			}
		}

		public static DateTime ConvertESTtoUTC(DateTime dt) => TimeZoneInfo.ConvertTime(dt, TimezoneEST, TimeZoneInfo.Utc);

		public static DateTime ConvertUTCtoEST(DateTime utc) => TimeZoneInfo.ConvertTime(utc, TimeZoneInfo.Utc, TimezoneEST);

		public static string ConformName(string name)
		{
			return name.Replace(" ", "").Replace("[", "").Replace("]", "");
		}

		public static object ChangeType(object value, Type conversion)
		{
			var t = conversion;

			if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
			{
				if (value == null)
				{
					return null;
				}

				t = Nullable.GetUnderlyingType(t);
			}

			return Convert.ChangeType(value, t);
		}

		/// <summary>
		/// Designed to log into a screen service site, such as FinViz,
		/// and fetch screen results based on parameterized query string.
		/// </summary>
		/// <param name="postParams">ServicePostParameterDTO</param>
		/// <returns>Full set of screen content as a string.</returns>
		public static async Task<string> GetRealtimeScreenData(ServicePostParameterDTO postParams)
		{
			Uri requestUri = new Uri(postParams.url, UriKind.Absolute);
			CookieContainer cookieContainer = new CookieContainer();
			string content = String.Empty;

			using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
			using (HttpClient client = new HttpClient())
			{
				var loginPageResult = client.GetAsync(postParams.loginURL);
				loginPageResult.Result.EnsureSuccessStatusCode();
				var formContent = new FormUrlEncodedContent(new[]
				{
					new KeyValuePair<string,string>(postParams.loginUsernameFormField, postParams.loginUsername),
					new KeyValuePair<string, string>(postParams.loginPasswordFormField, postParams.loginPassword)
				});
				var loginResult = client.PostAsync(postParams.loginSubmitURL, formContent);
				loginResult.Result.EnsureSuccessStatusCode();
				content = await client.GetStringAsync(requestUri);
			}
			return content;
		}

		//public static async Task<string> GetRealtimeQuoteData(ServicePostParameterDTO postParams, string symbol)
		//{
		//	postParams.url = string.Format(postParams.url, symbol);
		//	Uri requestUri = new Uri(postParams.url, UriKind.Absolute);
		//	string content = String.Empty;

		//	//using (var handler = new HttpClientHandler() { })
		//	using (HttpClient client = new HttpClient())
		//	{
		//		content = await client.GetStringAsync(requestUri);
		//	}
		//	return content;
		//}
		public static async Task<IReadOnlyList<IAgg>> GetRealtimeQuoteData(string symbol, TimeFrame period = TimeFrame.Minute, int limit = 100)
		{
			BarSetRequest bsr = new BarSetRequest(symbol, period) { Limit = limit };
			IReadOnlyDictionary<string, IReadOnlyList<IAgg>> bars = await Cache.APCADataClient.GetBarSetAsync(bsr);
			return bars[symbol];
		}

		public static async Task<IReadOnlyList<IAgg>> GetRealtimeQuoteData(string symbol, dynamic timeInterval, TimeFrame period = TimeFrame.Minute, int limit = 100)
		{
			return await GetRealtimeQuoteData(symbol, timeInterval, period, limit);
		}

		public static async Task<IReadOnlyList<IAgg>> GetRealtimeQuoteData(string symbol, IInclusiveTimeInterval timeInterval, TimeFrame period, int limit)
		{
			BarSetRequest bsr = new BarSetRequest(symbol, period) { Limit = limit };
			bsr.SetTimeInterval(timeInterval);
			IReadOnlyDictionary<string, IReadOnlyList<IAgg>> bars = await Cache.APCADataClient.GetBarSetAsync(bsr);
			return bars[symbol];
		}

		public static async Task<IReadOnlyList<IAgg>> GetRealtimeQuoteData(string symbol, IExclusiveTimeInterval timeInterval, TimeFrame period, int limit)
		{
			BarSetRequest bsr = new BarSetRequest(symbol, period) { Limit = limit };
			bsr.SetTimeInterval(timeInterval);
			IReadOnlyDictionary<string, IReadOnlyList<IAgg>> bars = await Cache.APCADataClient.GetBarSetAsync(bsr);
			return bars[symbol];
		}

		//public static List<QuoteLog> ConvertToQuoteLog(long workflowID, long serviceQueueID, DateTime queueDate, string symbol, List<IAgg> quotes)
		//{
		//	List<QuoteLog> ql = (
		//		from quote in quotes
		//		select new QuoteLog(ConformedKey(workflowID.ToString(), queueDate))
		//		{
		//			Symbol = symbol,
		//			WorkflowID = workflowID,
		//			PriceOpen = (double)quote.Open,
		//			PriceHigh = (double)quote.High,
		//			PriceLow = (double)quote.Low,
		//			PriceClose = (double)quote.Close,
		//			ServiceQueueID = serviceQueueID,
		//			Volume = quote.Volume,
		//			TimeStampRecorded = TimeZoneInfo.ConvertTimeFromUtc(quote.Time, Cache.EST)
		//		}
		//	).ToList();
		//	ql.ForEach(x => x.SetRowKey());
		//	return ql;
		//}
		//public static List<TradeBook> ConvertToTradeBook(long workflowID, long serviceQueueID, PositionType positionType, DateTime queueDate, string symbol, List<IAgg> quotes)
		//{
		//	List<TradeBook> trades = (
		//		from quote in quotes
		//		select new TradeBook(ConformedKey(workflowID.ToString(), queueDate))
		//		{
		//			Symbol = symbol,
		//			WorkflowID = workflowID,
		//			ServiceQueueID = serviceQueueID,
		//			LastQuotePrice = (double)quote.Close,
		//			LastQuoteVolume = (int)quote.Volume,
		//			ePositionType = positionType
		//		}
		//	).ToList();
		//	trades.ForEach(x => x.SetRowKey());
		//	return trades;
		//}
		public static int CalculatePositionCount(decimal accountLastEquity, int currentPositionCount = 0)
		{
			int n = 3;
			decimal splitAmt = (accountLastEquity / n);
			while (splitAmt < 30000 || splitAmt > 90000)
			{
				if (splitAmt <= 30000) { n--; }
				if (splitAmt >= 90000) { n++; }
				splitAmt = (accountLastEquity / n);
			}
			if (n > 10) { n = 10; } //Max intraday positions should rest at 10.
			n -= currentPositionCount;
			return (n >= 0) ? n : 0;
		}

		public static async Task RetryOnExceptionAsync(Func<Task> operation, ILogger logger)
		{
			await RetryOnExceptionAsync<Exception>(operation, logger);
		}

		public static async Task RetryOnExceptionAsync<TException>(Func<Task> operation, ILogger logger, bool isAccumulatingDelay = true, int times = 3, int intervalSeconds = 2) where TException : Exception
		{
			if (times <= 0) { times = 1; }
			int[] delays = generateDelayAttempts(times, isAccumulatingDelay, intervalSeconds);
			int attempts = 0;
			do
			{
				try
				{
					attempts++;
					await operation();
					break;
				}
				catch (TException ex)
				{
					if (attempts == times)
					{
						logger.LogError(ex, ex.Message + " : " + ex.StackTrace);
						throw;
					}

					await CreateDelayForException(times, attempts, delays, ex, logger);
				}
			} while (true);
		}

		public static async Task<T> RetryOnExceptionWithReturnAsync<T>(Func<Task<T>> operation, ILogger logger, bool isAccumulatingDelay = true, int times = 3, int intervalSeconds = 2)
		{
			if (times <= 0) { times = 1; }
			int[] delays = generateDelayAttempts(times, isAccumulatingDelay, intervalSeconds);
			int attempts = 0;
			do
			{
				try
				{
					attempts++;
					return await operation();
				}
				catch (Exception ex)
				{
					if (attempts >= times)
					{
						logger.LogError(ex, ex.Message + " : " + ex.StackTrace);
						throw;
					}
					await CreateDelayForException(times, attempts, delays, ex, logger);
				}
			} while (true);
		}

		private static Task CreateDelayForException(int times, int attempts, int[] delays, Exception ex, ILogger logger)
		{
			int delay = IncreasingDelayInMilliseconds(attempts, delays);
			logger.LogWarning($"Exception on attempt {attempts} of {times}. Will retry after sleeping for {delay}.", ex);
			return Task.Delay(delay);
		}

		/// <summary>
		/// Generates array of retry delay duration times in milliseconds.
		/// </summary>
		/// <param name="times"></param>
		/// <param name="isAccumulating"></param>
		/// <param name="interval"></param>
		/// <returns>Array of millisecond durations for retry logic.</returns>
		public static int[] generateDelayAttempts(int times = 3, bool isAccumulating = true, int interval = 2)
		{
			if (times <= 0) { times = 1; isAccumulating = false; interval = 1; }
			int[] attempts = new int[times];
			for (int n = 0; n < times; n++)
			{
				attempts[n] = (n > 0 && isAccumulating) ? (int)TimeSpan.FromSeconds(interval).TotalMilliseconds + attempts[n - 1] : (int)TimeSpan.FromSeconds(interval).TotalMilliseconds;
			}
			return attempts;
		}

		private static int IncreasingDelayInMilliseconds(int failedAttempts, int[] delays)
		{
			if (failedAttempts <= 0) { throw new ArgumentOutOfRangeException(); }

			return failedAttempts >= delays.Length ? delays.Last() : delays[failedAttempts];
		}
	}

	/// <summary>
	/// KenGen creates gated access to IDs to ensure they are picked off the list one at a time.
	/// </summary>
	public sealed class KeyGen
	{
		/// <summary>
		/// Uses a semaphore to gate access to queue of IDs;
		/// If the queue is empty, it gets populated first.
		/// </summary>
		/// <returns>long ID (Int64)</returns>
		public static async Task<long> NewID()
		{
			long ID;
			await Cache.IDSemaphore.WaitAsync();
			try
			{
				if (Cache.IDCache == null || Cache.IDCache.Count == 0)
				{
					Cache.IDCache = new Queue<long>(GenIDList(Cache.IDCacheQueueLimit));
				}
			}
			finally
			{
				ID = Cache.IDCache.Dequeue();
				Cache.IDSemaphore.Release();
			}
			return ID;
		}

		/// <summary>
		/// Generates enumerable list of long IDs.
		/// </summary>
		/// <param name="queueLength">Number of IDs to generate</param>
		/// <returns>Enumerable list of long IDs</returns>
		public static IEnumerable<long> GenIDList(int queueLength)
		{
			IdGenerator idGen = new IdGenerator(0);
			return idGen.Take(queueLength);
		}
	}

	public class JsonModuleTypeConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(int);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			string enumValue = reader.Value.ToString();
			return Enum.Parse(typeof(ModuleType), enumValue);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, (int)value);
		}
	}

	public class JsonServiceTypeConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(int);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			string enumValue = reader.Value.ToString();
			return Enum.Parse(typeof(ServiceType), enumValue);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, (int)value);
		}
	}

	public class JsonOperationTypeConverter : JsonConverter
	{
		public override bool CanConvert(Type objectType)
		{
			return objectType == typeof(int);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			string enumValue = reader.Value.ToString();
			return Enum.Parse(typeof(OperationType), enumValue);
		}

		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, (int)value);
		}
	}
}