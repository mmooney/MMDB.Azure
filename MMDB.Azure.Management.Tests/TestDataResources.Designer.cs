﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.34011
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MMDB.Azure.Management.Tests {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class TestDataResources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal TestDataResources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("MMDB.Azure.Management.Tests.TestDataResources", typeof(TestDataResources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        internal static byte[] MMDB_AzureSample_Web_Azure {
            get {
                object obj = ResourceManager.GetObject("MMDB_AzureSample_Web_Azure", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;ServiceConfiguration serviceName=&quot;MMDB.AzureSample.Web.Azure&quot; xmlns=&quot;http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration&quot; osFamily=&quot;2&quot; osVersion=&quot;*&quot; schemaVersion=&quot;2014-01.2.3&quot;&gt;
        ///  &lt;Role name=&quot;MMDB.AzureSample.Web&quot;&gt;
        ///    &lt;Instances count=&quot;1&quot; /&gt;
        ///    &lt;ConfigurationSettings&gt;
        ///      &lt;Setting name=&quot;Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString&quot; value=&quot;DefaultEndpointsProtocol=https;AccountName=mmdbazuresamplestorage;AccountKey=tzLoip05d [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string ServiceConfiguration_Cloud {
            get {
                return ResourceManager.GetString("ServiceConfiguration_Cloud", resourceCulture);
            }
        }
    }
}
