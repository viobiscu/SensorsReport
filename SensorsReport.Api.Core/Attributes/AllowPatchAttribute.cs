using System;

namespace SensorsReport;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class AllowPatchAttribute : Attribute
{

}
