// IImageService.cs
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace trackit.server.Services
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file);
    }
}
