using System;

namespace SensorsReport;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class PatchRequestBodyAttribute : Attribute
{
    public Type ModelType { get; }
    public PatchRequestBodyAttribute(Type modelType)
    {
        ModelType = modelType;
    }
}
