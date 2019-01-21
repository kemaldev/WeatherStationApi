using WeatherStationApi.Models;

namespace WeatherStationApi.Services
{
    /// <summary>
    /// Interface with methods for the different calls
    /// you can make in order to get a weather report.
    /// </summary>
    public interface IWeatherRepository
    {
        WeatherReport GetWeatherReport(string date, string device, string sensorType);
        WeatherReport GetWeatherReport(string date, string device);
    }
}
