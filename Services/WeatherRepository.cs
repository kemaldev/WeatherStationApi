using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using WeatherStationApi.Models;

namespace WeatherStationApi.Services
{
    /// <summary>
    /// Class with methods related to getting data from the
    /// blob storage.
    /// </summary>
    public class WeatherRepository : IWeatherRepository
    {

        /// <summary>
        /// Gets all the data from a sensor type in the blob storage
        /// according to the date, device and sensor type that are 
        /// entered as the parameters and packs the data in as a weather report.
        /// </summary>
        /// <param name="dateText"></param>
        /// <param name="deviceName"></param>
        /// <param name="sensorName"></param>
        /// <returns></returns>
        public WeatherReport GetWeatherReport(string dateText, string deviceName, string sensorName)
        {
            List<SensorType> data = GetDataFromFile(dateText, deviceName, sensorName);

            if (data == null)
            {
                return null;
            }

            DateTime date = DateTime.Parse(dateText);
            var device = new Device
            {
                DeviceName = deviceName,
                Date = date
            };

            List<SensorList> sensorTypes = new List<SensorList>();
            SensorList sensorList = new SensorList();
            sensorList.sensorName = sensorName;
            sensorList.sensorList = data;
            sensorTypes.Add(sensorList);

            WeatherReport report = new WeatherReport();
            report.Device = device;
            report.SensorTypes = sensorTypes;

            return report;
        }

        /// <summary>
        /// Gets all the data from all sensor types in the blob storage
        /// according to the date and device that are 
        /// entered as the parameters and packs the data in as a weather report.
        /// </summary>
        /// <param name="dateText"></param>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        public WeatherReport GetWeatherReport(string dateText, string deviceName)
        {
            List<SensorType> dataTemperature = GetDataFromFile(dateText, deviceName, "temperature");
            List<SensorType> dataHumidity = GetDataFromFile(dateText, deviceName, "humidity");
            List<SensorType> dataRainfall = GetDataFromFile(dateText, deviceName, "rainfall");

            if(dataTemperature == null && dataHumidity == null && dataRainfall == null)
            {
                return null;
            }

            DateTime date = DateTime.Parse(dateText);
            var device = new Device
            {
                DeviceName = deviceName,
                Date = date
            };

            List<SensorList> sensorTypes = new List<SensorList>();
            SensorList sensorListTemperature = new SensorList();
            sensorListTemperature.sensorName = "temperature";
            sensorListTemperature.sensorList = dataTemperature;

            SensorList sensorListHumidity = new SensorList();
            sensorListHumidity.sensorName = "humidity";
            sensorListHumidity.sensorList = dataHumidity;

            SensorList sensorListRainfall = new SensorList();
            sensorListRainfall.sensorName = "rainfall";
            sensorListRainfall.sensorList = dataRainfall;

            sensorTypes.Add(sensorListTemperature);
            sensorTypes.Add(sensorListHumidity);
            sensorTypes.Add(sensorListRainfall);

            WeatherReport report = new WeatherReport();
            report.Device = device;
            report.SensorTypes = sensorTypes;

            return report;
        }

        /// <summary>
        /// Gets all the data from a sensor type in the blob storage
        /// according to the date, device and sensor type that are 
        /// entered as the parameters.
        /// </summary>
        /// <param name="dateText"></param>
        /// <param name="deviceName"></param>
        /// <param name="sensorName"></param>
        /// <returns></returns>
        private List<SensorType> GetDataFromFile(string dateText, string deviceName, string sensorName)
        {
            string csvPath =  "/data/" + deviceName + "/" + sensorName + "/" + dateText + ".csv";
            string zipPath =  "/data/" + deviceName + "/" + sensorName + "/" + "historical.zip";

            List<SensorType> data = null;

            if (File.Exists(csvPath))
            {
                data = DataToSensorList(csvPath, sensorName);
            }
            else
            {
                if (File.Exists(zipPath))
                {
                    data = ZipToSensorList(sensorName, dateText, zipPath);
                }
                else
                {
                    return null;
                }
            }

            return data;
        }

        /// <summary>
        /// Reads the data from the blob and represents it as a List
        /// of sensor types.
        /// </summary>
        /// <param name="blob"></param>
        /// <param name="sensorName"></param>
        /// <returns></returns>
        private List<SensorType> DataToSensorList(string path, string sensorName)
        {
            List<SensorType> data = new List<SensorType>();

            using (StreamReader reader = new StreamReader(path))
            {
                parseSensorValues(reader, sensorName, data);
            }

            return data;
        }

        /// <summary>
        /// Parses the data gotten from the blob file into
        /// usable information that can be used by objects.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="sensorName"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private void parseSensorValues(StreamReader reader, string sensorName, List<SensorType> data)
        {
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                string[] values = line.Split(";");

                if (sensorName == "humidity")
                {
                    Humidity humidity = new Humidity();

                    humidity.Time = DateTime.Parse(values[0]);
                    humidity.HumidityAmount = Convert.ToDouble(values[1]);
                    data.Add(humidity);
                }
                else if (sensorName == "temperature")
                {
                    Temperature temperature = new Temperature();
                    temperature.Time = DateTime.Parse(values[0]);
                    temperature.Degrees = Convert.ToDouble(values[1]);
                    data.Add(temperature);
                }
                else
                {
                    Rainfall rainfall = new Rainfall();
                    rainfall.Time = DateTime.Parse(values[0]);
                    rainfall.RainAmount = Convert.ToDouble(values[1]);
                    data.Add(rainfall);
                }
            }
        }

        /// <summary>
        /// Searches for specific file with sensor data in compressed
        /// file and returns the data if found as a list of sensor types.
        /// </summary>
        /// <param name="sensorName"></param>
        /// <param name="dateText"></param>
        /// <returns></returns>
        private List<SensorType> ZipToSensorList(string sensorName, string dateText, string zipPath)
        {
            List<SensorType> data = new List<SensorType>();

            using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Read))
            {
                ZipArchiveEntry entry;

                entry = archive.GetEntry(dateText + ".csv");

                if (entry == null)
                {
                    return null;
                }

                using (StreamReader reader = new StreamReader(entry.Open()))
                {
                    parseSensorValues(reader, sensorName, data);
                }
            }

            return data;
        }
    }
}
