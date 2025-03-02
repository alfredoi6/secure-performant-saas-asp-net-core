using Microsoft.AspNetCore.Identity;

namespace Web.Data;

public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Stores the Stripe customer ID associated with this user.
    /// </summary>
    public string StripeCustomerId { get; set; }
}