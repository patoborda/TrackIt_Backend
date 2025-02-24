// ImageService.cs
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace trackit.server.Services
{
    public class ImageService : IImageService
    {
        private readonly Cloudinary _cloudinary;

        public ImageService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        // Subir una imagen a Cloudinary y devolver la URL de la imagen
        public async Task<string> UploadImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file uploaded");
            }

            try
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    Folder = "trackit"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                Console.WriteLine($"Cloudinary Response: {uploadResult.StatusCode}");

                if (uploadResult.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Console.WriteLine($"Image uploaded successfully: {uploadResult.SecureUrl.AbsoluteUri}");
                    return uploadResult.SecureUrl.AbsoluteUri;
                }

                throw new Exception("Error uploading image to Cloudinary");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UploadImageAsync: {ex.Message}");
                throw;
            }
        }

    }
}
