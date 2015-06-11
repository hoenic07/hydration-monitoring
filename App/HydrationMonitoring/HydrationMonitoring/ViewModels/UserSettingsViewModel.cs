using Caliburn.Micro;
using HydrationMonitoring.Models;
using HydrationMonitoring.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrationMonitoring.ViewModels
{
    public class UserSettingsViewModel : ViewModelBase
    {

        private int _height;

        public int Height
        {
            get { return _height; }
            set { _height = value; NotifyOfPropertyChange(); }
        }

        private int _weight;

        public int Weight
        {
            get { return _weight; }
            set { _weight = value; NotifyOfPropertyChange(); }
        }

        private TimeSpan _sleepTime;

        public TimeSpan SleepTime
        {
            get { return _sleepTime; }
            set { _sleepTime = value; NotifyOfPropertyChange(); }
        }

        private TimeSpan _wakeUpTime;

        public TimeSpan WakeUpTime
        {
            get { return _wakeUpTime; }
            set { _wakeUpTime = value; NotifyOfPropertyChange(); }
        }

        private int sweatRate;

        public int SweatRate
        {
            get { return sweatRate; }
            set { sweatRate = value; NotifyOfPropertyChange();
            NotifyOfPropertyChange(() => SweatRateString);
            }
        }

        public string SweatRateString
        {
            get
            {
                if (SweatRate <= 3)
                {
                    return "LIGHT";
                }
                else if (SweatRate <= 6)
                {
                    return "MODERATE";
                }
                else return "HEAVY";
            }
        }

        private int _fluidInMeals;

        public int FluidInMeals
        {
            get { return _fluidInMeals; }
            set
            {
                _fluidInMeals = value;
                NotifyOfPropertyChange();
                NotifyOfPropertyChange(() => FluidInMealsString);
            }
        }

        public string FluidInMealsString
        {
            get
            {
                if (FluidInMeals <= 3)
                {
                    return "LOW";
                }
                else if (FluidInMeals <= 6)
                {
                    return "MEDIUM";
                }
                else return "HIGH";
            }
        }

        private bool _hasFirstMeal;

        public bool HasFirstMeal
        {
            get { return _hasFirstMeal; }
            set { _hasFirstMeal = value; NotifyOfPropertyChange(); }
        }

        private bool _hasSecondMeal;

        public bool HasSecondMeal
        {
            get { return _hasSecondMeal; }
            set { _hasSecondMeal = value; NotifyOfPropertyChange(); }
        }
        
        
        private TimeSpan _firstMealTime;

        public TimeSpan FirstMealTime
        {
            get { return _firstMealTime; }
            set { _firstMealTime = value; NotifyOfPropertyChange(); }
        }

        private TimeSpan _secondMealTime;

        public TimeSpan SecondMealTime
        {
            get { return _secondMealTime; }
            set { _secondMealTime = value; NotifyOfPropertyChange(); }
        }

        private bool _hasBreakfast;

        public bool HasBreakfast
        {
            get { return _hasBreakfast; }
            set { _hasBreakfast = value; NotifyOfPropertyChange(); }
        }
        
        
        public UserSettingsViewModel(INavigationService _nav)
            : base(_nav)
        {
            SleepTime = new TimeSpan(22, 0, 0);
            WakeUpTime = new TimeSpan(7, 0, 0);
            Height = 175;
            Weight = 75;
            SweatRate = 5;
            FluidInMeals = 5;
            FirstMealTime = new TimeSpan(12, 0, 0);
            SecondMealTime = new TimeSpan(18, 0, 0);
            HasBreakfast = true;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            if (GlobalSettings.Instance.User.WeightInKg != 0)
            {
                var user = GlobalSettings.Instance.User;
                SleepTime = user.SleepTime;
                WakeUpTime = user.WakeUpTime;
                Weight = user.WeightInKg;
                Height = user.HeightInCm;
                HasFirstMeal = user.FirstMealTime != TimeSpan.MaxValue;
                HasSecondMeal = user.SecondMealTime != TimeSpan.MaxValue;
                FirstMealTime = user.FirstMealTime == TimeSpan.MaxValue ? new TimeSpan(12, 0, 0) : user.FirstMealTime;
                SecondMealTime = user.SecondMealTime == TimeSpan.MaxValue ? new TimeSpan(18, 0, 0) : user.SecondMealTime;
                HasBreakfast = user.HasBreakfast;
            }
        }

        /// <summary>
        /// Save the defined settings
        /// </summary>
        public void SaveUser()
        {
            User user = new User
            {
                SleepTime = SleepTime,
                WakeUpTime = WakeUpTime,
                WeightInKg = Weight,
                HeightInCm = Height,
                SweatRate = SweatRate,
                FluidInMeals = FluidInMeals,
                FirstMealTime = HasFirstMeal ? FirstMealTime : TimeSpan.MaxValue,
                SecondMealTime = HasSecondMeal ? SecondMealTime : TimeSpan.MaxValue,
                HasBreakfast=HasBreakfast
                
            };
            GlobalSettings.Instance.User = user;

            if (_navigationService.BackStack.Count > 0)
            {
                _navigationService.GoBack();
            }
            else
            {
                _navigationService.UriFor<MainViewModel>().Navigate();
                _navigationService.BackStack.Clear();
            }

        }


    }
}
