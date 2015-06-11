using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace HydrationMonitoring.Utils
{
    public static class Tools
    {
        /// <summary>
        /// Converts a hexcode to a brush object
        /// </summary>
        /// <param name="hexCode"></param>
        /// <returns></returns>
        public static Brush ToBrush(this string hexCode)
        {
            var color = new Color();
            color.A = byte.Parse(hexCode.Substring(1, 2), NumberStyles.AllowHexSpecifier);
            color.R = byte.Parse(hexCode.Substring(3, 2), NumberStyles.AllowHexSpecifier);
            color.G = byte.Parse(hexCode.Substring(5, 2), NumberStyles.AllowHexSpecifier);
            color.B = byte.Parse(hexCode.Substring(7, 2), NumberStyles.AllowHexSpecifier);
            return new SolidColorBrush(color);
        }

    }
}
