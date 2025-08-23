namespace SensorsReport.Frontend.Membership;

public class SignUpResponse : ServiceResponse
{
    public string DemoActivationLink { get; set; }
    public bool NeedsActivation { get; set; } = true;
}