using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Stripe;

namespace Stripe
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            StripeConfiguration.ApiKey = "sk_test_51Owfp3SE9u5I1bSOo0Rz2i93YLXyQiV5c6aiLeP8drKtqkIdFO42I3hKFvSeoOXmQQhjTzY2pleLi4mEASJ0zlnJ00chXoREMJ";
        }
    }
}
