using Caliburn.Micro;
using HydrationMonitoring.ViewModels;
using HydrationMonitoring.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;
using Caliburn;
using HydrationMonitoring.Utils;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace HydrationMonitoring
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : CaliburnApplication
    {
        private WinRTContainer container;

        public App()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initial config for Caliburn
        /// </summary>
        protected override void Configure()
        {
            container = new WinRTContainer();

            container.RegisterWinRTServices();
            container.Singleton<MainViewModel>();
            
            container.PerRequest<UserSettingsViewModel>();
            container.PerRequest<AddCustomDrinkViewModel>();
            container.PerRequest<AddWorkoutViewModel>();
            container.PerRequest<ChooseBottleViewModel>();
            container.PerRequest<TodayDetailViewModel>();

        }

        protected override void PrepareViewFirst(Frame rootFrame)
        {
            container.RegisterNavigationService(rootFrame);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Show user settings view if no user defined yet
            if (GlobalSettings.Instance.User.WeightInKg == 0)
            {
                DisplayRootView<UserSettingsView>();
            }
            else DisplayRootView<MainView>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }
    }
}