using Microsoft.AspNetCore.Mvc;
using WeatherStationApi.Models;
using WeatherStationApi.Services;

namespace WeatherStationApi.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class WeatherReportsController : ControllerBase
    {
        private IWeatherRepository _weatherRepository;

        public WeatherReportsController(IWeatherRepository weatherRepository)
        {
            _weatherRepository = weatherRepository;
        }

        /// <summary>
        /// Api endpoint to /api/weatherreports/getdata
        /// Returns all the data that is associated with the 
        /// date, device name and sensor type the user decides
        /// to query.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="deviceName"></param>
        /// <param name="sensorType"></param>
        /// <returns></returns>
        [HttpGet("getdata/{date}/{deviceName}/{sensorType}")]
        public IActionResult GetAsync(string date, string deviceName, string sensorType)
        {
            WeatherReport report = _weatherRepository.GetWeatherReport(date, deviceName, sensorType);

            if(report == null)
            {
                return NotFound("No record found...");
            }

            return Ok(report);
        }

        /// <summary>
        /// Api endpoint to /api/weatherreports/getdatafordevice
        /// Returns all the data from all available sensor types for the
        /// specified date and device name the user decides to query.
        /// </summary>
        /// <param name="date"></param>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        [HttpGet("getdatafordevice")]
        public IActionResult GetAllSensorTypesAsync(string date, string deviceName)
        {
            WeatherReport report =  _weatherRepository.GetWeatherReport(date, deviceName);

            if(report == null)
            {
                return NotFound("No records found...");
            }

            return Ok(report);
        }
    }
}
