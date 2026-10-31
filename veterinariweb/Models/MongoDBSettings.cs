namespace veterinariweb.Models
{
    public class MongoDBSettings
    {
        public const string SectionName = "MongoDBSettings";

        public string ConnectionString { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

    }
}
