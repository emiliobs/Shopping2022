using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace Shopping2022.Helpers
{
    public class BlobHelper : IBlobHelper
    {
        private readonly CloudBlobClient _blobClient;

        public BlobHelper(IConfiguration configuration)
        {
            //string keys = configuration["Blob:ConnectionString"];
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(keys);
            //_blobClient = storageAccount.CreateCloudBlobClient();

            string keys = configuration["Blob:connectionstring"];
            CloudStorageAccount storageaccount = CloudStorageAccount.Parse(keys);
            _blobClient = storageaccount.CreateCloudBlobClient();
        }

        public async Task<Guid> UploadBlobAsync(IFormFile file, string containerName)
        {
            Stream? stream = file.OpenReadStream();

            return await UploadBlobAsync(stream, containerName);

            //Guid name = Guid.NewGuid();
            //CloudBlobContainer? container = _blobClient.GetContainerReference(containerName);
            //CloudBlockBlob? blockBlob = container.GetBlockBlobReference($"{name}");
            //await blockBlob.UploadFromStreamAsync(stream);
            // return name;
        }

        public async Task<Guid> UploadBlobAsync(string image, string containerName)
        {
            FileStream? stream = File.OpenRead(image);
            return await UploadBlobAsync(stream, containerName);
        }

        public async Task<Guid> UploadBlobAsync(byte[] file, string containerName)
        {
            MemoryStream? stream = new(file);
            return await UploadBlobAsync(stream, containerName);
        }


        public async Task DeleteBlobAsync(Guid id, string containerName)
        {
            try
            {
                CloudBlobContainer container = _blobClient.GetContainerReference(containerName);
                CloudBlockBlob blockBlob = container.GetBlockBlobReference($"{id}");
                await blockBlob.DeleteAsync();
            }
            catch
            {

            }
        }

        private async Task<Guid> UploadBlobAsync(Stream stream, string containerName)
        {


            Guid name = Guid.NewGuid();
            CloudBlobContainer? container = _blobClient.GetContainerReference(containerName);
            CloudBlockBlob? blockBlob = container.GetBlockBlobReference($"{name}");
            await blockBlob.UploadFromStreamAsync(stream);

            return name;


        }
    }
}
