using Web.Models;

public interface ITenantService
{
	Task<TenantInfo> GetTenantInfoAsync(string userId);
}