using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;

namespace Blog.Services.Firebase.Storage;

public class StorageFile
{
    private readonly String _bucket;
    private readonly StorageClient _storageClient;

    public StorageFile(string bucket, StorageClient storageClient)
    {
        _bucket = bucket;
        _storageClient = storageClient;
    }

    public async Task<string> UploadImage(string fileName, string filePath)
    {
        // tạo unique file
        fileName = fileName + Guid.NewGuid();
        var stream = new FileStream(filePath, FileMode.Open);
        var contentType = "image/jpeg";
        var result = await _storageClient.UploadObjectAsync(_bucket, fileName, contentType, stream);

        // return về một uri (link)
        return result.MediaLink;
    }
}
