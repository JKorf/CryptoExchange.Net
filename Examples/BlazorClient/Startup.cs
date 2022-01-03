using Binance.Net;
using Binance.Net.Clients;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Objects;
using Bitfinex.Net;
using Bitfinex.Net.Clients;
using Bitfinex.Net.Interfaces.Clients;
using Bittrex.Net;
using Bittrex.Net.Clients;
using Bittrex.Net.Interfaces.Clients;
using Bybit.Net;
using Bybit.Net.Clients;
using Bybit.Net.Interfaces.Clients;
using CoinEx.Net.Clients;
using CoinEx.Net.Interfaces.Clients;
using CryptoExchange.Net.Authentication;
using FTX.Net;
using FTX.Net.Clients;
using FTX.Net.Interfaces.Clients;
using Huobi.Net;
using Huobi.Net.Clients;
using Huobi.Net.Interfaces.Clients;
using Kraken.Net;
using Kraken.Net.Clients;
using Kraken.Net.Interfaces.Clients;
using Kucoin.Net;
using Kucoin.Net.Clients;
using Kucoin.Net.Interfaces.Clients;
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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();

            // Register the clients, options can be provided in the callback parameter
            services.AddBinance((restClientOptions, socketClientOptions) => {
                restClientOptions.ApiCredentials = new ApiCredentials("KEY", "SECRET");
                restClientOptions.LogLevel = LogLevel.Debug;

                socketClientOptions.ApiCredentials = new ApiCredentials("KEY", "SECRET");
            });
            services.AddBitfinex();
            services.AddBittrex();
            services.AddBybit();
            //services.AddCoinEx();
            services.AddFTX();
            services.AddHuobi();
            services.AddKraken();
            services.AddKucoin();

            // TODO remove when AddCoinEx fixed
            services.AddScoped<ICoinExClient, CoinExClient>();
            services.AddScoped<ICoinExSocketClient, CoinExSocketClient>();
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
