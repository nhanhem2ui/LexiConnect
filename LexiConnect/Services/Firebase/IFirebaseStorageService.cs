namespace LexiConnect.Services.Firebase
{
    public interface IFirebaseStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder);
        Task DeleteFileAsync(string folder, string fileName);
    }
}
