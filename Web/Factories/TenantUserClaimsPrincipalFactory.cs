using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Web.Factories;

/// <summary>
/// This class is registered with ASP.NET Core Identity and is called
/// whenever a user signs in or their cookie is refreshed. It enriches
/// the principal (User) with additional claims from tenant-specific
/// data. By injecting <see cref="ITenantService"/>, we separate the
/// tenant retrieval logic from the Identity code, keeping our design
/// clean and modular.
///
/// When a user successfully authenticates (e.g., logs in, SSO, etc.),
/// the Identity system invokes this factory's <c>GenerateClaimsAsync</c>
/// method to build the user's claims. We start by calling the base
/// method to include default Identity claims (like username), then
/// enrich those claims with the tenant data returned by the service.
/// </summary>
public class TenantUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<IdentityUser>
{
	private readonly ITenantService _tenantService;

	/// <summary>
	/// Initializes a new instance of the <see cref="TenantUserClaimsPrincipalFactory"/>.
	/// </summary>
	/// <param name="userManager">
	/// Provides user-related actions (e.g., create, delete, validate, etc.).
	/// Typically used internally by <c>base.GenerateClaimsAsync</c>.
	/// </param>
	/// <param name="optionsAccessor">
	/// Supplies <see cref="IdentityOptions"/> which govern security settings,
	/// password complexity, lockout rules, etc.
	/// </param>
	/// <param name="tenantService">
	/// Custom service for fetching tenant data (e.g., from a database or external API).
	/// </param>
	public TenantUserClaimsPrincipalFactory(
		UserManager<IdentityUser> userManager,
		IOptions<IdentityOptions> optionsAccessor,
		ITenantService tenantService)
		: base(userManager, optionsAccessor)
	{
		_tenantService = tenantService;
	}

	/// <summary>
	/// Builds the <see cref="ClaimsIdentity"/> for the given user by combining
	/// default Identity claims with additional tenant claims. This is automatically
	/// called during sign-in, sign-up, and when the cookie is refreshed.
	/// </summary>
	/// <param name="user">The <see cref="IdentityUser"/> for whom claims are generated.</param>
	/// <returns>A <see cref="ClaimsIdentity"/> containing both default and tenant-specific claims.</returns>
	protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
	{
		// 1. Retrieve the base Identity claims (e.g., Name, UserId).
		var identity = await base.GenerateClaimsAsync(user);

		// 2. Query tenant data from our ITenantService using the user's unique ID.
		var tenantInfo = await _tenantService.GetTenantInfoAsync(user.Id);
		if (tenantInfo != null)
		{
			// 3. Append tenant-specific information as claims for quick lookup during each request.
			identity.AddClaim(new Claim(TenantClaimTypes.UserId, tenantInfo.UserId ?? string.Empty));
			identity.AddClaim(new Claim(TenantClaimTypes.TenantId, tenantInfo.TenantId ?? string.Empty));
			identity.AddClaim(new Claim(TenantClaimTypes.TenantName, tenantInfo.TenantName ?? string.Empty));
		}

		return identity;
	}
}
