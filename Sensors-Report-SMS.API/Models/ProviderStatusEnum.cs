
using System.ComponentModel;

namespace Sensors_Report_SMS.API.Models;

public enum ProviderStatusEnum
{
    [Description("Unavailable")]
    Unavailable,
    [Description("Active")]
    Active
}