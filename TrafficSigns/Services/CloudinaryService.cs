using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace TrafficSigns.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration configuration)
        {
            Account account = new()
            {
                Cloud = configuration[Constants.Constants.Cloudinary_CloudName],
                ApiKey = configuration[Constants.Constants.Cloudinary_ApiKey],
                ApiSecret = configuration[Constants.Constants.Cloudinary_ApiSecret],
            };

            _cloudinary = new Cloudinary(account);
        }

        public async Task<ImageUploadResult> UploadImage(IFormFile file)
        {
            if (file.Length > 0)
            {
                using Stream stream = file.OpenReadStream();

                ImageUploadParams uploadParams = new ()
                {
                    File = new FileDescription(file.FileName, stream)
                };

                ImageUploadResult uploadResult = await _cloudinary.UploadAsync(uploadParams);

                return uploadResult;
            }
            throw new Exception();   
        }
    }
}
