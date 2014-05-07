using Microsoft.WindowsAzure.ServiceRuntime;
using MMDB.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MMDB.Azure.Settings
{
	public class AppSettingsAdapter : IAppSettingsAdapter
	{
		private bool? Azure { get; set; }

        public string GetSetting(string key)
        {
            return InternalGetSetting(key);
        }

        public string GetSetting(string key, string defaultValue)
        {
            return StringHelper.IsNullOrEmpty(this.InternalGetSetting(key), defaultValue);
       }

        public string GetRequiredSetting(string key)
		{
			string returnValue = InternalGetSetting(key);
			if (string.IsNullOrEmpty(returnValue))
			{
				throw new Exception(string.Format("Missing required configuration setting \"{0}\"", key));
			}
			return returnValue;
		}

        public bool? GetBoolSetting(string key)
        {
            string tempValue = InternalGetSetting(key);
            if (string.IsNullOrEmpty(tempValue))
            {
                return null;
            }
            else
            {
                bool boolValue;
                if (!bool.TryParse(tempValue, out boolValue))
                {
                    throw new Exception(string.Format("Failed to parse application setting \"{0}\" into a boolean, value: \"{1}\"", key, tempValue));
                }
                return boolValue;
            }
        }
        
        public bool GetBoolSetting(string key, bool defaultValue)
		{
            return this.GetBoolSetting(key).GetValueOrDefault(defaultValue);
		}

        public bool GetRequiredBoolSetting(string key)
        {
            bool? returnValue = this.GetBoolSetting(key);
            if (!returnValue.HasValue)
            {
                throw new Exception(string.Format("Missing required configuration setting \"{0}\"", key));
            }
            return returnValue.Value;
        }

		public int? GetIntSetting(string key)
		{
			int? returnValue;
			int tempValue;
			string stringValue = GetSetting(key);
			if (string.IsNullOrEmpty(stringValue))
			{
				returnValue = null;
			}
			else if (!int.TryParse(stringValue, out tempValue))
			{
				throw new Exception(string.Format("Failed to parse application setting \"{0}\" into a integer, value: \"{1}\"", key, stringValue));
			}
			else
			{
				returnValue = tempValue;
			}
			return returnValue;
		}

        public int GetIntSetting(string key, int defaultValue)
        {
            return this.GetIntSetting(key).GetValueOrDefault(defaultValue);
        }

        public int GetRequiredIntSetting(string key)
        {
            int? returnValue = this.GetIntSetting(key);
            if (!returnValue.HasValue)
            {
                throw new Exception(string.Format("Missing required configuration setting \"{0}\"", key));
            }
            return returnValue.Value;
        }

		private string InternalGetSetting(string key)
		{
			if (!this.Azure.HasValue)
			{
				try
				{
					if (RoleEnvironment.IsAvailable)
					{
						this.Azure = true;
					}
					else
					{
						this.Azure = false;
					}
				}
				catch (FileNotFoundException)
				{
					this.Azure = false;
				}
			}
			if (this.Azure.GetValueOrDefault(false))
			{
                try 
                {
    				return RoleEnvironment.GetConfigurationSettingValue(key);
                }
                catch(RoleEnvironmentException)
                {
                    return AppSettingsHelper.GetSetting(key);
                }
			}
			else
			{
				return AppSettingsHelper.GetSetting(key);
			}
		}

    }
}
