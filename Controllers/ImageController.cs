using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using imagetest1;
using imagetest1.ImageUtilities;
using imagetest1.Interfaces;
using imagetest1.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace content.Controllers
{
    [Route("api/[controller]")]
    public class ImageController : Controller
    {

        private readonly IImageHandler _imageHandler;
        private readonly IOptions<CloudinaryKeys> _options;

        private readonly DatabaseContext _context;

        public ImageController(
            IImageHandler imageHandler, 
            IOptions<CloudinaryKeys> options,
            DatabaseContext context)
        {
            _imageHandler = imageHandler;
            _options = options;
            _context = context; 
        }

        /// <summary>
        /// Uplaods an image to the server.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult> UploadImage(IFormFile file)
        {

            var path =  await _imageHandler.UploadImage(file);
            var rv = new CloudinaryStorage(_options.Value).UploadFile(path);
            
            var image = new Image{
                Url = rv.SecureUri.AbsoluteUri
            };
            this._context.Images.Add(image);
            await this._context.SaveChangesAsync();
            
            await _imageHandler.DeleteFile(path);

            return Ok(new {path, image});
        }


    }
}
