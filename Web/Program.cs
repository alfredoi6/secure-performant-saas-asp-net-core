using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Factories;
using Web.Managers;
using Web.Services;

namespace Web
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// --------------------------------------------------------------
			// 1. Database Configuration
			// --------------------------------------------------------------
			// Retrieve the connection string from appsettings.json or the
			// environment. Throw an exception if it's not found, ensuring
			// we don't run without a valid DB connection.
			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
			                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

			// Register the DbContext, specifying SQL Server and the connection string.
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(connectionString));

			// Show detailed database errors (e.g., migrations) during development.
			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			// --------------------------------------------------------------
			// 2. Identity Configuration
			// --------------------------------------------------------------
			// AddDefaultIdentity sets up basic ASP.NET Core Identity features
			// for IdentityUser, without role support. We also configure the
			// account confirmation requirement for sign-in.
			builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
				{
					options.SignIn.RequireConfirmedAccount = true;
				})
				
				.AddDefaultTokenProviders()
			// Register our custom claims principal factory. This replaces
			// the default claims factory with one that can enrich user
			// identities with tenant-specific claims.
			.AddClaimsPrincipalFactory<TenantUserClaimsPrincipalFactory>()
			// Link the Identity system to the EF Core context for storing and
			// retrieving identity data (users, passwords, etc.).
			.AddEntityFrameworkStores<ApplicationDbContext>();


			// --------------------------------------------------------------
			// 3. Cookie Configuration
			// --------------------------------------------------------------
			// Override the default naming (.AspNetCore.Identity.Application)
			// for the auth cookie to hide ASP.NET Core specifics.
			builder.Services.ConfigureApplicationCookie(options =>
			{
				options.Cookie.Name = "i6AuthCookie";
			});

			// Configure Antiforgery token cookie name for similar reasons.
			builder.Services.AddAntiforgery(options =>
			{
				options.Cookie.Name = "i6Antiforgery";
			});

			// Configure the TempData cookie. By default, ASP.NET uses
			// .AspNetCore.Mvc.CookieTempDataProvider � renaming it further
			// obscures the tech stack signature.
			builder.Services.AddControllersWithViews()
				.AddCookieTempDataProvider(options =>
				{
					options.Cookie.Name = "i6TempDataCookie";
				});

			builder.Services.AddRazorPages();
			// --------------------------------------------------------------
			// 4. Application Services
			// --------------------------------------------------------------
			// TenantService handles tenant-specific logic (e.g., fetching
			// tenant info from the database). We register it so our
			// custom claims principal factory can inject and use it.
			builder.Services.AddScoped<ITenantService, TenantService>();

			builder.Services.AddScoped<StripeUserManager>(); // Keep this
			builder.Services.AddScoped<UserManager<ApplicationUser>, StripeUserManager>(); // Explicit replacement
			builder.Services.AddScoped<IStripeIntegrationService, StripeIntegrationService>();
			builder.Services.AddTransient<IEmailSender, PapercutEmailSenderService>();



			// --------------------------------------------------------------
			// 5. Build the Application
			// --------------------------------------------------------------
			var app = builder.Build();


			// --------------------------------------------------------------
			// 6. HTTP Request Pipeline
			// --------------------------------------------------------------
			// Use specialized pages for exceptions and migrations
			// when in development mode. Otherwise, use production patterns.
			if (app.Environment.IsDevelopment())
			{
				app.UseMigrationsEndPoint();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts(); // Adds Strict-Transport-Security header
			}

			// Enforce HTTPS for better security.
			app.UseHttpsRedirection();

			// Middleware to remove server-identifying headers.
			app.Use(async (context, next) =>
			{
				context.Response.Headers.Remove("Server");
				context.Response.Headers.Remove("X-Powered-By");
				await next.Invoke();
			});

			// Set up routing so the app knows how to match incoming requests.
			app.UseRouting();

			// Authentication and Authorization must come after routing but
			// before endpoints are mapped.
			app.UseAuthentication();
			app.UseAuthorization();

			// Map static files and default routes to controllers and Razor Pages.
			app.MapStaticAssets();
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}")
				.WithStaticAssets();
			app.MapRazorPages()
				.WithStaticAssets();
			// --------------------------------------------------------------
			// 7. Apply Database Migrations Automatically
			// --------------------------------------------------------------
			using (var scope = app.Services.CreateScope())
			{
				var services = scope.ServiceProvider;
				try
				{
					var dbContext = services.GetRequiredService<ApplicationDbContext>();
					dbContext.Database.Migrate(); // Apply pending migrations automatically
				}
				catch (Exception ex)
				{
					var logger = services.GetRequiredService<ILogger<Program>>();
					logger.LogError(ex, "An error occurred while applying database migrations.");
					throw; // Ensure the error is logged and doesn't go unnoticed
				}
			}
			
			// --------------------------------------------------------------
			// 7. Start the Application
			// --------------------------------------------------------------
			app.Run();
		}
	}
}
