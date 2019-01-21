using System.Collections.Generic;

namespace WeatherStationApi.Models
{
    /// <summary>
    /// Class representing all of the data that a sensor has recorded.
    /// </summary>
    public class SensorList
    {
        public string sensorName { get; set; }
        public List<SensorType> sensorList { get; set; }
    }
}
