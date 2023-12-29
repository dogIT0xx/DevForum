using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using Microsoft.CodeAnalysis.Options;

namespace Blog.Services.Firebase.Storage;

public class StorageFile
{
    private string _bucket;
    private StorageClient _storageClient;

    public StorageFile(StorageClient storageClient)
    {
        _bucket = "blogmvc-b2c63.appspot.com";
        _storageClient = storageClient;
    }

    public async Task<string> UploadImage(string fileName, string filePath)
    {
        var stream = new FileStream(filePath, FileMode.Open);
        var contentType = "image/jpeg";
        var result = await _storageClient.UploadObjectAsync(_bucket, fileName, contentType, stream);

        // return về một uri (link)
        return result.MediaLink;
    }

    public async Task<string> UploadImage(string fileName, Stream stream)
    {
        var contentType = "image/jpeg";
        var result = await _storageClient.UploadObjectAsync(_bucket, fileName, contentType, stream);

        // return về một uri (link)
        var uri = $"https://firebasestorage.googleapis.com/v0/b/blogmvc-b2c63.appspot.com/o/{fileName}?alt=media";
        
        return uri;
    }
}
