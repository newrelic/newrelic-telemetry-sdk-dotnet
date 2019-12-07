using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;



namespace SampleAspNetFrameworkApp
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            var apiKey = ConfigurationManager.AppSettings["NewRelic.Telemetry.ApiKey"];





        }
    }
}
