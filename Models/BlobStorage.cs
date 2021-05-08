using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AzureFileUploader.Models
{
    public class BlobStorage : IStorage
    {
        private readonly AzureStorageConfig _storageConfig;

        public BlobStorage(IOptions<AzureStorageConfig> storageConfig)
        {
            _storageConfig = storageConfig.Value;
        }

        public async Task Initialize()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageConfig.ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.FileContainerName);

            await container.CreateIfNotExistsAsync();
        }

        public async Task Save(Stream fileStream, string name)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageConfig.ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.FileContainerName);
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(name);

            await blockBlob.UploadFromStreamAsync(fileStream);
        }

        public async Task<IEnumerable<string>> GetNames()
        {
            List<string> names = new();

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageConfig.ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.FileContainerName);

            BlobContinuationToken continuationToken = null;
            BlobResultSegment resultSegment = null;

            do
            {
                resultSegment = await container.ListBlobsSegmentedAsync(continuationToken);

                // Get the name of each blob.
                names.AddRange(resultSegment.Results.OfType<ICloudBlob>().Select(b => b.Name));

                continuationToken = resultSegment.ContinuationToken;
            }
            while (continuationToken != null);

            return names;
        }

        public async Task<Stream> Load(string name)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageConfig.ConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(_storageConfig.FileContainerName);

            return await container.GetBlobReference(name).OpenReadAsync();
        }
    }
}
