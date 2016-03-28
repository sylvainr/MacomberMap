using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace MacomberMapClient.Data_Elements.Weather
{
    /// <summary>
    /// (C) 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved
    /// This class holds the collection of relevant weather stations for a county
    /// </summary>
    public class MM_WeatherStation : MM_Serializable
    {
        #region Variable declarations
        /// <summary>The ID of the weather station</summary>
        public String StationID;

        /// <summary> The title of the weather station</summary>
        public String Description;

        /// <summary>The latitude/longitude of the weather station</summary>
        public PointF LngLat;

        /// <summary>The elevation of the weather site</summary>
        public float Elevation;

        /// <summary>The temperature as recorded by the weather station</summary>
        public float Temperature;

        /// <summary>The relative humidity at the weather station</summary>
        public float Humidity;

        /// <summary>The direction of the wind</summary>
        public string WindDirection;

        /// <summary>The speed of the wind</summary>
        public float WindSpeed;

        /// <summary>Atmospheric pressure</summary>
        public float Pressure;

        /// <summary>The summary of the weather (e.g., overcast, raining)</summary>
        public string Summary;

        /// <summary>The dew point</summary>
        public float Dewpoint;

        /// <summary>The query prepend for the weather station</summary>
        public static string QueryPrepend;
        #endregion

        #region Initialization
        /// <summary>
        /// Initialize a new weather station
        /// </summary>
        /// <param name="xE">The XML Element containing the station's ID and lat/long</param>
        public MM_WeatherStation(XmlElement xE)
        {
            if (xE["icao"] != null)
                this.StationID = xE["icao"].InnerText;
            if (xE["lon"] != null && xE["lat"] != null)
                this.LngLat = new PointF(ConvertToSingle(xE["lon"].InnerText), ConvertToSingle(xE["lat"].InnerText));
            if (xE.HasAttribute("StationID"))
                this.StationID = xE.Attributes["StationID"].Value;
            if (xE.HasAttribute("Description"))
                this.Description = (xE.Attributes["Description"].Value);
            if (xE.HasAttribute("Coordinates"))
                this.LngLat = new PointF(MM_Converter.ToSingle(xE.Attributes["Coordinates"].Value.Split(',')[0]), MM_Converter.ToSingle(xE.Attributes["Coordinates"].Value.Split(',')[1]));
            if (xE.HasAttribute("Elevation"))
                this.Elevation = MM_Converter.ToSingle(xE.Attributes["Elevation"].Value);
            if (xE.HasAttribute("Temperature"))
                this.Temperature = MM_Converter.ToSingle(xE.Attributes["Temperature"].Value);
            if (xE.HasAttribute("Humidity"))
                this.Humidity = MM_Converter.ToSingle(xE.Attributes["Humidity"].Value);
            if (xE.HasAttribute("WindDirection"))
                this.WindDirection = xE.Attributes["WindDirection"].Value;
            if (xE.HasAttribute("WindSpeed"))
                this.WindSpeed = MM_Converter.ToSingle(xE.Attributes["WindSpeed"].Value);
            if (xE.HasAttribute("Pressure"))
                this.Pressure = MM_Converter.ToSingle(xE.Attributes["Pressure"].Value);
            if (xE.HasAttribute("Dewpoint"))
                this.Dewpoint = MM_Converter.ToSingle(xE.Attributes["Dewpoint"].Value);
            if (xE.HasAttribute("Summary"))
                this.Summary = (xE.Attributes["Summary"].Value);
        }

        #endregion

        /// <summary>
        /// Get this weather station's information in XML format
        /// </summary>
        public String XMLString
        {
            get
            {
                StringBuilder OutStr = new StringBuilder("<WeatherStation");

                OutStr.Append(" StationID=\"" + System.Web.HttpUtility.HtmlEncode(StationID) + "\"");
                OutStr.Append(" Description=\"" + System.Web.HttpUtility.HtmlEncode(Description) + "\"");
                OutStr.Append(" Coordinates=\"" + LngLat.X + "," + LngLat.Y + "\"");
                OutStr.Append(" Elevation=\"" + Elevation + "\"");
                OutStr.Append(" Temperature=\"" + Temperature + "\"");
                OutStr.Append(" Humidity=\"" + Humidity + "\"");
                OutStr.Append(" WindDirection=\"" + System.Web.HttpUtility.HtmlEncode(WindDirection) + "\"");
                OutStr.Append(" WindSpeed=\"" + WindSpeed + "\"");
                OutStr.Append(" Pressure=\"" + Pressure + "\"");
                OutStr.Append(" Summary=\"" + System.Web.HttpUtility.HtmlEncode(Summary) + "\"");
                OutStr.Append(" Dewpoint=\"" + Dewpoint + "\"");
                OutStr.Append("/>");
                return OutStr.ToString();
            }
        }
        #region Weather updating
        /// <summary>
        /// Update the weather for this station
        /// </summary>
        public void UpdateWeather()
        {
            WebRequest wReq = WebRequest.Create(MM_WeatherStation.QueryPrepend + StationID);
            wReq.UseDefaultCredentials = true;
            XmlDocument xDoc = new XmlDocument();
            try
            {
                xDoc.Load(wReq.GetResponse().GetResponseStream());

                this.Description = xDoc.SelectSingleNode("/current_observation/display_location/full").InnerText;
                this.Summary = xDoc.SelectSingleNode("/current_observation/weather").InnerText;
                this.WindDirection = xDoc.SelectSingleNode("/current_observation/wind_dir").InnerText;
                this.Dewpoint = ConvertToSingle(xDoc.SelectSingleNode("/current_observation/dewpoint_f").InnerText);
                this.Elevation = ConvertToSingle(xDoc.SelectSingleNode("/current_observation/observation_location/elevation").InnerText);
                this.Humidity = ConvertToSingle(xDoc.SelectSingleNode("/current_observation/relative_humidity").InnerText);
                this.Pressure = ConvertToSingle(xDoc.SelectSingleNode("/current_observation/pressure_in").InnerText);
                this.Temperature = ConvertToSingle(xDoc.SelectSingleNode("/current_observation/temp_f").InnerText);
                this.WindSpeed = ConvertToSingle(xDoc.SelectSingleNode("/current_observation/wind_mph").InnerText);
                Debug.WriteLine("Updated weather data for station " + this.StationID);
                xDoc = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error retrieving weather station " + StationID + ":" + ex.Message);
            }
            finally
            {
                xDoc = null;
                wReq = null;
            }
        }

        /// <summary>
        /// Convert a string to a single-precision number
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        private float ConvertToSingle(String Value)
        {
            StringBuilder ToReadStr = new StringBuilder();
            foreach (Char c in Value.ToCharArray())
                if (Char.IsNumber(c) || (c == '.'))
                    ToReadStr.Append(c);
            if (ToReadStr.Length == 0)
                return 0f;
            else
                return MM_Converter.ToSingle(ToReadStr.ToString());
        }
        #endregion

    }
}
