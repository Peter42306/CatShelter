
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using CatShelter.Options;
using Microsoft.Extensions.Options;

namespace CatShelter.Services.PhotoStorage
{
    public class S3PhotoStorage : IPhotoStorage
    {
        private readonly AmazonS3Client _s3Client;
        private readonly S3Options _options;

        public S3PhotoStorage(IOptions<S3Options> options)
        {
            _options = options.Value;

            var credentials = new BasicAWSCredentials(
                _options.AccessKey,
                _options.SecretKey);

            var config = new AmazonS3Config
            {
                ServiceURL = _options.ServiceUrl,
                ForcePathStyle = _options.ForcePathStyle
            };

            _s3Client = new AmazonS3Client(
                credentials,
                config);
        }

        public async Task<string> UploadAsync(
            IFormFile file, 
            int animalId, 
            CancellationToken ct = default)
        {
            var extension = Path.GetExtension(file.FileName);

            var storageKey = $"animals/{animalId}/{Guid.NewGuid():N}{extension}";

            await using var stream = file.OpenReadStream();

            var request = new PutObjectRequest
            {
                BucketName = _options.Bucket,
                Key = storageKey,
                InputStream = stream,
                ContentType = file.ContentType
            };

            await _s3Client.PutObjectAsync(request, ct);

            return storageKey;
        }

        public async Task DeleteAsync(
            string storageKey, 
            CancellationToken ct = default)
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _options.Bucket,
                Key = storageKey
            };

            await _s3Client.DeleteObjectAsync(request, ct);
        }

        public string GetPublicUrl(string storageKey)
        {
            return $"{_options.ServiceUrl.TrimEnd('/')}/{_options.Bucket}/{storageKey}";
        }
    }
}
