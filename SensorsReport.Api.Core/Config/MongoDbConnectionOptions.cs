namespace SensorsReport;

[ConfigName(SectionName)]
public class MongoDbConnectionOptions
{
    public const string SectionName = nameof(MongoDbConnectionOptions);
    public string? ConnectionString { get; set; }
    public string? DatabaseName { get; set; }
    public string? CollectionName { get; set; }
}
