namespace CatShelter.Options
{
    public class S3Options
    {
        public string ServiceUrl { get; set; } = string.Empty;

        public string AccessKey { get; set; } = string.Empty;

        public string SecretKey { get; set; } = string.Empty;

        public string Bucket { get; set; } = string.Empty;

        public bool ForcePathStyle { get; set; }
    }
}
