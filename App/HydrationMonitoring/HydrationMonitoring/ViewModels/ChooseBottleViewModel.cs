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
    public class ChooseBottleViewModel:ViewModelBase
    {
        public List<SensorCan> SensorCans { get; set; }

        private SensorCan _selectedSensorCan;

        public SensorCan SelectedSensorCan
        {
            get { return _selectedSensorCan; }
            set {
                _selectedSensorCan = value;
                _selectedSensorCan.IsSelected = true;
                
                NotifyOfPropertyChange(() => SelectedSensorCan);
            }
        }
        

        public ChooseBottleViewModel(INavigationService nav)
            : base(nav)
        {
            SensorCans = SensorCan.AllSensorCans;

        }

        protected override void OnActivate()
        {
            base.OnActivate();
            SensorCans.Apply(s => s.IsSelected = false);
            int index = GlobalSettings.Instance.SelectedCanIndex;
            SelectedSensorCan = SensorCans[index];
        }

        /// <summary>
        /// Sets the selected can as the current active one
        /// for any sensor calculations in the future
        /// </summary>
        public void CanSelected()
        {
            var sel = SelectedSensorCan;
            GlobalSettings.Instance.SelectedCanIndex = SensorCans.IndexOf(sel);
            
            _navigationService.GoBack();
        }

    }
}
