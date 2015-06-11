using Caliburn.Micro;
using HydrationMonitoring.Models;
using HydrationMonitoring.Modules;
using HydrationMonitoring.Networking;
using HydrationMonitoring.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace HydrationMonitoring.ViewModels
{
    public class AddWorkoutViewModel : ViewModelBase
    {

        private int _intensity;

        public int Intensity
        {
            get { return _intensity; }
            set
            {
                _intensity = value; NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => IntensityString);
                PrecalculateDehydration();
            }
        }

        public string IntensityString
        {
            get
            {
                if (Intensity < 8) return "LOW";
                else if (Intensity < 16) return "MEDIUM";
                else if (Intensity < 24) return "HIGH";
                else return "VERY HIGH";
            }
        }

        private int _duration;

        public int Duration
        {
            get { return _duration; }
            set
            {
                _duration = value; NotifyOfPropertyChange(); NotifyOfPropertyChange(() => DurationString);
                PrecalculateDehydration();
            }
        }

        public string DurationString
        {
            get
            {
                return string.Format("{0:00}:{1:00}", Duration / 60, Duration % 60);
            }
        }

        private TimeSpan _startTime;

        public TimeSpan StartTime
        {
            get { return _startTime; }
            set { _startTime = value; NotifyOfPropertyChange();
            NotifyOfPropertyChange(() => TimeString);
            }
        }

        public string TimeString
        {
            get { return string.Format("{0:00}:{1:00}", StartTime.Hours, StartTime.Minutes); }
        }

        private bool _isIndoor;

        public bool IsIndoor
        {
            get { return _isIndoor; }
            set { _isIndoor = value; NotifyOfPropertyChange(); PrecalculateDehydration(); }
        }

        private int _causedDehydration;

        public int CausedDehydration
        {
            get { return _causedDehydration; }
            set { _causedDehydration = value; NotifyOfPropertyChange(() => CausedDehydration); }
        }
        


        private Page _view;
        

        public AddWorkoutViewModel(INavigationService _nav)
            : base(_nav)
        {

        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            _view = (Page)view;
            StartTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute / 5 * 5 + 10, 0);
            Duration = 45;
            Intensity = 10;
        }

        /// <summary>
        /// Sets the time via a flyout
        /// </summary>
        public async void SetTime()
        {
            var tp = new TimePickerFlyout();
            tp.Time = StartTime;
            tp.MinuteIncrement = 5;
            var res = await tp.ShowAtAsync(_view);
            if (res == null) return;
            StartTime = res.Value;
        }

        /// <summary>
        /// Saves the workout to the current hydration
        /// </summary>
        public void SaveWorkout()
        {

            var temp = 24;

            if (WeatherApiConnection.LastTemperature != -1000)
            {
                temp = (int)WeatherApiConnection.LastTemperature;
            }

            Workout wo = new Workout()
            {
                Duration = Duration,
                StartTime = DateTime.Today + StartTime,
                Intensity = Intensity,
                Temperature = temp,
                IsIndoor = IsIndoor
            };
            GlobalSettings.Instance.TodaysHydration.Workouts.Add(wo);
            GlobalSettings.Instance.SaveDailyHydration();
            HydrationCalculation.Instance.UpdateHydration();

            _navigationService.GoBack();
        }

        /// <summary>
        /// Calculates the dehydration for the current defined parameters
        /// and updates the UI in real time after changes
        /// </summary>
        public void PrecalculateDehydration()
        {
            var temp = 24;

            if (WeatherApiConnection.LastTemperature != -1000)
            {
                temp = (int)WeatherApiConnection.LastTemperature;
            }

            if (IsIndoor) temp = 24;

            Workout wo = new Workout()
            {
                Duration = Duration,
                StartTime = DateTime.Today + StartTime,
                Intensity = Intensity,
                Temperature = temp,
                IsIndoor = IsIndoor
            };

            CausedDehydration = (int)(wo.CalculateTotalDehydration(GlobalSettings.Instance.User)*1000);
        }

    }
}
