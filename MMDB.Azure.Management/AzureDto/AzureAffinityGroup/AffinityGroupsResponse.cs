using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MMDB.Azure.Management.AzureDto.AzureAffinityGroup
{
    [XmlType("AffinityGroups")]
    public class AffinityGroupsResponse
    {
        [XmlElement("AffinityGroup")]
        public List<AffinityGroup> AffinityGroupList { get; set; }
    }
}
