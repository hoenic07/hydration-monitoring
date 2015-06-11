using Caliburn.Micro;
using HydrationMonitoring.Models;
using HydrationMonitoring.Utils;
using HydrationMonitoring.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HydrationMonitoring.Modules
{
    public class HydrationCalculation
    {
        private static HydrationCalculation _instance;

        public static HydrationCalculation Instance
        {
            get
            {
                if (_instance == null) _instance = new HydrationCalculation();
                return _instance;
            }
        }

        public TaskStatus TaskState { get; private set; }
        public TimeSpan UpdateInterval { get; set; }

        private HydrationCalculation()
        {
            UpdateInterval = TimeSpan.FromMinutes(1);
            TaskState = TaskStatus.WaitingForActivation;
        }

        /// <summary>
        /// The task that starts the automatic calculation of the hydration every 1 minute
        /// </summary>
        /// <returns>if the task could have been started, or if it is already running (false)</returns>
        public bool Start()
        {
            if (TaskState == TaskStatus.Running) return false;
            else if (TaskState == TaskStatus.Canceled)
            {
                TaskState = TaskStatus.Running;
                return true;
            }

            TaskState = TaskStatus.Running;

            Task.Run(async () =>
            {
                while (TaskState==TaskStatus.Running)
                {
                    UpdateHydration();
                    await Task.Delay(UpdateInterval);
                }
                TaskState = TaskStatus.WaitingForActivation;
            });

            return true;
        }

        /// <summary>
        /// Stops the automatic calculation
        /// Currently not used
        /// </summary>
        public void Stop()
        {
            TaskState = TaskStatus.Canceled;
        }


        /// <summary>
        /// Calculates the current hydration according to all available information like
        /// -user height, weight, sleep- and eat times etc
        /// -drinking amounts
        /// -entered workouts
        /// </summary>
        /// <param name="isInBackground">if it is called from background no notification needs to be sended at the end</param>
        /// <returns></returns>
        public async Task UpdateHydration(bool isInBackground=false)
        {
            var user = GlobalSettings.Instance.User;
            if (user.WeightInKg == 0) return;

            if (GlobalSettings.Instance.TodaysHydration.TenMinuteHistory.Count == 0)
            {
                await GlobalSettings.Instance.LoadDailyHydration();
            }

            var hydration = GlobalSettings.Instance.TodaysHydration;
            

            if (hydration.Date != DateTime.Today||hydration.DehydrationPerMinute==0)
            {
                //Do something with the old hydration object...

                //Init new day
                var endValue = hydration.TenMinuteHistory.LastOrDefault();
                var lastDayDrinkVolume = hydration.TotalLitersToday;

                //Create new hydration object for today
                hydration = new DailyHydration();

                if (lastDayDrinkVolume > 0)
                {
                    if (endValue < -0.5)
                    {
                        hydration.StartValue = -0.5;
                    }
                    else hydration.StartValue = endValue;

                }

                //Human needs about 30ml per kg body weight. Day has 1440 minutes
                hydration.DehydrationPerMinute = user.WeightInKg * 0.03 / 1440.0;
            }

            // Breakfast
            if (user.HasBreakfast && DateTime.Now.TimeOfDay > user.WakeUpTime && !hydration.Drinks.Any(d => d.DrinkType == DrinkType.Breakfast))
            {
                hydration.Drinks.Add(new DrinkStamp { DrinkType = DrinkType.Breakfast, Time = DateTime.Today + user.WakeUpTime + TimeSpan.FromMinutes(10), Volume = 0.2 });
            }

            // FirstMeal
            if (user.FirstMealTime != TimeSpan.MaxValue && DateTime.Now.TimeOfDay > user.FirstMealTime && !hydration.Drinks.Any(d => d.DrinkType == DrinkType.FirstMeal))
            {
                var m = (user.FluidInMeals / 10.0) * 0.9;
                hydration.Drinks.Add(new DrinkStamp { DrinkType = DrinkType.FirstMeal, Time = DateTime.Now, Volume = m });
            }

            // Second Meal
            if (user.FirstMealTime != TimeSpan.MaxValue && DateTime.Now.TimeOfDay > user.FirstMealTime && !hydration.Drinks.Any(d => d.DrinkType == DrinkType.FirstMeal))
            {
                var m = (user.FluidInMeals / 10.0) * 0.9;
                hydration.Drinks.Add(new DrinkStamp { DrinkType = DrinkType.SecondMeal, Time = DateTime.Now, Volume = m });
            }


            //Calculate current hydration
            var totalIn = hydration.Drinks.Sum(d => d.Volume);
            var idleDehydrationUntilNow = (DateTime.Now - DateTime.Today).TotalMinutes * hydration.DehydrationPerMinute;
            var workoutDehydrationUntilNow = hydration.Workouts.Sum(w => w.CalculateDehydrationUntil(DateTime.Now, user));

            hydration.CurrentHydration = hydration.StartValue - idleDehydrationUntilNow - workoutDehydrationUntilNow + totalIn;

            //Calculate missing 10minute hydration steps
            int stepsUntilNow = (int)(DateTime.Now - DateTime.Today).TotalMinutes / 10;

            for (int i = hydration.TenMinuteHistory.Count; i < stepsUntilNow; i++)
            {
                var dt = DateTime.Today.AddMinutes((i+1) * 10);
                var inUntil = hydration.Drinks.Where(f => f.Time < dt).Sum(f => f.Volume);
                var idle = (dt - DateTime.Today).TotalMinutes * hydration.DehydrationPerMinute;
                var wo = hydration.Workouts.Sum(w => w.CalculateDehydrationUntil(dt, user));
                var dehydration = hydration.StartValue - idle - wo - inUntil;
                hydration.TenMinuteHistory.Add(dehydration);
            }

            // Save to settings
            await GlobalSettings.Instance.SaveDailyHydration(hydration);

            if (isInBackground) return;

            Execute.OnUIThread(() =>
            {
                MessageCenter.Notifications.Publish(NotificationKind.HydrationUpdated);
            });


        }


    }
}
