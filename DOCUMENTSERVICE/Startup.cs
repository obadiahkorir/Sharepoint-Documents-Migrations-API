using Hangfire;
using Microsoft.Owin;
using Owin;
using System;
using System.Web.Hosting;

[assembly: OwinStartupAttribute(typeof(DOCUMENTSERVICE.Startup))]
namespace DOCUMENTSERVICE
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            //providing database name to save jobs etc
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");
        }
    }
}
