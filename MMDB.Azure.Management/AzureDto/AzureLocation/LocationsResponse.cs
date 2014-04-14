﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MMDB.Azure.Management.AzureDto.AzureLocation
{
    [XmlType("Locations")]
    public class LocationsResponse
    {
        [XmlElement("Location")]
        public List<Location> LocationList { get; set; }
    }
}
