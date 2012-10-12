using System;
using System.Web.Hosting;
using RazorEngine.Web;

namespace RazorEngine.Website
{
    public class Global : System.Web.HttpApplication
    {
        void Application_Start(object sender, EventArgs e)
        {
            // Register the virtual path provider for medium trust usage
            HostingEnvironment.RegisterVirtualPathProvider(new RazorVirtualPathProvider());
        }
    }
}
