using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace imagetest1.Interfaces
{
    public interface IImageWriter
    {
        Task<string> UploadImage(IFormFile file);
    }
}