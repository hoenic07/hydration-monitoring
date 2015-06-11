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
    public class AddCustomDrinkViewModel : ViewModelBase
    {

        private double _value;

        public double Value
        {
            get { return _value; }
            set
            {
                _value = value; NotifyOfPropertyChange(() => Value);
                NotifyOfPropertyChange(() => ValueString);
            }
        }

        public string ValueString
        {
            get { return string.Format("{0:0.00}", Value); }
        }

        private TimeSpan _time;

        public TimeSpan Time
        {
            get { return _time; }
            set { _time = value; NotifyOfPropertyChange(); }
        }
        

        public AddCustomDrinkViewModel(INavigationService _nav)
            : base(_nav)
        {
            Value = 1;
            Time = DateTime.Now.TimeOfDay;
        }

        /// <summary>
        /// Add a custom drink to the current hydration
        /// </summary>
        public void AddDrink()
        {
            DrinkStamp st = new DrinkStamp { Volume = Value, Time = DateTime.Today + Time };
            MessageCenter.Instance.Publish(MessageKind.AddDrink, st);
            _navigationService.GoBack();
        }

    }
}
