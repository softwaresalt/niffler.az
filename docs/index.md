# niffler.az

![.NETCore Build](https://github.com/softwaresalt/niffler.az/workflows/.NET/badge.svg)

Event driven Azure serverless platform for automated, algorithmic trading. Implemented in C#, using Azure storage and Key Vault.  Modular design allows algorithmic traders to develop their own bespoke implementations by simply creating their own algorithm module and plugging into the existing code structure to operate a highly economical, algorithmic trading system.

The solution is only intended for single tenant use, although it is also designed to be operational on multiple accounts and algorithms, so you can run multiple algorithms off of a single tenant that may target multiple online brokerage accounts.

### Features
- Implements simple event processing model to drive workflow; uses storage queues as the channels for asynchronous processing of workflow operations, which creates a highly resilient model for serverless applications; since actions are injected into the channel, system failures are effectively checkpointed and rerunnable.
- Serverless architecture that doesn't run outside of market hours based entirely on Azure serverless functions coded in C#.
- Uses inexpensive Azure table storage for configuration, logging, account management and trade operations and tracking.
- Uses Azure storage queues as part of a simple event driven model that includes a kick-off timer and which allows for checkpoints and retries in case of failures.  
- Interface based module framework for implementing custom algorithms into the application.
- Extensible model for adding adapters for different online brokerages.
- Extensible model for adding new quote providers.
- Extensible model for adding new screening services.
- Allows you to configure limits on # of positions to take and how much per position.
- Uses Azure Key Vault to secure login info for services consumed.
- Enables a workflow sequence for trade operations and decision making.
- Enables e-mail notification of trade operations and errors.
- Caches operational data in memory for quicker access during session.
- Gates some operations for transactional consistency.

### Current limitations
- Uses FinViz.com for stock screening services.
- Uses Alpaca for online brokerage services, NuGet package for Alpaca API.
- Uses Polygon.io API with Alpaca account for free quote data
- Does not implement streaming quote data; lowest level is 1 minute quote data.


### Baseline Requirements
- .NETCore 3.1
- [FinViz.com](https://finviz.com/) account (screening service)
- [Alpaca](https://app.alpaca.markets/) account (no trading fees, no cost to open, API based brokerage)
- [Azure account](https://portal.azure.com/)

### System Setup
1. Follow the steps in the az.setup.bat file; execute each step individually. Don't run the whole batch file at once
1. Open the az.setup.log.json file with all output from step 1.
1. Find the niffler.config.json file at the root of the project and open.
1. Paste the "instrumentationKey" GUID value into the value field in quotes for the following nodes: APPINSIGHTS_INSTRUMENTATIONKEY, APPLICATIONINSIGHTS_CONNECTION_STRING ("value": "InstrumentationKey=[GUID-value-here]")
1. Run command: ```az storage account show-connection-string --name [your-storage-account-name-here]```
1. Copy the value in quotes after ```"connectionString":```
1. Paste the value into the value field in quotes for the following nodes: AzureWebJobsStorage, WEBSITE_CONTENTAZUREFILECONNECTIONSTRING
1. Copy the APCA-API-KEY-ID and APCA-API-SECRET-KEY values from the Alpaca site after creating your account and paste into the corresponding value fields in quotes in the config.json file.
1. If you want to use notification, you'll need to provide values in the config for NotifyFromAddress, Notification-Credential-UN, and Notification-Credential-PW. Note that the Mail class uses SMTP as the delivery method.
1. Log into the [Azure portal](https://portal.azure.com) and navigate to the Configuration pane of your function app.
1. Click on Advanced Edit, copy all of the text from your modified version of niffler.config.json and paste over the existing text in the advanced edit pane.

### How to publish with Visual Studio 2019
The recommended method of deploying Azure serverless function apps to Azure is via a deployment Zip file.
Perhaps the easiest way to do this is to set up a publish profile in Visual Studio that publishes to a local file system folder.  
1. With the project open in Visual Studio, right-click the niffler.az project and select Publish.
1. A dialog box should appear asking you to choose a target; select Folder as the option and click [Next].
1. It will then ask you for a Specific Target; again select Folder and click [Next].
1. The default will be ```bin\Release\netcoreapp3.1\publish\```, which should be fine; click [Finish].
1. Next to your new FolderProfile publish profile, click the Publish button to publish to this folder.
1. You can rename the profile to something more specific like ReleaseFolderPublishProfile.  You may want to create a debug publish profile as well.
1. To deploy to Azure using this technique, next follow the steps for deploying a zip package using Azure CLI.


### How to deploy with [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
I would suggest first creating a deployment folder close to the root of the drive.  For example, ```C:\az.deploy\zip```  You will use the command window to navigate to this folder either by using the CD or PUSHD command.  For the purposes of this guide, I will use the example.

1. Create a deployment folder: ```C:\az.deploy\zip```
1. Create a new, empty Zip file named [niffler.az.release.deploy.zip]; open the Zip file.
1. Using Windows Explorer on a PC, navigate to the publish folder to which you published the release version of niffler.az.
1. Select all files [Ctrl]+[A]; drag selected files into the open Zip file window and use default settings to include relative file paths.
1. Once all files and folders from the publish directory are in the Zip file, close the Zip file.
1. Open a command window; navigate to ```C:\az.deploy\zip```
1. Run command: ```az functionapp deployment source config-zip --resource-group [your-resource-group-name-here] --name [your-app-name-here]-devtest --src .\[your-app-name-here].az.release.deploy.zip --verbose```

### Notes and Tools
- IDs in the system are 18 digit long(Int64) unsigned integers generated using a version of RobThree/IdGen, which is itself an implemenation of Twitter's Snowflake project.  You can download the tool (IDGen.exe) I use to generate these IDs from the commandline [here](https://github.com/softwaresalt/idGenExe/releases/download/v1.0.0/IDGen.exe).
- You can also make use of pre-generated ID values in /Setup/id.queue.txt

### Table Storage Schema (PartitionKey, RowKey)
- Account: PartitionKey=AccountID; RowKey=AccountID;
- Secret: PartitionKey=AccountID; RowKey=AccountID;
- Service: PartitionKey=ServiceID; RowKey=ServiceID;
- ServiceParameter: PartitionKey=ServiceID; RowKey=ParameterID:Version;
- ServiceLoginParameter: PartitionKey=ServiceID; RowKey=ServiceLoginParameterType;
- Workflow: PartitionKey=AccountID; RowKey=WorkflowID;
- WorkflowOperation: PartitionKey=WorkflowID; RowKey=ServiceType:OperationType;

### How to configure with [Azure Storage Explorer](https://github.com/Microsoft/AzureStorageExplorer/releases/latest)
1. Using Storage Explorer, log into your Azure account and navigate to the storage tables that were created in a previous step.
1. Open up each table, which should be empty, and using the Import command in the top ribbon, navigate to the /Setup/az.table.data folder in the project to load the corresponding table file data [table-name].typed.csv.
1. Note that to customize, you'll need to generate additional IDs for table configuration, which you can do using the IDGen.exe noted under Notes and Tools.
1. The Enums table contains actual Enums used in the code and their members.  You can use some of these in your configuration as options.

### Known Issues
- Currently the only screen and brokerage service adapters includes are for FinViz and the Alpaca online brokerage.
- Data service is currently provided through the Alpaca data APIs; Polygon.io was deprecated
- Code base does not yet implement Websocket streaming quote capabilities; this would also require a different deployment than standard Azure function app, which times out after an interval.
- Alpaca data API provides down to 1-minute quote data, although there is not a price fixed to it.  AlphaVantage can also provides quote data either for free or at a low price, although I have found issues with the quality of that data from previous tests.

### Project Status and Contributions

You can add to this project by contributing code improvements, new adapters for screen, quote, and online broker APIs to enable broader usage.
You can create your own fork of the project for personal use and/or add new algorithm modules you may have tested out. Please contribute back to the project anything that would improve it via a Pull Request.
