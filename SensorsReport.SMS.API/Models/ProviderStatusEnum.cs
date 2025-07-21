
using System.ComponentModel;

namespace SensorsReport.SMS.API.Models;

public enum ProviderStatusEnum
{
    [Description("Unavailable")]
    Unavailable,
    [Description("Active")]
    Active
}