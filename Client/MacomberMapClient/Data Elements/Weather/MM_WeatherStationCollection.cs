using MacomberMapClient.Data_Elements.Geographic;
using MacomberMapClient.Integration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace MacomberMapClient.Data_Elements.Weather
{
    /// <summary>
    /// (C) 2014, Michael E. Legatt, Ph.D., Electric Reliability Council of Texas, Inc. All Rights Reserved
    /// This class is responsible for maintaining the collection of weather stations, and loading them appropriately
    /// </summary>
    public class MM_WeatherStationCollection
    {
        #region Variable declarations
        /// <summary>The URL that begins the query to retrieve a list of stations within a particular county</summary>
        public static string QueryPrepend;

        /// <summary>The weather alerts for the county</summary>
        public static string AlertsPrepend;

        /// <summary>The weather forecast for the county</summary>
        public static string ForecastPrepend;

        /// <summary>The collection of weather stations</summary>
        public Dictionary<String, MM_WeatherStation> WeatherStations = new Dictionary<string, MM_WeatherStation>();
        #endregion


        #region Initialization
        /// <summary>
        /// Load the list of weather stations for each county
        /// </summary>
        public void LoadWeatherStations()
        {
            ThreadPool.QueueUserWorkItem(new WaitCallback(WeatherManager));
        }

        /// <summary>
        /// Manage the weather by continuously updating the weather for each county every 10 minutes
        /// </summary>
        /// <param name="state">The state of the thread</param>
        private void WeatherManager(Object state)
        {
            //First, load in all of our weather stations.
            foreach (MM_Boundary Bound in MM_Repository.Counties.Values)
                LoadWeatherStation(Bound);


            //Now, periodically update them
            while (true)
            {
                foreach (MM_WeatherStation Station in WeatherStations.Values)
                    Station.UpdateWeather();
                Thread.Sleep(1000 * 60 * 10);
            }
        }


        /// <summary>
        /// Load the weather stations for each county
        /// </summary>
        /// <param name="County">The county to be loaded</param>
        private void LoadWeatherStation(MM_Boundary County)
        {
            try
            {
                WebRequest wReq = WebRequest.Create(MM_WeatherStationCollection.QueryPrepend + County.Name + ",TX"); //+ County.Centroid.Y.ToString() + "," + County.Centroid.X.ToString());
                wReq.UseDefaultCredentials = true;
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(wReq.GetResponse().GetResponseStream());

                XmlNodeList AirportStations = xDoc.SelectNodes("/location[@type='CITY']/nearby_weather_stations/airport/station");
                County.WeatherStations = new List<MM_WeatherStation>(AirportStations.Count);
                foreach (XmlElement xE in AirportStations)
                    if (xE.ParentNode.ParentNode.ParentNode["state"].InnerText == "TX")
                    {
                        MM_WeatherStation wx = new MM_WeatherStation(xE);
                        County.WeatherStations.Add(wx);
                        if (!WeatherStations.ContainsKey(wx.StationID))
                            WeatherStations.Add(wx.StationID, wx);
                        wx.UpdateWeather();
                    }

                //Now load the alerts and forecast for the county
                LoadWeatherAlerts(County);
                LoadWeatherForecast(County);

                Debug.WriteLine("Loaded " + AirportStations.Count.ToString("#,##0") + " weather stations for " + County.Name + " county.");
                xDoc = null;

            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error retrieving weather station list for county " + County.Name + ":" + ex.Message);
            }
            finally
            {

            }
        }

        /// <summary>
        /// Load the weather alerts for a county
        /// </summary>
        /// <param name="County">The county for which alerts should be loaded</param>
        public void LoadWeatherAlerts(MM_Boundary County)
        {
            WebRequest wReq = WebRequest.Create(MM_WeatherStationCollection.AlertsPrepend + County.Centroid.Y.ToString() + "," + County.Centroid.X.ToString());
            wReq.UseDefaultCredentials = true;
            XmlDocument xDoc = new XmlDocument();
            try
            {
                xDoc.Load(wReq.GetResponse().GetResponseStream());
                XmlNodeList Messages = xDoc.SelectNodes("/alerts/alert/AlertItem/message");
                if (County.WeatherAlerts != null)
                    County.WeatherAlerts.Clear();
                County.WeatherAlerts = new List<string>(Messages.Count);
                foreach (XmlElement xE in Messages)
                    County.WeatherAlerts.Add(xE.InnerText);
                Debug.WriteLine("Loaded weather alerts for county" + County.Name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error parsing weather alerts for county" + County.Name + ":" + ex.Message);
            }
            finally
            {
                xDoc = null;
                wReq = null;
            }
        }

        /// <summary>
        /// Load the weather forecast for a county
        /// </summary>
        /// <param name="County">The county for which forecasts should be loaded</param>
        public void LoadWeatherForecast(MM_Boundary County)
        {
            WebRequest wReq = WebRequest.Create(MM_WeatherStationCollection.ForecastPrepend + County.Centroid.Y.ToString() + "," + County.Centroid.X.ToString());
            wReq.UseDefaultCredentials = true;
            XmlDocument xDoc = new XmlDocument();
            try
            {
                xDoc.Load(wReq.GetResponse().GetResponseStream());
                XmlNodeList Messages = xDoc.SelectNodes("/forecast/txt_forecast/forecastday");
                if (County.WeatherForecast != null)
                    County.WeatherForecast.Clear();
                County.WeatherForecast = new Dictionary<string, string>(Messages.Count);
                foreach (XmlElement xE in Messages)
                    County.WeatherForecast.Add(xE["title"].InnerText, xE["fcttext"].InnerText);
                Debug.WriteLine("Loaded weather forecast for county" + County.Name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error parsing weather forecast for county" + County.Name + ":" + ex.Message);
            }
            finally
            {
                xDoc = null;
                wReq = null;
            }
        }
        #endregion
    }
}
