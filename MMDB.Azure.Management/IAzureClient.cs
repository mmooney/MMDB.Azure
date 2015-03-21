using MMDB.Azure.Management.AzureDto.AzureAffinityGroup;
using MMDB.Azure.Management.AzureDto.AzureCloudService;
using MMDB.Azure.Management.AzureDto.AzureLocation;
using MMDB.Azure.Management.AzureDto.AzureStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMDB.Azure.Management
{
    public interface IAzureClient
    {
        List<HostedService> GetCloudServiceList();
        bool CheckCloudServiceNameAvailability(string serviceName, out string message);
        HostedService CreateCloudService(string serviceName, string label=null, string location=null, string affinityGroup=null);
        HostedService GetCloudService(string serviceName);
        void DeleteCloudService(string serviceName, bool waitForDelete = false, TimeSpan timeout = default(TimeSpan));

        List<StorageService> GetStorageAccountList();
        bool CheckStorageAccountNameAvailability(string storageAccountName, out string message);
        StorageService CreateStorageAccount(string storageAccountName, string label = null, string location = null, string affinityGroup = null);
        StorageService GetStorageAccount(string storageAccountName);
        void DeleteStorageAccount(string storageAccountName, bool waitForDelete=false, TimeSpan timeout=default(TimeSpan));
        StorageService WaitForStorageAccountStatus(string storageAccountName, StorageServiceProperties.EnumStorageServiceStatus status, TimeSpan timeout=default(TimeSpan));

        StorageServiceKeys GetStorageAccountKeys(string storageAccountName);

        string UploadBlobFile(string storageAccountName, string accessKey, string localPath, string containerName);

        List<Location> GetLocationList();

        List<AffinityGroup> GetAffinityGroupList();
        AffinityGroup CreateAffinityGroup(string affinityGroupName, string label=null, string description=null, string location=null);
        AffinityGroup GetAffinityGroup(string affinityGroupName);
        void DeleteAffinityGroup(string affinityGroupName);

        List<DeploymentItem> GetCloudServiceDeploymentList(string serviceName);
        DeploymentItem CreateCloudServiceDeployment(string serviceName, string blobUrl, string configurationData, string deploymentSlot);
        DeploymentItem GetCloudServiceDeployment(string serviceName, string deploymentSlot);
        DeploymentItem WaitForCloudServiceDeploymentStatus(string serviceName, string deploymentSlot, DeploymentItem.EnumDeploymentItemStatus status, TimeSpan timeout=default(TimeSpan));
        DeploymentItem WaitForAllCloudServiceInstanceStatus(string serviceName, string deploymentSlot, RoleInstance.EnumInstanceStatus status, TimeSpan timeout=default(TimeSpan));
    }
}
