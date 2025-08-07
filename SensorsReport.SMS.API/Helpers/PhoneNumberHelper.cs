namespace SensorsReport;

public static class PhoneNumberHelper
{
    public const string DefaultCountryCode = "RO";

    public static string GetCountryCode(string? phoneNumber)
    {
        var phoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
        try
        {
            var parsedPhoneNumber = phoneNumberUtil.Parse(phoneNumber, null);
            if (phoneNumberUtil.IsValidNumber(parsedPhoneNumber))
                return phoneNumberUtil.GetRegionCodeForNumber(parsedPhoneNumber); ;
        }
        catch (PhoneNumbers.NumberParseException)
        {
        }

        return DefaultCountryCode;
    }
}
