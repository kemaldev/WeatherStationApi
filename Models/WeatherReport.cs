using System.Collections.Generic;

namespace WeatherStationApi.Models
{
    /// <summary>
    /// Class that contains information about
    /// data from several sensor types that a device has recorded.
    /// </summary>
    public class WeatherReport
    {
        public Device Device { get; set; }
        public List<SensorList> SensorTypes { get; set; }
    }
}
