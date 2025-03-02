using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web.Data;
using Web.Factories;

namespace Web
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// 1. Configure the database context
			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
			                       ?? throw new InvalidOperationException(
				                       "Connection string 'DefaultConnection' not found.");
			builder.Services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(connectionString));
			builder.Services.AddDatabaseDeveloperPageExceptionFilter();

			// 1. Add Default Identity
			builder.Services.AddDefaultIdentity<IdentityUser>(options =>
				{
					options.SignIn.RequireConfirmedAccount = true;
				})
				// 2. Register the custom claims principal factory
				.AddClaimsPrincipalFactory<TenantUserClaimsPrincipalFactory>()
				// 3. Use your EF stores
				.AddEntityFrameworkStores<ApplicationDbContext>();

			// Rename the default auth cookie
			builder.Services.ConfigureApplicationCookie(options =>
			{
				// By default, this is ".AspNetCore.Identity.Application"; we'll replace it
				options.Cookie.Name = "i6AuthCookie";
			});

			builder.Services.AddAntiforgery(options => { options.Cookie.Name = "i6Antiforgery"; });


			// Add MVC with a custom Cookie TempData provider
			builder.Services.AddControllersWithViews()
				.AddCookieTempDataProvider(options =>
				{
					// The default is ".AspNetCore.Mvc.CookieTempDataProvider"
					options.Cookie.Name = "i6TempDataCookie";
				});

			builder.Services.AddScoped<ITenantService, TenantService>();

			var app = builder.Build();

			// 4. Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseMigrationsEndPoint();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days.
				app.UseHsts();
			}

			app.UseHttpsRedirection();

			// 5. Remove or modify server headers
			app.Use(async (context, next) =>
			{
				// Remove the "Server" header
				context.Response.Headers.Remove("Server");
				context.Response.Headers.Remove("X-Powered-By");

				await next.Invoke();
			});

			app.UseRouting();

			// 6. These need to be in place for auth to work properly
			app.UseAuthentication();
			app.UseAuthorization();

			// 7. Map routes and Razor Pages
			app.MapStaticAssets();
			app.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}")
				.WithStaticAssets();
			app.MapRazorPages()
				.WithStaticAssets();

			// 8. Run the app
			app.Run();
		}
	}
}
