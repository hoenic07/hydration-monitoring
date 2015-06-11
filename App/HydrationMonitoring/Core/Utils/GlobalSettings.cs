using HydrationMonitoring.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

namespace HydrationMonitoring.Utils
{
    public class GlobalSettings
    {
        private Dictionary<string, object> _defaultValues;

        private static GlobalSettings _instance;

        public static GlobalSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GlobalSettings();
                }
                return _instance;
            }
        }

        private GlobalSettings()
        {
            SetDefaultValues();

            ApplyDefaultValues();
        }

        public object this[string name]
        {
            get
            {
                return ApplicationData.Current.LocalSettings.Values[name];
            }
            set
            {
                ApplicationData.Current.LocalSettings.Values[name] = value;
            }
        }

        /// <summary>
        /// Sets the default values for any settings
        /// </summary>
        private void SetDefaultValues()
        {
            _defaultValues = new Dictionary<string, object>();
            _defaultValues["User"] = new User();
            _defaultValues["SelectedCanIndex"] = 0;
            _defaultValues["NotificationsEnabled"] = true;
        }

        /// <summary>
        /// Stores the default value in the local settings
        /// </summary>
        /// <returns></returns>
        public async Task ApplyDefaultValues()
        {
            foreach (var item in _defaultValues)
            {
                if (this[item.Key] == null)
                {
                    Save(item.Key, item.Value);
                }
            }            
        }

        /// <summary>
        /// Saves a setting in the local settings file of the app
        /// </summary>
        /// <param name="name">name of the setting</param>
        /// <param name="data">data to store</param>
        public void Save(string name, object data)
        {
            if (data == null) return;
            var json = JsonConvert.SerializeObject(data);
            this[name] = json;
        }

        /// <summary>
        /// Gets a setting form the local settings file of the app
        /// </summary>
        /// <typeparam name="T">Type if the settings</typeparam>
        /// <param name="name">Name of the setting</param>
        /// <returns>The value of the setting</returns>
        public T Get<T>(string name)
        {
            var json = (string)this[name];
            return JsonConvert.DeserializeObject<T>(json);
        }

        /// <summary>
        /// Saves settings that are to big for the settings file to an own file - async
        /// Currently used for the hydration file
        /// </summary>
        /// <param name="name">Name of the setting/file</param>
        /// <param name="data">The data to store</param>
        /// <returns></returns>
        public async Task SaveToFile(string name, object data)
        {
            if (data == null) return;
            var json = JsonConvert.SerializeObject(data);

            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
            var s = await file.OpenAsync(FileAccessMode.ReadWrite);
            var writer = new StreamWriter(s.AsStreamForWrite());
            writer.Write(json);
            await writer.FlushAsync();
            writer.Dispose();
        }


        /// <summary>
        /// Gets the content of the file as an object of the given type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name">name of the file/setting</param>
        /// <returns>object value</returns>
        public async Task<T> GetFromFile<T>(string name)
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            var s = await file.OpenAsync(FileAccessMode.Read);
            var reader = new StreamReader(s.AsStreamForRead());
            var json = reader.ReadToEnd();
            reader.Dispose();
            return JsonConvert.DeserializeObject<T>(json);
        }


        private User _user;

        public User User
        {
            get
            {
                if (_user == null) _user = Get<User>("User");
                return _user;
            }
            set
            {
                Save("User", value);
                _user = value;
            }
        }

        private DailyHydration _todaysHydration;

        public DailyHydration TodaysHydration
        {
            get
            {
                if (_todaysHydration == null) _todaysHydration = new DailyHydration();
                return _todaysHydration;
            }
        }

        /// <summary>
        /// Loads the hydration from the file and saves it locally
        /// </summary>
        /// <returns></returns>
        public async Task LoadDailyHydration()
        {
            _todaysHydration = await GetFromFile<DailyHydration>("TodaysHydratrion");
        }

        /// <summary>
        /// Saves the current hydration to the file
        /// </summary>
        /// <returns></returns>
        public async Task SaveDailyHydration(){
            await  SaveToFile("TodaysHydratrion", TodaysHydration);
        }

        /// <summary>
        /// Saves the given hydration to the file
        /// and saves it in a file
        /// </summary>
        /// <param name="hyd"></param>
        /// <returns></returns>
        public async Task SaveDailyHydration(DailyHydration hyd)
        {
            _todaysHydration = hyd;
            SaveDailyHydration();
        }

        private int _selectedCanIndex = -1;

        public int SelectedCanIndex
        {
            get
            {
                if (_selectedCanIndex == -1) _selectedCanIndex = Get<int>("SelectedCanIndex");
                return _selectedCanIndex;
            }
            set
            {
                Save("SelectedCanIndex", value);
                _selectedCanIndex = value;
                MessageCenter.Notifications.Publish(NotificationKind.SensorCanChanged);
            }
        }

        public bool NotificationsEnabled
        {
            get { return Get<bool>("NotificationsEnabled"); }
            set { Save("NotificationsEnabled", value); }
        }
        
        
    }
}