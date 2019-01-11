# Uploading Image example

To handle images, its a best practice to use a service that specializes in storing and serving images.  In this example, we will use Dropzone to upload an image to our API and then have our API store that image in [Cloudinary](https://cloudinary.com). 

## Client side: 
This is using [Dropzone](https://react-dropzone.netlify.com/) to capture the file that was selected 
in the browser and then successfully uploaded to the our API endpoint.

Using Dropzone is not needed to upload files, but it provides for us
functionally such validation, multiple files and other features that will be needed later.


OnDrop is called from the Dropzone component and each file that is passed in
will create a promise that is then used by axios to actually send our requests 

``` shell
yarn add react-dropzone 
yarn add classnames
yarn add axios
```


``` javascript
import Dropzone from 'react-dropzone';
import axios from 'axios';
import classNames from 'classnames';

```
Add this code into component that you want to upload files

  ``` javascript
  onDrop = files => {
    console.log({ files })
    // Push all the axios request promise into a single array
    const uploaders = files.map(file => {
      // Initial FormData
      const formData = new FormData();
      formData.append("file", file);
      
      // Make an AJAX upload request using Axios
      return axios.post("/api/image", formData, {
        // using 
        headers: { 
          "content-type": "multipart/form-data",
          "accept" : "application/json"
         },
      }).then(response => {
        console.log({response});
        this.setState({
          lastUploadedUrl: response.data.secure_url
        })
      })
    });
  
    // Once all the files are uploaded 
    axios.all(uploaders).then(() => {
     console.log("done");
    });
  }
  ```

  In your html, you will need to add the Dropzone component: 
  
  ``` jsx
<Dropzone onDrop={this.onDrop}>
    {({ getRootProps, getInputProps, isDragActive }) => {
    return (
        <div
        {...getRootProps()}
        className={classNames('dropzone', { 'dropzone--isActive': isDragActive })}
        >
        <input {...getInputProps()} />
        {
            isDragActive ?
            <p>Drop files here...</p> :
            <p>Try dropping some files here, or click to select files to upload.</p>
        }
        </div>
    )
    }}
</Dropzone>
```

## Server Side. 

Mode of the heavy lifting is done in the `ImageController`. This creates the route that the image is posted to. 

The controller has 3 dependencies that we register in services configure, in `StartUp.cs`

``` C# 
// Register what handles the image uploading
services.AddTransient<IImageHandler, ImageHandler>();
// register the ImageWriter, this will save the file to disc
services.AddTransient<IImageWriter, ImageWriter>();
// register you keys for Cloudiary
services.Configure<CloudinaryKeys>(opts => Configuration.Bind(opts));
```

There the next two steps are to save the incoming file to the disc, then to save that file to the cloud

### Step 1, Save to local disc

We want to save to local disc first so we can validate the incoming image, and as to pass it over to the Cloudiary

We will add the `ImageHandler` and the `ImageWriter` Classes



This `ImageHandler` has 1 job and that is call the ImageWrite with the file data from the incoming Form request


``` C# 
  public interface IImageHandler
    {
        Task<string> UploadImage(IFormFile file);
    }

    public class ImageHandler : IImageHandler
    {
        private readonly IImageWriter _imageWriter;
        public ImageHandler(IImageWriter imageWriter)
        {
            _imageWriter = imageWriter;
        }

        public async Task<string> UploadImage(IFormFile file)
        {
            var result = await _imageWriter.UploadImage(file);
            return result;
        }
    }
```

The `ImageWriter` class is what is going to create the file on disk.  This code is the meat of the first part, be sure to take your time look at what each method is doing. 

```C#
 public interface IImageWriter
    {
        Task<string> UploadImage(IFormFile file);
    }

    public class ImageWriter : IImageWriter
    {
        public async Task<string> UploadImage(IFormFile file)
        {
            var exists = CheckIfImageFile(file);
            if (exists)
            {
                return await WriteFile(file);
            }

            return "Invalid image file";
        }

        /// <summary>
        /// Method to check if file is image file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private bool CheckIfImageFile(IFormFile file)
        {
            byte[] fileBytes;
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                fileBytes = ms.ToArray();
            }

            return WriterHelper.GetImageFormat(fileBytes) != WriterHelper.ImageFormat.unknown;
        }

        /// <summary>
        /// Method to write file onto the disk
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<string> WriteFile(IFormFile file)
        {
            string fileName;
            var path = String.Empty;
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                fileName = Guid.NewGuid().ToString() + extension; //Create a new Name 
                                                                  //for the file due to security reasons.
                path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                using (var bits = new FileStream(path, FileMode.Create))
                {
                    await file.CopyToAsync(bits);
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return path;
        }


        public class WriterHelper
        {
            public enum ImageFormat
            {
                bmp,
                jpeg,
                gif,
                tiff,
                png,
                unknown
            }

            public static ImageFormat GetImageFormat(byte[] bytes)
            {
                var bmp = Encoding.ASCII.GetBytes("BM");     // BMP
                var gif = Encoding.ASCII.GetBytes("GIF");    // GIF
                var png = new byte[] { 137, 80, 78, 71 };              // PNG
                var tiff = new byte[] { 73, 73, 42 };                  // TIFF
                var tiff2 = new byte[] { 77, 77, 42 };                 // TIFF
                var jpeg = new byte[] { 255, 216, 255, 224 };          // jpeg
                var jpeg2 = new byte[] { 255, 216, 255, 225 };         // jpeg canon

                if (bmp.SequenceEqual(bytes.Take(bmp.Length)))
                    return ImageFormat.bmp;

                if (gif.SequenceEqual(bytes.Take(gif.Length)))
                    return ImageFormat.gif;

                if (png.SequenceEqual(bytes.Take(png.Length)))
                    return ImageFormat.png;

                if (tiff.SequenceEqual(bytes.Take(tiff.Length)))
                    return ImageFormat.tiff;

                if (tiff2.SequenceEqual(bytes.Take(tiff2.Length)))
                    return ImageFormat.tiff;

                if (jpeg.SequenceEqual(bytes.Take(jpeg.Length)))
                    return ImageFormat.jpeg;

                if (jpeg2.SequenceEqual(bytes.Take(jpeg2.Length)))
                    return ImageFormat.jpeg;

                return ImageFormat.unknown;
            }
        }
```


After we save the file, lets add the stuff the controller to actually accept the file.

``` C#
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using content.ImageHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace content.Controllers
{
    [Route("api/[controller]")]
    public class ImageController : Controller
    {

        private readonly IImageHandler _imageHandler;

        public ImageController(IImageHandler imageHandler, )
        {
            _imageHandler = imageHandler;
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
            return Ok(path);
        }

    }
}


```

Also you will need to add an `images` in a `wwwroot` folder.  Add an empty `.keep` file in the `images` folder. This will make sure the `image` gets into source control, but not the images

To keep our git clean also add

```
wwwroot/images/* 
!wwwroot/images/.keep
``` 

to your get ignore

> Code check!
After you have added the above code, figure out the references, you should be able to post a file to the API and you should be get a file path of the image returned. The image that you uploaded should be at the path. If its not, double check what was added. 

**Do not continue until you get that working**

### Step 2 Cloudinary Integrations

Now we need to say the image downloaded to a third party service. We will be using [Cloudinary](https://cloudinary.com). This is a great free and easy to use service. 


Add the package:

``` bash 
dotnet add package CloudinaryDotNet --version 1.6.0
```

And now add this class to the ImageUtilities Namespace:

``` C# 

using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace ImageTest1.ImageUtilities
{
    public class CloudinaryStorage
    {

        private Cloudinary _cloudinary;
        public CloudinaryStorage(CloudinaryKeys creds)
        {
            Account account = new Account(
                  creds.CloudName,
                 creds.CloudKey,
                  creds.CloudSecret);

            _cloudinary = new Cloudinary(account);

        }

        public ImageUploadResult UploadFile(string path)
        {
            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(path)
            };
            var uploadResult = _cloudinary.Upload(uploadParams);
            return uploadResult;
        }
    }

    public class CloudinaryKeys
    {
        public string CloudName { get; set; }
        public string CloudKey { get; set; }
        public string CloudSecret { get; set; }
    }
}

``` 

With this  you will have to add a few more things to the configuration of the app.

First add another dependency to be injected

```
services.Configure<CloudinaryKeys>(opts => Configuration.Bind(opts));
```


This dependency allows you to configure the keys for your Cloudinary account. Grab the `Cloud Name`, `API Secret` and `API Key` from your account. 

[Turn on the user secret for your project](https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-2.2&tabs=macos#set-a-secret)

Then you need to add 3 secret keys to your environment.

``` bash
dotnet user-secrets set "CloudSecret" "YOUR_SECRET_HERE"
dotnet user-secrets set "CloudKey" "YOUR_KEY_HERE"
dotnet user-secrets set "CloudName" "YOUR_CLOUD_NAME"
```

After we get our Cloudinary set up, lets call this in our Controller. Update Your controller to this: 

```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using imagetest1.ImageUtilities;
using imagetest1.Interfaces;
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

        public ImageController(IImageHandler imageHandler, IOptions<CloudinaryKeys> options)
        {
            _imageHandler = imageHandler;
            _options = options;
            Console.WriteLine(_options.Value.CloudName);
            
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
            return Ok(rv);
        }


    }
}

```


> Code Check
After you have updated your controller, you should be able to see the file being uploaded to Cloudinary. 


## Step 3 

Next we need to take that object from Cloudinary and save it to our database. Add an `Image` Model that has all the properties you need, as well as an `Url` property. This `Url` property will be where the `Url` of the string will be saved. 

Sample Model:
``` C# 
  public class Image
  {
    public int Id { get; set; }
    public string Url { get; set; }
    public DateTime Created { get; set; } = DateTime.Now;
  }
```

And then use the model in your Controller

``` C# 
      public async Task<ActionResult> UploadImage(IFormFile file)
        {

            var path =  await _imageHandler.UploadImage(file);
            var rv = new CloudinaryStorage(_options.Value).UploadFile(path);
            
            var image = new Image{
                Url = rv.SecureUri.AbsoluteUri
            };
            this._context.Images.Add(image);
            await this._context.SaveChangesAsync();
            
            return Ok(image);
        }

```
## Step 4 Clean Up!

After we have saved the image refernce to our database, we need to clean up the file what we created back in step 1. Lets add a new method to our `IImageHandler` called `DeleteFile` and call that from our controller


Update `IImageHandler` to:
```C#

 public interface IImageHandler
{
    Task<string> UploadImage(IFormFile file);
    Task DeleteFile(string path);
}

```

Update `ImageHandler` to:
```C# 
public class ImageHandler : IImageHandler
{
    private readonly IImageWriter _imageWriter;
    public ImageHandler(IImageWriter imageWriter)
    {
        _imageWriter = imageWriter;
    }

    public async Task DeleteFile(string path)
    {
        File.Delete(path);
    }

    public async Task<string> UploadImage(IFormFile file)
    {
        var result = await _imageWriter.UploadImage(file);
        return result;
    }
}
```


Update the Image Controller:
``` C#

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

```