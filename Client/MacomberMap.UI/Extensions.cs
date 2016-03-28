using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MacomberMap.UI
{
    public static class Extensions
    {
        public static void SetDoubleBuffered(System.Windows.Forms.Control c)
        {
            //Taxes: Remote Desktop Connection and painting
            //http://blogs.msdn.com/oldnewthing/archive/2006/01/03/508694.aspx
            if (System.Windows.Forms.SystemInformation.TerminalServerSession)
                return;

            System.Reflection.PropertyInfo aProp =
                  typeof(System.Windows.Forms.Control).GetProperty(
                        "DoubleBuffered",
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Instance);

            aProp.SetValue(c, true, null);
        }


        #region RP Status Strings and Colors
        public static string RpStatusToText(int rpstatuscode)
        {
            string statusText = string.Empty;

            switch (rpstatuscode)
            {
                case 0: statusText = "Unavailable"; break;
                case 1: statusText = "Manual"; break;
                case 2: statusText = "Self Scheduled"; break;
                case 3: statusText = "Available"; break;
                case 4: statusText = "Supplemental"; break;
                case 5: statusText = "Quick Start"; break;
                case 6: statusText = "Intermittent"; break;
                case 7: statusText = "Startup"; break;
                case 8: statusText = "Shutdown"; break;
                case 9: statusText = "Testing"; break;
                case 10: statusText = "QualifyingFacility"; break;
                case 11: statusText = "ExigentConditions"; break;
                default: statusText = "UNKNOWN"; break;
            }

            return statusText;
        }

        public static System.Drawing.Color RpStatusTextToColor(string rpstatus)
        {
            System.Drawing.Color statusColor = System.Drawing.Color.White;

            switch (rpstatus)
            {
                case "Unavailable": statusColor = System.Drawing.Color.FromArgb(255, 128, 128); break;
                case "Manual": statusColor = System.Drawing.Color.FromArgb(255, 192, 128); break;
                case "Self Scheduled": statusColor = System.Drawing.Color.FromArgb(192, 192, 255); break;
                case "Available": statusColor = System.Drawing.Color.FromArgb(192, 255, 192); break;
                case "Supplemental": statusColor = System.Drawing.Color.FromArgb(255, 255, 192); break;
                case "Quick Start": statusColor = System.Drawing.Color.FromArgb(192, 255, 192); break;
                case "Intermittent": statusColor = System.Drawing.Color.FromArgb(255, 192, 128); break;
                case "Startup": statusColor = System.Drawing.Color.FromArgb(255, 192, 128); break;
                case "Shutdown": statusColor = System.Drawing.Color.FromArgb(255, 192, 128); break;
                case "Testing": statusColor = System.Drawing.Color.FromArgb(255, 192, 128); break;
                case "QualifyingFacility": statusColor = System.Drawing.Color.FromArgb(255, 192, 128); break;
                case "ExigentConditions": statusColor = System.Drawing.Color.FromArgb(255, 192, 128); break;
                default: statusColor = System.Drawing.Color.White; break;
            }

            return statusColor;
        }

        public static System.Drawing.Color FuelTextToColor(string fuel)
        {
            return FuelTextToColor(fuel, false);
        }

        public static System.Drawing.Color FuelTextToColor(string fuel, bool useFullColor)
        {
            System.Drawing.Color retColor = System.Drawing.Color.White;

            if (useFullColor)
            {
                switch (fuel.ToLower())
                {
                    case "coal": retColor = System.Drawing.Color.Gray; break;
                    case "gas": retColor = System.Drawing.Color.Orange; break;
                    case "nuc": retColor = System.Drawing.Color.Yellow; break;
                    case "hydr": retColor = System.Drawing.Color.Blue; break;
                    case "wind": retColor = System.Drawing.Color.LightBlue; break;
                    case "dfo": retColor = System.Drawing.Color.Red; break;
                    default: retColor = System.Drawing.Color.White; break;
                }
            }
            else
            {
                switch (fuel.ToLower())
                {
                    case "coal": retColor = System.Drawing.Color.FromArgb(224, 224, 224); break;
                    case "gas": retColor = System.Drawing.Color.FromArgb(255, 255, 192); break;
                    case "nuc": retColor = System.Drawing.Color.FromArgb(255, 224, 192); break;
                    case "hydr": retColor = System.Drawing.Color.FromArgb(192, 192, 255); break;
                    case "wind": retColor = System.Drawing.Color.FromArgb(192, 255, 255); break;
                    case "dfo": retColor = System.Drawing.Color.FromArgb(255, 192, 192); break;
                    default: retColor = System.Drawing.Color.White; break;
                }
            }

            return retColor;
        }

        public static System.Drawing.Color SelectedColor(this System.Drawing.Color normalColor)
        {
            HSLColor c = new HSLColor(normalColor);

            if (c.Luminosity > 128)
                c.Luminosity = c.Luminosity - 32;
            else
                c.Luminosity = c.Luminosity + 32;

            return (System.Drawing.Color)c;
        }

        public static System.Drawing.Color TextColor(this System.Drawing.Color backgroundColor)
        {
            HSLColor c = new HSLColor(backgroundColor);

            if (c.Luminosity > 100)
                return System.Drawing.Color.Black;
            else
                return System.Drawing.Color.White;
        }
        #endregion
    }
}
