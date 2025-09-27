using Firebase.Auth;
using Firebase.Storage;

namespace LexiConnect.Services.Firebase
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly string _apiKey;
        private readonly string _bucket;
        private readonly string _authEmail;
        private readonly string _authPassword;

        public FirebaseStorageService(IConfiguration config)
        {
            _apiKey = config["Firebase:ApiKey"] ?? "";
            _bucket = config["Firebase:Bucket"] ?? "";
            _authEmail = config["Firebase:AuthEmail"] ?? "";
            _authPassword = config["Firebase:AuthPassword"] ?? "";
        }

        private async Task<string> GetFirebaseTokenAsync()
        {
            var auth = new FirebaseAuthProvider(new FirebaseConfig(_apiKey));
            var a = await auth.SignInWithEmailAndPasswordAsync(_authEmail, _authPassword);
            return a.FirebaseToken;
        }

        // Upload a file by name
        public async Task<string> UploadFileAsync(IFormFile file, string folder)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is empty or not provided.");

            var token = await GetFirebaseTokenAsync();
            var fileName = $"{Guid.NewGuid()}_{file.FileName}";

            using var stream = file.OpenReadStream();

            var task = new FirebaseStorage(
                _bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(token),
                    ThrowOnCancel = true
                })
                .Child(folder)        // folder in Firebase Storage
                .Child(fileName)      // actual file name
                .PutAsync(stream);

            return await task; // returns download URL
        }

        // Delete a file by name
        public async Task DeleteFileAsync(string folder, string fileName)
        {
            var token = await GetFirebaseTokenAsync();

            var storage = new FirebaseStorage(
                _bucket,
                new FirebaseStorageOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(token),
                    ThrowOnCancel = true
                });

            await storage
                .Child(folder)
                .Child(fileName)
                .DeleteAsync();
        }
    }
}
