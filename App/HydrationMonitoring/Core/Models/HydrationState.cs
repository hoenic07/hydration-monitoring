using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using HydrationMonitoring.Utils;


namespace HydrationMonitoring.Models
{
    public class HydrationState
    {
        public HydrationState(string text, Color brush, double from, double to):this(text, new SolidColorBrush(brush), from, to){}

        public HydrationState(string text, Brush brush, double from, double to)
        {
            TextTemplate = text;
            Background = brush;
            FromValue = from;
            ToValue = to;
        }

        public string TextTemplate { get; set; }
        public Brush Background { get; set; }

        public double FromValue { get; set; }
        public double ToValue { get; set; }

        public string Text { get; set; }


        private static List<HydrationState> _states;

        /// <summary>
        /// All current states that can be shown on the main screen
        /// </summary>
        public static void InitStates()
        {
            _states = new List<HydrationState>()
            {
                new HydrationState("You are well hydrated! No need to drink now.",Colors.Transparent, 0.5, 3),
                new HydrationState("You are overhydrated, which should be avoided. Drink not too much the next hours.", "#FFFF9756".ToBrush(), 3, 1000),
                new HydrationState("Consider drinking something within the next hour.", Colors.Transparent, -0.5, 0.5),
                new HydrationState("Dehydration already started. Consider drinking multiple times within the next hour.", "#FFFF6767".ToBrush(), -1.5, -0.5),
                new HydrationState("You are seriously deyhdrated, which is not good for your health!", Colors.Red, -1000, -1.5)
            };
        }

        /// <summary>
        /// Gets the state for the current hydration according to the percentage range that is defined in each state
        /// </summary>
        /// <param name="currentHydrationInPercentage"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static HydrationState StateForCurrentHydration(double currentHydrationInPercentage, params object[] parameter)
        {
            var state = _states.FirstOrDefault(s => s.FromValue <= currentHydrationInPercentage && s.ToValue > currentHydrationInPercentage);
            if (state != null)
            {
                state.Text = string.Format(state.TextTemplate, parameter);
            }
            return state;
        }
    
    
    }


}
