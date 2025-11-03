namespace FootballField.API.Storage

{
    public class MinioSettings
    {
        public string Endpoint { get; set; } = default!;
        public string AccessKey { get; set; } = default!;
        public string SecretKey { get; set; } = default!;
        public string BucketName { get; set; } = default!;
        public bool WithSSL { get; set; }
    }
}
