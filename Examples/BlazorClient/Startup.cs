using System.Collections.Generic;
using Binance.Net;
using Binance.Net.Clients;
using Binance.Net.Interfaces.Clients;
using Bitfinex.Net;
using Bittrex.Net;
using Bybit.Net;
using CoinEx.Net;
using CryptoExchange.Net.Authentication;
using Huobi.Net;
using Kraken.Net;
using Kucoin.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlazorClient
{
    public class Startup
    {
        private ILoggerFactory _loggerFactory;

        public Startup(IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            Configuration = configuration;
            _loggerFactory = loggerFactory;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            // Register the clients, options can be provided in the callback parameter
            services.AddBinance(restOptions =>
            {
                restOptions.ApiCredentials = new ApiCredentials("KEY", "SECRET");
            }, socketOptions =>
            {
                socketOptions.ApiCredentials = new ApiCredentials("KEY", "SECRET");
            });

            services.AddBitfinex();
            services.AddBittrex();
            services.AddBybit();
            services.AddCoinEx();
            services.AddHuobi();
            services.AddKraken();
            services.AddKucoin();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
