using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Web.Factories;

public class TenantUserClaimsPrincipalFactory 
	: UserClaimsPrincipalFactory<IdentityUser>
{
	private readonly ITenantService _tenantService;

	public TenantUserClaimsPrincipalFactory(
		UserManager<IdentityUser> userManager,
		IOptions<IdentityOptions> optionsAccessor,
		ITenantService tenantService)
		: base(userManager, optionsAccessor)
	{
		_tenantService = tenantService;
	}

	protected override async Task<ClaimsIdentity> GenerateClaimsAsync(IdentityUser user)
	{
		// First, get the default claims identity (with name, roles, etc.)
		var identity = await base.GenerateClaimsAsync(user);

		// Then, call your TenantService to get the user's tenant info
		var tenantInfo = await _tenantService.GetTenantInfoAsync(user.Id);
		if (tenantInfo != null)
		{
			// 3. Add claims from tenant info
			identity.AddClaim(new Claim(TenantClaimTypes.UserId, tenantInfo.UserId ?? string.Empty));
			identity.AddClaim(new Claim(TenantClaimTypes.TenantId, tenantInfo.TenantId ?? string.Empty));
			identity.AddClaim(new Claim(TenantClaimTypes.TenantName, tenantInfo.TenantName ?? string.Empty));
		}

		return identity;
	}
}