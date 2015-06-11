using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrationMonitoring.Models
{
    public enum DrinkType { Default, Sensor, Breakfast, FirstMeal, SecondMeal }
    public class DrinkStamp
    {
        public double Volume { get; set; }
        public DateTime Time { get; set; }

        public DrinkType DrinkType { get; set; }

        public bool IsMeal { get; set; }

        public string FormatTime
        {
            get
            {
                return Time.ToString("HH:mm")+" "+NameForDrinkType();
            }
        }

        public string FormatName
        {
            get { return Volume * 1000 + " ml"; }
        }

        public string NameForDrinkType()
        {
            switch (DrinkType)
            {
                case DrinkType.Default:
                    return string.Empty;
                case DrinkType.Sensor:
                    return "(Sensor)";
                case DrinkType.Breakfast:
                    return "(Breakfast)";
                case DrinkType.FirstMeal:
                    return "(First meal)";
                case DrinkType.SecondMeal:
                    return "(Second meal)";
                default:
                    return string.Empty;
            }
        }

    }

    public class SensorDrinkStamp : DrinkStamp
    {
        public SensorDrinkStamp()
        {
            SensorValues = new List<double>();
            SmoothValues = new List<double>();
            DrinkType = DrinkType.Sensor;
        }

        public List<double> SensorValues { get; set; }
        public List<double> SmoothValues { get; set; }
    }
}
