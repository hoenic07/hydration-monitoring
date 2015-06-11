using HydrationMonitoring.Models;
using HydrationMonitoring.Networking;
using HydrationMonitoring.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace HydrationMonitoring.Modules
{
    public class SensorVolumeCalculation
    {

        private static SensorVolumeCalculation _instance;

        public static SensorVolumeCalculation Instance
        {
            get
            {
                if (_instance == null) _instance = new SensorVolumeCalculation();
                return _instance;
            }
        }

        private SensorVolumeCalculation()
        {
            Readings = new List<double>();

            var index = GlobalSettings.Instance.SelectedCanIndex;
            CurrentCan = SensorCan.AllSensorCans[index];
            
            MessageCenter.Notifications.Register(this, NotificationKind.SensorCanChanged, () =>
            {
                var id = GlobalSettings.Instance.SelectedCanIndex;
                CurrentCan = SensorCan.AllSensorCans[id];
            });

            MonitorSensorActivity();

        }

        public SensorCan CurrentCan { get; set; }

        public List<double> Readings { get; set; }
        public SensorDrinkStamp CurrentDrink { get; set; }

        public event TypedEventHandler<object, SensorDrinkStamp> DrinkDetected;

        public event TypedEventHandler<object, bool> SensorAvailabilityChanged;

        public DateTime LastValue { get; set; }
        public bool CurrentAvailabilityState { get; set; }
        public bool AvailabilityMonitoringActive { get; set; }

        /// <summary>
        /// New sensor values arrive and will be added to the readings
        /// if there are at least 11 values the edge detection starts
        /// </summary>
        /// <param name="service"></param>
        /// <param name="args"></param>
        public void SensorValueUpdate(BtleSensorService service, BtleAccelerometerReading args)
        {
            Readings.Add(args.Y);
            LastValue = DateTime.Now;

            if (Readings.Count < 11) return;

            // only detect edges every 500ms
            if (Readings.Count % 5 == 0)
            {
                DetectEdges();
            } 
        }

        /// <summary>
        /// Live detection of edges and further detection of drinking actions
        /// If values were found the drinking amount will be calculated
        /// </summary>
        private void DetectEdges()
        {
            var smooth = SmoothedValues(Readings, 5);
            var validSmooth = smooth.Range(5, smooth.Count - 5).ToList();
            bool hasRising = false;
            int riseIndex = -1;
            int fallIndex = -1;

            for (int i = 1; i < validSmooth.Count; i++)
            {
                if (!hasRising && validSmooth[i] > CurrentCan.DrinkThreshold && validSmooth[i - 1] < CurrentCan.DrinkThreshold)
                {
                    riseIndex = i;
                    hasRising = true;
                }
                if (hasRising && validSmooth[i] < CurrentCan.DrinkThreshold && validSmooth[i - 1] > CurrentCan.DrinkThreshold)
                {
                    fallIndex = i;
                }
            }

            if (riseIndex != -1 && fallIndex != -1)
            {
                var drink = new SensorDrinkStamp
                {
                    Time = DateTime.Now,
                    SensorValues = Readings.Range(5 + riseIndex, 5 + fallIndex).ToList(),
                    SmoothValues = validSmooth.Range(riseIndex, fallIndex).ToList()
                };
                AnalyzeDrinkingActions(drink);
                Readings.RemoveRange(0, Readings.Count - 5);
            }
            else if (riseIndex == -1)
            {
                Readings.RemoveRange(0, Readings.Count - 11);
            }
        }

        /// <summary>
        /// base method for calculating the drinking amount
        /// here the parameter calculation and the volume calculation will be called
        /// </summary>
        /// <param name="drink"></param>
        private void AnalyzeDrinkingActions(SensorDrinkStamp drink)
        {
            drink.Time = DateTime.Now;
            var p = GetParametersFromRawValues(drink.SensorValues, drink.SmoothValues);
            var volume = CalculateVolumeByParametersMatlab(p);
            drink.Volume = ((int)volume) / 1000.0;

            if (p.Any(a => double.IsNaN(a)))
            {
                return;
                //AnalyzeDrinkingActions(drink);
            }

            drink.SmoothValues.Clear();
            drink.SensorValues.Clear();
            if (DrinkDetected != null)
            {
                DrinkDetected(this, drink);
            }
        }

        /// <summary>
        /// Calculates the parameters from the raw values and the smoothed values
        /// </summary>
        /// <param name="values">raw values</param>
        /// <param name="smoothVals">smooth values</param>
        /// <returns>the parameters that are used for CalculateVolumeByParameters2</returns>
        private List<double> GetParametersFromRawValues(List<double> values, List<double> smoothVals)
        {
            var l = values.Count;
            var angles = values.Select(v => Math.Asin(v * CurrentCan.MultiplyFactor) * (180 / Math.PI)).ToList();
            var smoothAngles = SmoothedValues(angles, 5);

            var smoothDistance = 0.0;
            for (int i = 0; i < l - 1; i++)
            {
                smoothDistance += Math.Sqrt(Math.Pow((smoothAngles[i] - smoothAngles[i + 1]), 2) + 1);
            }

            var variance = 0.0;
            for (int i = 0; i < l; i++)
            {
                variance += Math.Abs(values[i] - smoothVals[i]);
            }

            var center = l / 2.0 != l / 2 ? (l / 2) + 1 : l / 2;
            var fh = smoothAngles.Take(center).ToList();
            var fh1 = fh.Sum() / (center);
            var sh = smoothAngles.Reverse<double>().Take(center).ToList();
            var sh2 = sh.Sum() / (center);
            var slopeDuringDrinking = (sh2 - fh1);

            var time = l * (1000 / CurrentCan.SampleRate);

            var ctAngles = angles.Skip(l / 3).Take((l / 3)+1).ToList();
            var mean = ctAngles.Sum() / ctAngles.Count;
            var standardDeviationOfAngles = Math.Sqrt(ctAngles.Select(a => Math.Pow(a - mean,2)).Sum() / (ctAngles.Count-1));

            return new List<double> { 
                variance,
                standardDeviationOfAngles,
                slopeDuringDrinking,
                l,
                0,
                smoothDistance,   
            };
        }

        /// <summary>
        /// Calculates the volume in milliliter
        /// WEKA values. not used currently
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private double CalculateVolumeByParametersWEKA(List<double> p)
        {
            var variance = p[0];
            var standardDeviationOfAngles = p[1];
            var slopeDuringDrinking = p[2];
            var time = p[3];
            var sov = p[4];
            var calculatedAmount = 20*p[5]+ 24.0482 * variance + -1.6765 * standardDeviationOfAngles + 1.7737 * slopeDuringDrinking + 0.0073 * time + -1.5158 * sov + -15.866;
            return calculatedAmount;
        }

        /// <summary>
        /// Calculates the volume in milliliter
        /// Matlab method, currently used
        /// </summary>
        /// <param name="p">array of parameters, need to be in the correct order</param>
        /// <returns>the drinking amount in ml</returns>
        private double CalculateVolumeByParametersMatlab(List<double> p)
        {
            var correctVariance = p[0];
            var stdCA = p[1];
            var diffAng = p[2];
            var len = p[3];
            var sov = p[4];
            var distSmooth = p[5];
            var finalDist = distSmooth - Math.Pow(0.55 * diffAng, 2) + stdCA + 19 * correctVariance; //- sov + 0.1 * len;
            var res = 0.000231420324744535 * finalDist * finalDist + 0.430363891043345 * finalDist - 3.52355689079025;

            if (res < 0)
            {
                p[2] = p[2] / 2;
                return CalculateVolumeByParametersMatlab(p);
            }

            return res;
        }

        /// <summary>
        /// Smooths the values by calculated the average over a given windowsize
        /// around a value
        /// </summary>
        /// <param name="values">the values that should be made smooth</param>
        /// <param name="windowsSize">+/- range to calculate the average</param>
        /// <returns>smooth values</returns>
        private List<double> SmoothedValues(List<double> values, int windowsSize)
        {
            if (values == null) return new List<double>();

            var smoothVals = new List<double>();

            for (int i = 0; i < values.Count; i++)
            {
                int left = windowsSize;
                int right = windowsSize;

                if (i < windowsSize)
                {
                    left = i;
                }
                if (i >= values.Count - windowsSize)
                {
                    right = values.Count - i - 1;
                }

                var vals = values.Range(i-left, i+right+1).ToList();
                var sum = vals.Sum();
                var smoothValues = sum / (right + left + 1);

                smoothVals.Add(smoothValues);
            }

            return smoothVals;
        }

        #region Test Data

        /// <summary>
        /// Not used, only for debugging
        /// </summary>
        public static List<double> TestData = new List<double>(){
            -0.156250000000000,
            0,
            0.0468750000000000,
            0.0781250000000000,
            0.125000000000000,
            0.125000000000000,
            0.125000000000000,
            0.0937500000000000,
            0.0468750000000000,
            -0.0625000000000000,
            -0.140625000000000,

        };

        public static List<double> SmoothTestData = new List<double>()
        {
            -0.177556818181818,
            -0.116477272727273,
            -0.0667613636363636,
            -0.0198863636363636,
            0.0113636363636364,
            0.0255681818181818,
            0.00852272727272727,
            -0.0284090909090909,
            -0.0852272727272727,
            -0.150568181818182,
            -0.213068181818182,
        };

        #endregion

        /// <summary>
        /// Monitors the activity of the sensor
        /// If there is no new sensor value within one second the sensor state is set to inactive in the UI
        /// Will be checked every 2.5 seconds
        /// </summary>
        private void MonitorSensorActivity()
        {
            if (AvailabilityMonitoringActive) return;
            Task t = new Task(async () =>
            {
                AvailabilityMonitoringActive = true;
                while (true)
                {
                    if (CurrentAvailabilityState && DateTime.Now.AddSeconds(-1) > LastValue)
                    {
                        CurrentAvailabilityState = false;
                        if (SensorAvailabilityChanged != null) SensorAvailabilityChanged(this, CurrentAvailabilityState);
                    }

                    else if (!CurrentAvailabilityState && DateTime.Now.AddSeconds(-1) < LastValue)
                    {
                        CurrentAvailabilityState = true;
                        if (SensorAvailabilityChanged != null) SensorAvailabilityChanged(this, CurrentAvailabilityState);
                    }
                    await Task.Delay(2500);
                }

                AvailabilityMonitoringActive = false;
            });

            t.Start();

        }

    }

    public static class LinqEx
    {
        /// <summary>
        /// Extension method for linq. Gets object at a specific range inside an array
        /// Like in matlab
        /// </summary>
        /// <typeparam name="T">type of the list</typeparam>
        /// <param name="list">the list</param>
        /// <param name="begin">begin index</param>
        /// <param name="end">end index</param>
        /// <returns></returns>
        public static IEnumerable<T> Range<T>(this IEnumerable<T> list, int begin, int end)
        {
            return list.Skip(begin).Take(end - begin);
        }
    }
}
