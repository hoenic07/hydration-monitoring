using Caliburn.Micro;
using HydrationMonitoring.Models;
using HydrationMonitoring.Modules;
using HydrationMonitoring.Networking;
using HydrationMonitoring.Utils;
using HydrationMonitoring.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Devices.Geolocation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace HydrationMonitoring.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        #region Properties

        private string _currentTemperature;

        public string CurrentTemperature
        {
            get { return _currentTemperature; }
            set { _currentTemperature = value; NotifyOfPropertyChange(() => CurrentTemperature); }
        }

        public List<Can> Cans { get; set; }

        public string FluidTodayString
        {
            get { return string.Format("{0:0}", FluidToday*1000); }
        }

        private double _fluidToday;

        public double FluidToday
        {
            get { return _fluidToday; }
            set
            {
                _fluidToday = value;
                NotifyOfPropertyChange(() => FluidToday);
                NotifyOfPropertyChange(() => FluidTodayString);
            }
        }

        public string HydrationString
        {
            get
            {
                var val = GlobalSettings.Instance.TodaysHydration.CurrentHydration;
                return string.Format("{0:0.00}",val);
            }
        }

        private Visibility _isSensorActive;

        public Visibility IsSensorActive
        {
            get { return _isSensorActive; }
            set { _isSensorActive = value; NotifyOfPropertyChange(() => IsSensorActive); }
        }

        public string NotificationsEnabled
        {
            get
            {
                if (GlobalSettings.Instance.NotificationsEnabled) return "disable notifications";
                else return "enable notifications";
            }
        }

        public string LastSensorRead { get; set; }

        public HydrationState HydrationState { get; set; }

        public string HydrationPercentageString
        {
            get
            {
                if (GlobalSettings.Instance.TodaysHydration.Drinks == null || GlobalSettings.Instance.TodaysHydration.Drinks.Count == 0) return "-";
                return string.Format("{0:0}", GlobalSettings.Instance.TodaysHydration.Drinks.Last().Volume * 1000);
                //var val = GlobalSettings.Instance.TodaysHydratrion.CurrentHydration;
                //var we = GlobalSettings.Instance.User.WeightInKg;
                //return string.Format("{0:0.00}", val / we * 60);
            }
        }

        #endregion

        /// <summary>
        /// Registers for many uodates and starts BT init
        /// </summary>
        /// <param name="nav"></param>
        public MainViewModel(INavigationService nav):base(nav)
        {
            CurrentTemperature = "...";
            Cans = Can.AvailableStaticCans;

            MessageCenter.Instance.Register<DrinkStamp>(this, MessageKind.AddDrink, DrinkAdded);
            MessageCenter.Notifications.Register(this, NotificationKind.HydrationUpdated, HydrationUpdated);
            HydrationCalculation.Instance.Start();
            BtleSensorService.Instance.InitDevice();
            HydrationState.InitStates();
            IsSensorActive = Visibility.Collapsed;
            BtleSensorService.Instance.BtleAccelerometerReadingUpdated += SensorVolumeCalculation.Instance.SensorValueUpdate;
            BtleSensorService.Instance.BtleAccelerometerReadingUpdated += Instance_BtleAccelerometerReadingUpdated;
            SensorVolumeCalculation.Instance.DrinkDetected += Instance_DrinkDetected;
            SensorVolumeCalculation.Instance.SensorAvailabilityChanged += Instance_SensorAvailabilityChanged;

            RegisterBackgroundTask();
        }

        #region Message handlers

        /// <summary>
        /// Shows the last accelerometer value on the ui
        /// Just for testing
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Instance_BtleAccelerometerReadingUpdated(BtleSensorService sender, BtleAccelerometerReading args)
        {
            Execute.OnUIThread(() =>
            {
                LastSensorRead = args.Y.ToString();
                NotifyOfPropertyChange(() => LastSensorRead);
            });
        }

        /// <summary>
        /// Shows the availability of the sensor on the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Instance_SensorAvailabilityChanged(object sender, bool args)
        {
            Execute.OnUIThread(() =>
            {
                if (args)
                {
                    IsSensorActive = Visibility.Visible;
                }
                else
                {
                    IsSensorActive = Visibility.Collapsed;
                }
            });

        }

        /// <summary>
        /// A detected drink by the sensor calculation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void Instance_DrinkDetected(object sender, SensorDrinkStamp args)
        {
            DrinkAdded(args);
        }

        /// <summary>
        /// Updates the UI when the hydration was updates
        /// </summary>
        private void HydrationUpdated()
        {
            FluidToday = GlobalSettings.Instance.TodaysHydration.TotalLitersToday;
            NotifyOfPropertyChange(() => HydrationString);
            NotifyOfPropertyChange(() => HydrationPercentageString);
            double hPerc = (GlobalSettings.Instance.TodaysHydration.CurrentHydration / (GlobalSettings.Instance.User.WeightInKg * 0.6))*100;
            HydrationState = HydrationState.StateForCurrentHydration(hPerc);
            NotifyOfPropertyChange(() => HydrationState);
        }

        /// <summary>
        /// Any drink added leads to an update of the hydration
        /// </summary>
        /// <param name="drinkStamp"></param>
        private void DrinkAdded(DrinkStamp drinkStamp)
        {
            GlobalSettings.Instance.TodaysHydration.Drinks.Add(drinkStamp);
            HydrationCalculation.Instance.UpdateHydration();
        }

        #endregion

        #region Methods

        protected override void OnActivate()
        {
            _navigationService.BackStack.Clear();
            GetPositionAndTemperature();
        }

        /// <summary>
        /// Gets the current position and displays the fetched temperature on the UI
        /// </summary>
        private async void GetPositionAndTemperature()
        {
            Geolocator loc = new Geolocator();
            if (loc.LocationStatus == PositionStatus.Disabled) return;
            var pos = await loc.GetGeopositionAsync();
            var temp = await WeatherApiConnection.GetTemperatureForPosition(pos.Coordinate);

            if (temp != -1000)
            {
                CurrentTemperature = string.Format("{0}°", (int)temp);
            }
            else
            {
                CurrentTemperature = "N/A";
            }

        }

        /// <summary>
        /// Registers the background task that shows the notifications
        /// </summary>
        /// <returns></returns>
        public async Task RegisterBackgroundTask()
        {
            var taskRegistered = false;
            var exampleTaskName = "HydrationBackgroundTrigger";

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == exampleTaskName)
                {
                    taskRegistered = true;
                    break;
                }
            }

            if (!taskRegistered)
            {
                var builder = new BackgroundTaskBuilder();

                builder.Name = "HydrationBackgroundTrigger";
                builder.TaskEntryPoint = "BackgroundTask.HydrationBackgroundTrigger";
                builder.SetTrigger(new TimeTrigger(30, false));
                
                BackgroundTaskRegistration task = builder.Register();
                await BackgroundExecutionManager.RequestAccessAsync();
            }

            // Just for testing the background task
            if (false)
            {
                BackgroundTask.HydrationBackgroundTrigger w = new BackgroundTask.HydrationBackgroundTrigger();
                w.Run(null);
            }

        }


        #endregion

        #region Actions

        /// <summary>
        /// Shows the drink page
        /// </summary>
        /// <param name="can"></param>
        public void AddDrink(Can can)
        {
            if (can.Volume != -1)
            {
                var ds = new DrinkStamp { Volume = can.Volume, Time = DateTime.Now };
                DrinkAdded(ds);
            }
            else
            {
                _navigationService.UriFor<AddCustomDrinkViewModel>().Navigate();
            }
        }

        /// <summary>
        /// Shows the edit profile page
        /// </summary>
        public void EditProfile()
        {
            _navigationService.UriFor<UserSettingsViewModel>().Navigate();
        }

        /// <summary>
        /// Shows the workout page
        /// </summary>
        public void AddWorkout()
        {
            _navigationService.UriFor<AddWorkoutViewModel>().Navigate();
        }

        /// <summary>
        /// Shows the change can page
        /// </summary>
        public void ChangeCan()
        {
            _navigationService.UriFor<ChooseBottleViewModel>().Navigate();
        }

        /// <summary>
        /// Shows the daily detail page
        /// </summary>
        public void ShowTodaysDetail()
        {
            _navigationService.UriFor<TodayDetailViewModel>().Navigate();
        }

        /// <summary>
        /// Toggle if background notifications are enabled or disabled
        /// </summary>
        public void ToggleNotifications(){
            if (GlobalSettings.Instance.NotificationsEnabled)
            {
                GlobalSettings.Instance.NotificationsEnabled = false;
            }
            else
            {
                GlobalSettings.Instance.NotificationsEnabled = true;
            }

            NotifyOfPropertyChange(() => NotificationsEnabled);

        }


        public void ShowBtleTestScreen()
        {
            _navigationService.Navigate(typeof(BtleTestScreen));
        }

        #endregion

    }
}
