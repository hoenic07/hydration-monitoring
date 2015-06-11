using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrationMonitoring.Models
{
    public class User
    {
        public int ID { get; set; }

        public int HeightInCm { get; set; }
        public int WeightInKg { get; set; }
        public TimeSpan SleepTime { get; set; }
        public TimeSpan WakeUpTime { get; set; }

        public int SweatRate { get; set; }

        public double FluidInMeals { get; set; }

        public bool HasBreakfast { get; set; }

        public TimeSpan FirstMealTime { get; set; }

        public TimeSpan SecondMealTime { get; set; }

        public int SelectedSensorCanIndex { get; set; }
    }
}
