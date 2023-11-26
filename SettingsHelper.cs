using Windows.Storage;

namespace Browser
{
    public class SettingsHelper
    {
        private ApplicationDataContainer localSettings;

        public SettingsHelper()
        {
            localSettings = ApplicationData.Current.LocalSettings;
        }

        public void AddOrUpdateSetting(string key, bool? value)
        {
            localSettings.Values[key] = value.GetValueOrDefault(false); ;
        }

        public bool GetSetting(string key)
        {
            if (localSettings.Values.ContainsKey(key) && localSettings.Values[key] is bool)
            {
                return (bool)localSettings.Values[key];
            }
            // Возвращаем значение по умолчанию, если ключ отсутствует или тип не bool
            return false;
        }
    }
}

