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