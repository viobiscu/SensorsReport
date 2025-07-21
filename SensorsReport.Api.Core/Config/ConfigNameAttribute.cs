namespace SensorsReport;

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class ConfigNameAttribute : Attribute
{
    public string Name { get; }
    public ConfigNameAttribute(string name)
    {
        Name = name;
    }
}