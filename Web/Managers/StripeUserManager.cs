using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Web.Data;

namespace Web.Managers;

public class StripeUserManager : UserManager<ApplicationUser>
{
    private readonly IStripeIntegrationService _stripeIntegrationService;
    private readonly ApplicationDbContext _dbContext;

    public StripeUserManager(
        IUserStore<ApplicationUser> store,
        IOptions<IdentityOptions> optionsAccessor,
        IPasswordHasher<ApplicationUser> passwordHasher,
        IEnumerable<IUserValidator<ApplicationUser>> userValidators,
        IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        IServiceProvider services,
        ILogger<UserManager<ApplicationUser>> logger,
        IStripeIntegrationService stripeIntegrationService,
        ApplicationDbContext dbContext) 
        : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
    {
        _stripeIntegrationService = stripeIntegrationService;
        _dbContext = dbContext;
    }

    public override async Task<IdentityResult> CreateAsync(ApplicationUser user, string password)
    {
        var result = await base.CreateAsync(user, password);
        if (result.Succeeded)
        {
            // Register the user in Stripe
            string stripeCustomerId = _stripeIntegrationService.RegisterNewStripeUser(user.Email);
            
            // Update user record with StripeCustomerId
            user.StripeCustomerId = stripeCustomerId;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }
        return result;
    }

    public override async Task<IdentityResult> CreateAsync(ApplicationUser user)
    {
        var result = await base.CreateAsync(user);
        if (result.Succeeded)
        {
            string stripeCustomerId = _stripeIntegrationService.RegisterNewStripeUser(user.Email);
            user.StripeCustomerId = stripeCustomerId;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
        }
        return result;
    }
}