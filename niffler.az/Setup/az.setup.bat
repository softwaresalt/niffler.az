::Follow each step in order
:: Execute each command one at a time after swapping out placeholders with your values; placeholders always indicated by [your-thing-here]
::Step 1: install node.js: https://nodejs.org/en/download/
::Step 2: Make sure that the command window is in Administrative mode first.
npm install -g azure-functions-core-tools@3
::Step 3: Install Azure CLI
npm install -g azure-cli
refreshenv
::Step 4: Log into Azure through the browser to cache local credentials for all CLI work to follow
az login
::Step 5: Set up your resource group; replace placeholder with your group name
::Alternate option here: https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/manage-resource-groups-portal
echo RESOURCE-GROUP-SETUP>>az.setup.log.json
az group create --location eastus --name [your-resource-group-name-here]>>az.setup.log.json
echo ----------------------------------------------------->>az.setup.log.json
::Step 6: Create your storage account; alt option: https://docs.microsoft.com/en-us/azure/storage/common/storage-account-create?tabs=azure-portal
echo STORAGE-ACCOUNT-SETUP>>az.setup.log.json
az storage account create --name [your-storage-account-name-here] -g [your-resource-group-name-here] --access-tier Hot --assign-identity -q Service -t Service --kind StorageV2 -l eastus>>az.setup.log.json
az storage account keys list -g [your-resource-group-name-here] -n [your-storage-account-name-here]>>az.setup.log.json
echo ----------------------------------------------------->>az.setup.log.json
::Step 6: Create the AppInsights logging for your function app; alt option: https://docs.microsoft.com/en-us/azure/azure-monitor/app/create-new-resource
echo APP-INSIGHTS-SETUP>>az.setup.log.json
az monitor app-insights component create --app [your-app-name-here]-devtest --location eastus --kind web -g [your-storage-account-name-here] --application-type web --retention-time 30>>az.setup.log.json
echo ----------------------------------------------------->>az.setup.log.json
::Step 6: Create your function app (empty version of the app initially); find instrumentationKey from output of app-insights creation
echo FUNCTION-APP-SETUP>>az.setup.log.json
az functionapp create -c eastus --name [your-app-name-here]-devtest --functions-version 3 --os-type Windows --resource-group [your-resource-group-name-here] --runtime dotnet --storage-account [your-storage-account-name-here] --app-insights-key [app-insights-instrumentationKey-here] --subscription [subscription-ID-here] --tags Environment=Paper>>az.setup.log.json
echo ----------------------------------------------------->>az.setup.log.json
::Step 7: Create storage tables that support application
az storage table create --name Account --account-name [your-storage-account-name-here]
az storage table create --name MarketSchedule --account-name [your-storage-account-name-here]
az storage table create --name Secret --account-name [your-storage-account-name-here]
az storage table create --name Service --account-name [your-storage-account-name-here]
az storage table create --name ServiceLoginParameter --account-name [your-storage-account-name-here]
az storage table create --name ServiceParameter --account-name [your-storage-account-name-here]
az storage table create --name SettingsDictionary --account-name [your-storage-account-name-here]
az storage table create --name Workflow --account-name [your-storage-account-name-here]
az storage table create --name WorkflowOperation --account-name [your-storage-account-name-here]
az storage table create --name Enums --account-name [your-storage-account-name-here]
az storage table create --name ServiceQueue --account-name [your-storage-account-name-here]
az storage table create --name DispatchSymbol --account-name [your-storage-account-name-here]
az storage table create --name QuoteLog --account-name [your-storage-account-name-here]
az storage table create --name ScreenLog --account-name [your-storage-account-name-here]
az storage table create --name TradeBook --account-name [your-storage-account-name-here]


::To Set Up keyvault
::az keyvault create --name [your-app-name-here]-devtest --resource-group [your-resource-group-name-here] --location eastus
::az functionapp identity assign --name [your-app-name-here]-devtest --resource-group [your-resource-group-name-here]
::az functionapp show --name [your-app-name-here]-devtest --resource-group [your-resource-group-name-here]
::az keyvault set-policy --name [your-app-name-here]-devtest --object-id [./identity/principalId from-previous-output] --secret-permissions get list

::az functionapp deployment source config-zip --resource-group [your-resource-group-name-here] --name [your-app-name-here]-devtest --src .\[your-app-name-here].az.debug.deploy.zip --verbose


::Miscellaneous tools/commands
::az --version
::az upgrade
::refreshenv
::az group list
::az storage account show-connection-string --name [your-storage-account-name-here]
::IDGen.exe -gc=20 -o=C:\Temp\ids.txt -c

