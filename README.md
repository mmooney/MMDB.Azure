MMDB.Azure
==========

Azure Tools/Libraries

## MMDB.Azure.Management ## 
[TeamCity Build Status
![TeamCity Build Status](http://build.mmdbsolutions.com/app/rest/builds/buildType:(id:MmdbOpenSource_MmdbAzureManagement_MmdbAzureManagementReleaseBuild)/statusIcon "TeamCity Build Status")] (http://build.mmdbsolutions.com/viewType.html?buildTypeId=MmdbOpenSource_MmdbAzureManagement_MmdbAzureManagementReleaseBuild&guest=1)
### A Really Easy Azure Management Library ###

MMDB.Azure.Maangement is a simple wrapper around the the [Azure Management REST APIs] (http://msdn.microsoft.com/en-us/library/azure/ee460799.aspx).  It allows you to list, create, get, and delete Cloud Services, Deployments, and Storage Accounts.  It has everything you need to deploy a Azure Cloud Service project in a few lines of C# code.

### Getting Started ###
First, you need an Azure account.  You can sign up at http://azure.microsoft.com/.  If you have any issuses, check out http://mooneyblog.mmdbsolutions.com/index.php/2013/01/20/windows-azure-2-setting-up-an-account/.

Next, you need Azure Management Certificate.  There are a lot of complicated ways to get this, but the easiest way to install the Azure Powershell Cmdlets (http://azure.microsoft.com/en-us/documentation/articles/install-configure-powershell/) and then the following command in Powershell:
```Powershell
  Get-AzurePublishSettingsFile
```
That will send you to the Azure site, prompt you to log in (if you're not logged in already) and then it will download a publish settings file.  Open up this XML file, and grab the Subscription/@Id and ManagementCertificate values.

For more information, see http://mooneyblog.mmdbsolutions.com/index.php/2013/01/23/windows-azure-4-deploying-via-powershell/

### Basic Usage ###
#### Creating a Cloud Service:####
```C#
    string subscriptionIdentifier = "FromYourPublishSettingsFile";
    string managementCertificate = "AlsoFromYourPublishSettingsFile";
    string serviceName = "MyNewServiceName";
    var client = new AzureClient(subscriptionIdentifier, managementCertificate);
                
    string message;
    bool nameIsAvailable = client.CheckCloudServiceNameAvailability(serviceName, out message);
    if(!nameIsAvailable)
    {
        throw new Exception("Cannot create " + serviceName + ", service name is not available!  Details" + message);
    }

    var service = client.CreateCloudService(serviceName);

    Console.WriteLine("Successfully created service " + serviceName  + "!  URL = " + service.Url);
```

#### Creating a Cloud Storage Account:####
```C#
    string subscriptionIdentifier = "FromYourPublishSettingsFile";
    string managementCertificate = "AlsoFromYourPublishSettingsFile";
    string storageAccountName = "MyNewStorageAccount";
    var client = new AzureClient(subscriptionIdentifier, managementCertificate);
                
    string message;
    bool nameIsAvailable = client.CheckStorageAccountNameAvailability(storageAccountName, out message)
    if(!nameIsAvailable)
    {
        throw new Exception("Cannot create " + storageAccountName + ", service name is not available!  Details" + message);
    }

    var storageAccount = client.CreateCloudService(storageAccountName);

    //Initial setup is complete, but it is still resolving DNS, etc
    Console.WriteLine("Initial creation for storage account " + storageAccountName + " complete!  URL = " + storageAccount.Url);

    //Wait for the entire setup to be complete
    client.WaitForStorageAccountStatus(storageAccountName, StorageServiceProperties.EnumStorageServiceStatus.Created, timeout:TimeSpan.FromMinutes(2));
    Console.WriteLine("Final setup " + storageAccountName + ", your storage account is ready to go");
```

#### Putting It All Together, Deploying an Azure Package ####
```C#
    string subscriptionIdentifier = "FromYourPublishSettingsFile";
    string managementCertificate = "AlsoFromYourPublishSettingsFile";
    var client = new AzureClient(subscriptionIdentifier, managementCertificate);

    string serviceName = "MyNewServiceName";
    var service = client.CreateCloudService(serviceName);

    string storageAccountName = "MyNewStorageAccount";
    var storageAccount = client.CreateStorageAccount(storageAccountName);
    client.WaitForStorageAccountStatus(storageAccountName, StorageServiceProperties.EnumStorageServiceStatus.Created);

    string azureContainerName = "MyDeploymentContainer";
    string azurePackageFile = "C:\\Build\\MyAzurePackage.cspkg";
    string azureConfigFile = "C:\\Build\\MyAzureConfig.cscfg";
    string azureConfigData = File.ReadAllText(azureConfigFile);
    string deploymentSlot = "staging";

    var storageKeys = client.GetStorageAccountKeys(storageAccountName);
    var blobUrl = client.UploadBlobFile(storageAccountName, storageKeys.Primary, azurePackageFile, azureContainerName);

    var deployment = client.CreateCloudServiceDeployment(serviceName, blobUrl, azureConfigData, deploymentSlot);
    client.WaitForCloudServiceDeploymentStatus(serviceName, deploymentSlot, DeploymentItem.EnumDeploymentItemStatus.Running, TimeSpan.FromMinutes(5));
    client.WaitForAllCloudServiceInstanceStatus(serviceName, deploymentSlot, RoleInstance.EnumInstanceStatus.ReadyRole, TimeSpan.FromMinutes(10));
    return deployment;
```
