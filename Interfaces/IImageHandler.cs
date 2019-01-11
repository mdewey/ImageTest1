using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace imagetest1.Interfaces
{
    public interface IImageHandler
    {
        Task<string> UploadImage(IFormFile file);
    }
}