using Microsoft.Owin;

using PhotoBooth.WebApp;

[assembly: OwinStartup(typeof(Startup))]
namespace PhotoBooth.WebApp
{
    using System.Configuration;

    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    using Owin;

    using PhotoBooth.DAL;
    using PhotoBooth.WebApp.Models;

    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            this.ConfigureAuth(app);

            using (var context = new PhotoBoothContext())
            {
            }

            using (var context = new ApplicationDbContext())
            {
                var roleStore = new RoleStore<IdentityRole>(context);
                var roleManager = new RoleManager<IdentityRole>(roleStore);

                roleManager.Create(new IdentityRole("Admin"));

                var userStore = new UserStore<ApplicationUser>(context);
                var userManager = new UserManager<ApplicationUser>(userStore);

                var adminEmail = ConfigurationManager.AppSettings.Get("AdminEmail");
                if (adminEmail != null)
                {
                    var user = userManager.FindByEmail(adminEmail);
                    if (user == null)
                    {
                        user = new ApplicationUser { UserName = adminEmail, Email = adminEmail };

                        var result = userManager.Create(user, adminEmail);
                        if (result.Succeeded)
                        {
                            userManager.AddToRole(user.Id, "Admin");
                            context.SaveChanges();
                        }
                    }
                }
            }
        }
    }
}
