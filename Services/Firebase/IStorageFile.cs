namespace Blog.Services.Firebase
{
    public interface IStorageFile
    {
        Task UploadImageAsync(string fileName, Stream stream);
    }
}
