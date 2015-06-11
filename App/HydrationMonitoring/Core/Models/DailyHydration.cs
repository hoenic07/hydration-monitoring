using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrationMonitoring.Models
{
    public class DailyHydration
    {
        public int ID { get; set; }

        public DateTime Date { get; set; }

        public double StartValue { get; set; }

        public List<double> TenMinuteHistory { get; private set; }

        public double CurrentHydration { get; set; }

        public List<DrinkStamp> Drinks { get; set; }

        public List<Workout> Workouts { get; set; }

        public double TotalLitersToday
        {
            get
            {
                return Drinks.Sum(d => d.Volume);
            }
        }

        /// <summary>
        /// in ml
        /// </summary>
        public double DehydrationPerMinute { get; set; }

        public DailyHydration()
        {
            Drinks = new List<DrinkStamp>();
            Workouts = new List<Workout>();
            TenMinuteHistory = new List<double>();
            Date = DateTime.Today;
        }

    }
}
