using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrationMonitoring.ViewModels
{
    public class ViewModelBase : Screen
    {
        protected INavigationService _navigationService;

        public ViewModelBase(INavigationService nav)
        {
            _navigationService = nav;
        }

    }
}
