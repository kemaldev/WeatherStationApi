using Microsoft.Extensions.Hosting;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WeatherStationApi.Data;

namespace WeatherStationApi.Services
{
    /// <summary>
    /// Service that updates the local data files 
    /// to assure that the local files are up to 
    /// date with the files in the blob storage.
    /// </summary>
    public class DataService : IHostedService, IDisposable
    {
        private WeatherBlobContainer _weatherBlobContainer;
        private Timer _timer;

        public DataService(WeatherBlobContainer weatherBlobContainer, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            _weatherBlobContainer = weatherBlobContainer;
        }

        /// <summary>
        /// Downloads all .csv and .zip files from a specified CloudBlobDirectory
        /// and replaces these with old ones locally.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="directory"></param>
        /// <returns></returns>
        private async Task UpdateSensorTypeData(CloudBlobContainer container, CloudBlobDirectory directory)
        {
            BlobContinuationToken continuationToken = null;
            List<IListBlobItem> results = new List<IListBlobItem>();
            do
            {
                var response = await directory.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;
                results.AddRange(response.Results);
            }
            while (continuationToken != null);

            foreach (var blobItem in results)
            {
                if (blobItem.Uri.AbsoluteUri.Contains(".csv") || blobItem.Uri.AbsoluteUri.Contains(".zip"))
                {
                    StringBuilder sb = new StringBuilder();
                    CloudBlob blob;
                    string[] subdirectories = blobItem.Uri.AbsolutePath.Split("/");
                    for (int i = 2; i < subdirectories.Length; i++)
                    {
                        sb.Append(subdirectories[i]);
                        if (i + 1 != subdirectories.Length)
                            sb.Append("/");
                    }

                    if (blobItem.Uri.AbsoluteUri.Contains(".csv"))
                        blob = container.GetAppendBlobReference(sb.ToString());
                    else
                        blob = container.GetBlockBlobReference(sb.ToString());

                    string fileName = subdirectories[subdirectories.Length - 1];
                    string sensorType = subdirectories[subdirectories.Length - 2];
                    string deviceName = subdirectories[subdirectories.Length - 3];
                    string path = "/data/" + deviceName + "/" + sensorType + "/";

                    Directory.CreateDirectory(path);
                    using (var fileStream = File.OpenWrite(path + "_temp" + fileName))
                    {
                        await blob.DownloadToStreamAsync(fileStream);
                    }

                    File.Delete(path + fileName);
                    File.Move(path + "_temp" + fileName, path + fileName);
                }
            }
        }

        /// <summary>
        /// Updates all of the local files so they are up to date with 
        /// the files in the blob storage.
        /// </summary>
        /// <param name="state"></param>
        private async void UpdateDataAsync(object state)
        {
            CloudBlobContainer container = _weatherBlobContainer.BlobContainer;

            CloudBlobDirectory humidityDirectory = container.GetDirectoryReference("dockan/humidity");
            CloudBlobDirectory rainfallDirectory = container.GetDirectoryReference("dockan/rainfall");
            CloudBlobDirectory temperatureDirectory = container.GetDirectoryReference("dockan/temperature");

            await UpdateSensorTypeData(container, humidityDirectory);
            await UpdateSensorTypeData(container, rainfallDirectory);
            await UpdateSensorTypeData(container, temperatureDirectory);
        }

        /// <summary>
        /// Disposes the timer.
        /// </summary>
        public void Dispose()
        {
            _timer?.Dispose();
        }

        /// <summary>
        /// Starts the service and updates the files in the
        /// blob storage every 20 minutes.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            //Start timed background task to keep updating the historical data when needed.
            _timer = new Timer(UpdateDataAsync, null, TimeSpan.Zero, TimeSpan.FromMinutes(20));

            return Task.CompletedTask;
        }

        /// <summary>
        /// Timer is disabled and is no longer updating files.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }
    }
}
