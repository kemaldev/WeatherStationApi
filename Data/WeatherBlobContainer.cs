using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace WeatherStationApi.Data
{
    /// <summary>
    /// Class that represents the connection-point to the blob container.
    /// </summary>
    public class WeatherBlobContainer
    {
        private IConfiguration Configuration;
        public CloudBlobContainer BlobContainer { get; set; }

        public WeatherBlobContainer(IConfiguration configuration)
        {
            Configuration = configuration;
            BlobContainer = GetCloudBlobContainer();
        }

        /// <summary>
        /// Connects to the cloud storage account, creates a client
        /// and gets the container used for the blob storage.
        /// </summary>
        /// <returns></returns>
        private CloudBlobContainer GetCloudBlobContainer()
        {
            CloudStorageAccount storageAccount =
                CloudStorageAccount.Parse(Configuration.GetSection("ConnectionStrings").GetSection("StorageConnectionString").Value);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("iotbackend");

            return container;
        }

    }
}
