using MMDB.Azure.Management.AzureDto.AzureCloudService;
using MMDB.Azure.Management.AzureDto.AzureStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MMDB.Azure.Management.AzureDto.AzureAffinityGroup
{
    [XmlType("AffinityGroup")]
    public class AffinityGroup
    {
        public enum EnumAffinityGroupCapability
        {
            PersistentVMRole,
            HighMemory
        }

        public string Name { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }

        [XmlArray("HostedServices")]
        public List<HostedService> HostedServiceList { get; set; }

        [XmlArray("StorageServices")]
        public List<StorageService> StorageServiceList { get; set; }

        [XmlArray("Capability")]
        public List<EnumAffinityGroupCapability> Capabilities { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}
