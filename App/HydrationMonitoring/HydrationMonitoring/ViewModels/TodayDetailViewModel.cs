using Caliburn.Micro;
using HydrationMonitoring.Models;
using HydrationMonitoring.Modules;
using HydrationMonitoring.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrationMonitoring.ViewModels
{
    public class TodayDetailViewModel:ViewModelBase
    {
        public ObservableCollection<DrinkStamp> Drinks { get; set; }
        public ObservableCollection<Workout> Workouts { get; set; }

        private DrinkStamp _selectedDrink;

        public DrinkStamp SelectedDrink
        {
            get { return _selectedDrink; }
            set { _selectedDrink = value; NotifyOfPropertyChange(() => SelectedDrink); }
        }

        private Workout _selectedWorkout;

        public Workout SelectedWorkout
        {
            get { return _selectedWorkout; }
            set { _selectedWorkout = value; NotifyOfPropertyChange(() => SelectedWorkout); }
        }

        public TodayDetailViewModel(INavigationService nav)
            : base(nav)
        {

            Drinks = new ObservableCollection<DrinkStamp>(GlobalSettings.Instance.TodaysHydration.Drinks);
            Workouts = new ObservableCollection<Workout>(GlobalSettings.Instance.TodaysHydration.Workouts);
            _navigationService.BackPressed += _navigationService_BackPressed;
        }

        /// <summary>
        /// Removes a drink - just temporary on the UI
        /// </summary>
        public void RemoveDrink()
        {
            Drinks.Remove(SelectedDrink);
        }

        /// <summary>
        /// Removes a workout - just temporary on the UI
        /// </summary>
        public void RemoveWorkout()
        {
            Workouts.Remove(SelectedWorkout);
        }

        /// <summary>
        /// Saves that drinks and workouts have been removed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _navigationService_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            _navigationService.BackPressed -= _navigationService_BackPressed;

            GlobalSettings.Instance.TodaysHydration.Drinks = Drinks.ToList();
            GlobalSettings.Instance.TodaysHydration.Workouts = Workouts.ToList();
            GlobalSettings.Instance.SaveDailyHydration();

            HydrationCalculation.Instance.UpdateHydration();
        }

    }
}
