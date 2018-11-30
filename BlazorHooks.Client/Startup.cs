using Microsoft.AspNetCore.Blazor.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorHooks.Client
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new DomWindow());
            services.AddSingleton(new LocaleInfo { LocaleName = "en-us" });
            services.AddSingleton(new ThemeInfo {
                BgClassDemo2 = "bg-gold",
                BgClassDemo3 = "bg-light-yellow",
                BgClassDemo4 = "bg-light-blue",
                BgClassDemo5 = "bg-green"
            });
        }

        public void Configure(IBlazorApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
