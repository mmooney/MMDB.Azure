﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace MMDB.Azure.Management.AzureDto.AzureStorage
{
    [XmlType("CustomDomain")]
    public class CustomDomain
    {
        public string Name { get; set; }
    }
}
