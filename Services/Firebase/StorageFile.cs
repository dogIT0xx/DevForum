using Blog.Services.MailService;
using Google.Apis.Storage.v1.Data;
using Microsoft.Extensions.Options;

namespace Blog.Services.Firebase
{
    public class StorageFile : IStorageFile
    {
        private readonly StorageFileSetting _storageFileSetting;
        private readonly ILogger<StorageFile> _logger;

        public StorageFile(IOptions<StorageFileSetting> options, ILogger<StorageFile> logger)
        {
            _storageFileSetting = options.Value;
            _logger = logger;
        }

        public async Task UploadImageAsync(string fileName, Stream stream)
        {
            var contentType = "image/jpeg";
            await _storageFileSetting.StorageClient.UploadObjectAsync(_storageFileSetting.Bucket, fileName, contentType, stream);
        }
    }
}

