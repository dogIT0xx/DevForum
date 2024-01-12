using Google.Cloud.Storage.V1;

namespace Blog.Services.Firebase
{
    public class StorageFileSetting
    {
        public string Bucket;
        public StorageClient StorageClient;
    }
}
