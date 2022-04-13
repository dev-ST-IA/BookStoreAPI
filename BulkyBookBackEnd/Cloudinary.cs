using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace BulkyBookBackEnd
{
    public class CloudinaryClass
    {
        public static string CloudName { get; set; }
        public static string ApiSecret { get; set; }
        public static string ApiKey { get; set; }

        public Cloudinary CloudinaryAdapter { get; set; }

        public CloudinaryClass()
        {
            Account account = new Account
            {
                Cloud = CloudName,
                 ApiSecret=ApiSecret,
                ApiKey=ApiKey
            };
            this.CloudinaryAdapter = new Cloudinary(account);
            this.CloudinaryAdapter.Api.Secure = true;

        }

        public async Task<string> BookImageUpload(string filePath,string fileName)
        {
            try
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(@$"{filePath}"),
                    PublicId = $"books/{fileName}",
                    Overwrite = true
                };
                var response = await this.CloudinaryAdapter.UploadAsync(uploadParams);
                var url = response.Url.ToString();
                return url;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
