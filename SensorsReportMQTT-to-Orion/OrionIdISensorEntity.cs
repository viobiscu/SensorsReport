public enum EntityType
{
    TG8W,
    TG8I,
    WiSensor,
    BlueTooth,
    TRS,
    Unknow
}

public abstract class ISensorEntity : Orionld
{
    public  string? Id { get; set; }

    public string? APIKey { get; set; }
    public abstract Task<string> Consume(string payload); // Fixed the syntax for async method
    public bool HeartBeat { get; set; }
    public bool Online { get; set; }
    public bool Validate { get; set; }
    public abstract bool IsValid();
    public abstract Task<string> CommitToOrion();
}
