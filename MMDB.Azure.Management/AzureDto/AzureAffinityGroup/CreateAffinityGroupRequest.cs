using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MMDB.Azure.Management.AzureDto.AzureAffinityGroup
{
    [XmlType("CreateAffinityGroup")]
    public class CreateAffinityGroupRequest
    {
        public string Name { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
    }
}
