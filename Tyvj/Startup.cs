using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(Tyvj.Startup))]
namespace Tyvj
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            
            app.MapSignalR();
        }
    }
}
