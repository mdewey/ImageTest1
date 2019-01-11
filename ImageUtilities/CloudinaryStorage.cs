using System;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace imagetest1.ImageUtilities
{
    public class CloudinaryStorage
    {

        private Cloudinary _cloudinary;
        public CloudinaryStorage(CloudinaryKeys creds)
        {
            Account account = new Account(
                  creds.CloudName ?? Environment.GetEnvironmentVariable("CLOUD_NAME"),
                 creds.CloudKey ?? Environment.GetEnvironmentVariable("CLOUD_KEY"),
                  creds.CloudSecret?? Environment.GetEnvironmentVariable("CLOUD_SECRET"));

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
        public string CloudName { get; set; } = null;
        public string CloudKey { get; set; } = null;
        public string CloudSecret { get; set; } = null;
    }
}