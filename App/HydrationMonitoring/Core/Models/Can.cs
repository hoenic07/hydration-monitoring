using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrationMonitoring.Models
{
    public class Can
    {
        public Can() { }

        public Can(string imgName, double volume)
        {
            ImageSource = "ms-appx:///Assets/cans/" + imgName + ".png";
            Volume = volume;
        }

        public double Volume { get; set; }

        public string ImageSource { get; set; }
        public string Text
        {
            get
            {
                if (Volume == -1) return "Other";
                else return Volume + "l";
            }
        }

        public static List<Can> AvailableStaticCans = new List<Can>()
            {
                new Can("0_25",0.25),
                new Can("0_33",0.33),
                new Can("0_5",0.5),
                new Can("0_75",0.75),
                new Can("other",-1)
            };
    }

    public class SensorCan : Can
    {

        public int SampleRate { get; set; }

        public double DrinkThreshold { get; set; }

        public double MultiplyFactor { get; set; }

        public string Name { get; set; }

        public bool IsSelected { get; set; }

        // A list of all cans that are currently available
        public static List<SensorCan> AllSensorCans = new List<SensorCan>()
        {
            new SensorCan
            {
                Name="Aladdin",
                Volume = 0.6,
                SampleRate = 10,
                MultiplyFactor = 1.11,
                DrinkThreshold=-0.18,
            },
            new SensorCan
            {
                Name="Red Bull",
                Volume = 0.25,
                SampleRate = 10,
                MultiplyFactor = 1.11,
                DrinkThreshold=-0.6,
            },
            new SensorCan
            {
                Name="Tea Cup",
                Volume = 0.25,
                SampleRate = 10,
                MultiplyFactor = 1.11,
                DrinkThreshold=-0.75,
            },
        };

        public string DisplayedName
        {
            get { return string.Format("{0} - {1} ml", Name, (int)(Volume * 1000)); }
        }

    }
}
