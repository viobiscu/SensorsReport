namespace SensorsReport.SMS.API.Models;

public static class SmsStatus
{
    public static SmsStatusEnum Default => SmsStatusEnum.Pending;
    public static bool IsValidStatus(SmsStatusEnum status)
    {
        return Enum.IsDefined(typeof(SmsStatusEnum), status);
    }

    public static string GetStatusMessage(SmsStatusEnum status)
    {
        return status switch
        {
            SmsStatusEnum.Pending => "The SMS is pending sending.",
            SmsStatusEnum.Sent => "The SMS was successfully sent.",
            SmsStatusEnum.Failed => "The SMS failed to send.",
            SmsStatusEnum.Expired => "The SMS delivery has expired.",
            SmsStatusEnum.Error => "An error occurred while sending the SMS.",
            SmsStatusEnum.Unknown => "The status of the SMS is unknown.",
            _ => "Invalid status."
        };
    }
}
