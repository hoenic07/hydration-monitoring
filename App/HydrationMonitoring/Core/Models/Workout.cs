using HydrationMonitoring.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrationMonitoring.Models
{
    public class Workout
    {

        /// <summary>
        /// 0 (easy) - 30 (extreme intense)
        /// </summary>
        public int Intensity { get; set; }

        public DateTime StartTime { get; set; }

        public int Temperature { get; set; }

        public int Duration { get; set; }

        public bool IsIndoor { get; set; }

        /// <summary>
        /// Calculates the deyhdration during the workout until a specific time
        /// Using the formula from http://www.camelbak.com/HydrationCalculator
        /// </summary>
        /// <param name="time">date until the dehydration should be calculated</param>
        /// <param name="user">user with weight and height for body surface area</param>
        /// <returns></returns>
        public double CalculateDehydrationUntil(DateTime time, User user)
        {
            var minutesSinceStart = (time - StartTime).TotalMinutes;
            var duration = Duration;

            if (minutesSinceStart <= 0) return 0;
            else if (minutesSinceStart < duration)
            {
                duration = (int)minutesSinceStart;
            }

            var bsa = Math.Sqrt(user.HeightInCm * user.WeightInKg / 3600);
            var temp = GetTemperatureContribution();

            var volume = ((bsa * 24 / 2.57) + Intensity + temp + user.SweatRate + 3) * (duration * 5 / 190.0) / 100;

            // get value in liters
            return volume;
        }

        /// <summary>
        /// That total dehydration that comes up during the workout
        /// Same as CalculateDehydrationUntil
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public double CalculateTotalDehydration(User user)
        {
            return CalculateDehydrationUntil(DateTime.MaxValue, user);
        }

        public string FormatTotalHydration
        {
            get { return string.Format("{0} ml",(int)((CalculateTotalDehydration(GlobalSettings.Instance.User)*1000)));}
        }

        public string FormatTime
        {
            get { return string.Format("{0}, Duration: {1} Min.", StartTime.ToString("HH:mm"), Duration); }
        }

        public int GetTemperatureContribution()
        {
            var vals = new int[,]{
                {-100, -9, 12},
                {-9, -7, 10},
                {-7, -4, 8},
                {-4, -1, 6},
                {-1, 2, 4},
                {2, 4, 2},
                {4, 16, 0},
                {16, 18, 3},
                {18, 21, 6},
                {21, 27, 9},
                {27, 29, 15},
                {29, 32, 18},
                {32, 35, 21},
                {35, 38, 24},
                {38, 41, 27},
                {41, 100, 30},
            };

            for (int i = 0; i < vals.GetLength(0); i++)
            {
                var min = vals[i,0];
                var max = vals[i, 1];
                var contrib = vals[i, 2];

                if (Temperature >= min && Temperature < max)
                {
                    return contrib;
                }
            }

            return 0;
        }

    }
}
