using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMDB.Azure.Settings
{
	public interface IAppSettingsAdapter
	{
        string GetSetting(string key);
        string GetSetting(string key, string defaultValue);
        string GetRequiredSetting(string key);

        bool? GetBoolSetting(string key);
        bool GetBoolSetting(string key, bool defaultValue);
        bool GetRequiredBoolSetting(string key);

        int? GetIntSetting(string key);
        int GetIntSetting(string key, int defaultValue);
        int GetRequiredIntSetting(string key);
    }
}
