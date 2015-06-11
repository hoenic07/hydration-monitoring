using HydrationMonitoring.Networking;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace HydrationMonitoring.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BtleTestScreen : Page
    {
        BtleSensorService _sensor;
        List<BtleAccelerometerReading> _readings;

        public BtleTestScreen()
        {
            this.InitializeComponent();
            _sensor = BtleSensorService.Instance;
            
        }

        private async void OnConnect(object sender, RoutedEventArgs e)
        {
            await _sensor.InitDevice();
            
            if (_sensor.Device != null)
            {
                tbLog.Text = "Connected";
            }
            else tbLog.Text = "Failed";
            
        }

        int i = 0;

        void _sensor_BtleAccelerometerReadingUpdated(BtleSensorService sender, BtleAccelerometerReading args)
        {

            _readings.Add(args);

            string r = string.Format("\n{0}\n{1}\n{2}", args.X, args.Y, args.Z);
            Debug.WriteLine(r);
            i++;
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                tbLog.Text = i+" "+r;
            });
        }

        private async void OnStartStop(object sender, RoutedEventArgs e)
        {
            if (_readings == null)
            {
                _readings = new List<BtleAccelerometerReading>();
                (sender as Button).Content = "Stop";
                _sensor.BtleAccelerometerReadingUpdated += _sensor_BtleAccelerometerReadingUpdated;
            }
            else
            {
                _sensor.BtleAccelerometerReadingUpdated -= _sensor_BtleAccelerometerReadingUpdated;
                if (_readings.Count > 0)
                {
                    //Save to file
                    var date =DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss");
                    var personId=tbPersonId.Text;
                    if (string.IsNullOrEmpty(personId)) personId = "0";
                    var name = string.Format("{0}_{1}_{2}_{3}.txt", date, personId, cbTest.SelectedIndex + 1, cbContainer.SelectedIndex + 1);
                    var res = await SaveReadingsToFile(name);
                    if (res) tbLog.Text = "Saved to " + name;
                    else tbLog.Text = "Failed";
                }
                _readings = null;
                (sender as Button).Content = "Start";
                
            }
        }


        public async Task<bool> SaveReadingsToFile(string name)
        {
            try
            {
                var r = KnownFolders.RemovableDevices;
                var folders = await r.GetFoldersAsync();
                if (folders.Count == 0) return false;
                var f = await folders[0].CreateFolderAsync("HydrationMonitoring", CreationCollisionOption.OpenIfExists);
                var file = await f.CreateFileAsync(name);

                StreamWriter sw = new StreamWriter(await file.OpenStreamForWriteAsync());
                foreach (var item in _readings)
                {
                    var line = string.Format("{0},{1},{2}", item.X, item.Y, item.Z);
                    sw.WriteLine(line);
                }

                sw.Flush();
                sw.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
