using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;
using RoboticArmTableCore;
using System.Collections.Generic;

[assembly: OwinStartup(typeof(RoboticArmTableMVC.Startup))]

namespace RoboticArmTableMVC
{
    public partial class Startup
    {
        

        public void Configuration(IAppBuilder app)
        {
            //GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => JsonSerializer.Create(new JsonSerializerSettings()));

            app.MapSignalR(new HubConfiguration() { 
                EnableDetailedErrors = true 
            });
        }
    }
}