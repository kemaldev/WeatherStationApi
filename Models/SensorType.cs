using System;

namespace WeatherStationApi.Models
{
    /// <summary>
    /// Class that represents a sensor type.
    /// This can be temperature, humidity or rainfall
    /// as an example.
    /// </summary>
    public class SensorType
    {
        public DateTime Time { get; set; }
    }
}
