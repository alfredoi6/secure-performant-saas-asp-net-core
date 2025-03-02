namespace Web.Services;

public class StripeIntegrationService : IStripeIntegrationService
{
    public string RegisterNewStripeUser(string userEmail)
    {
        return $"mock_stripe_id_{userEmail}"; // Return a predictable mock ID
    }
}