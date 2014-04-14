using MMDB.Azure.Management.AzureDto.AzureCloudService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMDB.Azure.Management
{
    public interface IAzureClient
    {
        List<HostedService> GetCloudServiceList();
        HostedService CreateCloudService(string serviceName, string label=null, string location=null, string affinityGroup=null);
        HostedService GetCloudService(string serviceName);
        void DeleteCloudService(string serviceName);
    }
}
