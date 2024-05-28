using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(gNxt_HR_Dashboard.Startup))]
namespace gNxt_HR_Dashboard
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
