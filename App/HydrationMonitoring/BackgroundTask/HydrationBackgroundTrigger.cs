using HydrationMonitoring.Modules;
using HydrationMonitoring.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;

namespace BackgroundTask
{
    public sealed class HydrationBackgroundTrigger : IBackgroundTask 
    {
        /// <summary>
        /// Shows the toast notifications in a background task. Will be called every 30 minutes
        /// </summary>
        /// <param name="taskInstance"></param>
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral _deferral = taskInstance.GetDeferral();

            try
            {
                await HydrationCalculation.Instance.UpdateHydration();

                var h = GlobalSettings.Instance.TodaysHydration;
                var u = GlobalSettings.Instance.User;
                var time = DateTime.Now.TimeOfDay;

                // No notifications during sleep
                // if user is well hydrated no need to show any notification
                if (time > u.SleepTime || time < u.WakeUpTime || h.CurrentHydration > 0.75 || !GlobalSettings.Instance.NotificationsEnabled)
                {
                    _deferral.Complete();
                    return;
                };

                var dehyInPerc = (h.CurrentHydration / u.WeightInKg * 0.6) * 100;

                // First Meal before notification
                if (u.FirstMealTime != TimeSpan.MaxValue && IsWithinNext30Minutes(u.FirstMealTime, time))
                {
                    ShowToast("Mealtime!", "Drink a glass of water before you eat.", false);
                }

                // Second meal before notification
                if (u.SecondMealTime != TimeSpan.MaxValue && IsWithinNext30Minutes(u.SecondMealTime, time))
                {
                    ShowToast("Mealtime!", "Drink a glass of water before you eat.", false);
                }

                // Before sleep notification
                if (IsWithinNext30Minutes(u.SleepTime, time))
                {
                    ShowToast("Good night!", "Drink a cup of water before going to bed.", false);
                }

                // Next workout notification
                foreach (var w in h.Workouts)
                {
                    if (IsWithinNext30Minutes(w.StartTime.TimeOfDay, time))
                    {
                        var wv = w.CalculateTotalDehydration(u);
                        wv /= 3;
                        if (wv < 250)
                        {
                            wv = 250;
                        }
                        else
                        {
                            wv = ((int)wv / 10) * 10;
                        }
                        ShowToast("Workout!", string.Format("Drink about {0} ml before you begin.", wv),false);
                        break;
                    }
                }

                // General dehydration notification
                if (dehyInPerc < 0)
                {
                    ShowToast("Drink!", "You are about to dehydrate.", true);
                }
            }
            catch (Exception ex)
            {
                ShowToast("Exception!", ex.Message, false);
            }

            _deferral.Complete();
        }

        /// <summary>
        /// Shows a toast notification with a header and text
        /// </summary>
        /// <param name="header"></param>
        /// <param name="text"></param>
        /// <param name="isDehydration">If this is the case any toasts of the dehydration-group will be deleted from the action center</param>
        public void ShowToast(string header, string text, bool isDehydration)
        {
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(header));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode(text));

            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "long");

            ToastNotification toast = new ToastNotification(toastXml);
            if (isDehydration)
            {
                toast.Group = "dehydration";
                ToastNotificationManager.History.RemoveGroup("dehydration");
            }
            
            toast.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(3600);

            ToastNotificationManager.CreateToastNotifier().Show(toast);  
        }

        /// <summary>
        /// Checks if the checkTime is within 30 minutes of the anchorTime
        /// </summary>
        /// <param name="anchorTime"></param>
        /// <param name="checkTime"></param>
        /// <returns></returns>
        private bool IsWithinNext30Minutes(TimeSpan anchorTime, TimeSpan checkTime)
        {
            var minA = anchorTime.TotalMinutes;
            var minC = checkTime.TotalMinutes;
            return minC < minA && minA - 30 < minC;
        }
    }
}
