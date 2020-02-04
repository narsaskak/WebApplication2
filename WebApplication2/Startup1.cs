using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin;
using Owin;
using System.Web.Http.Owin;


[assembly: OwinStartup(typeof(WebApplication2.Startup1))]

namespace WebApplication2
{
    public class Startup1
    {
        static HttpClient forwardingClient = new HttpClient();

        public void Configuration(IAppBuilder app)
        {
            app.Use((context, next) =>
            {
                TextWriter output = context.Get<TextWriter>("host.TraceOutput");
                return next().ContinueWith(result =>
                {
                    output.WriteLine("Scheme {0} : Method {1} : Path {2} : MS {3}",
                    context.Request.Scheme, context.Request.Method, context.Request.Path, getTime());
                });
            });

            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultController",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
             );

            app.UseWebApi(config);
            //app.Run(async context =>
            //{
            //    await context.Response.WriteAsync(getTime() + " My First OWIN App");
            //});

            //app.Run(SendRequestToRestfulEndpoint);
        }

        string getTime()
        {
            return DateTime.Now.Millisecond.ToString();
        }

        private static HttpRequestMessage CreateRequestMessage(IOwinContext context, string redirectUri, string verb)
        {
            HttpRequestMessage message = new HttpRequestMessage(new HttpMethod(verb), redirectUri);

            // GET cannot have content.
            if (message.Method != HttpMethod.Get)
            {
                message.Content = new StreamContent(context.Request.Body);
            }

            return message;
        }

        private static async Task<HttpResponseMessage> SendRequestToRestfulEndpoint(IOwinContext context)
        {
            using (HttpRequestMessage message = CreateRequestMessage(context, context.Request.Uri.AbsoluteUri, context.Request.Method))
            {
                return await forwardingClient.SendAsync(message);
            }
        }



    }
}
