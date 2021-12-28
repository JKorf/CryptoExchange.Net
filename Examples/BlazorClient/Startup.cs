using Binance.Net.Clients;
using Binance.Net.Interfaces.Clients;
using Binance.Net.Objects;
using Bitfinex.Net.Clients;
using Bitfinex.Net.Interfaces.Clients;
using Bittrex.Net.Clients;
using Bittrex.Net.Interfaces.Clients;
using Bybit.Net.Clients;
using Bybit.Net.Interfaces.Clients;
using CoinEx.Net.Clients;
using CoinEx.Net.Interfaces.Clients;
using CryptoExchange.Net.Authentication;
using FTX.Net.Clients;
using FTX.Net.Interfaces.Clients;
using Huobi.Net.Clients;
using Huobi.Net.Interfaces.Clients;
using Kraken.Net.Clients;
using Kraken.Net.Interfaces.Clients;
using Kucoin.Net.Clients;
using Kucoin.Net.Interfaces.Clients;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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

            // Register the rest clients as transient, will create a new one when needed
            services.AddTransient<IBinanceClient, BinanceClient>();
            services.AddTransient<IBitfinexClient, BitfinexClient>();
            services.AddTransient<IBittrexClient, BittrexClient>();
            services.AddTransient<IBybitClient, BybitClient>();
            services.AddTransient<ICoinExClient, CoinExClient>();
            services.AddTransient<IFTXClient, FTXClient>();
            services.AddTransient<IHuobiClient, HuobiClient>();
            services.AddTransient<IKrakenClient, KrakenClient>();
            services.AddTransient<IKucoinClient, KucoinClient>();

            // Register the socket clients as scoped so the same instance will be reused per client
            services.AddScoped<IBinanceSocketClient, BinanceSocketClient>();
            services.AddScoped<IBitfinexSocketClient, BitfinexSocketClient>();
            services.AddScoped<IBittrexSocketClient, BittrexSocketClient>();
            services.AddScoped<IBybitSocketClient, BybitSocketClient>();
            services.AddScoped<ICoinExSocketClient, CoinExSocketClient>();
            services.AddScoped<IFTXSocketClient, FTXSocketClient>();
            services.AddScoped<IHuobiSocketClient, HuobiSocketClient>();
            services.AddScoped<IKrakenSocketClient, KrakenSocketClient>();
            services.AddScoped<IKucoinSocketClient, KucoinSocketClient>();

            // Can set default client options here, for instance:
            BinanceClient.SetDefaultOptions(new BinanceClientOptions
            {
                ApiCredentials = new ApiCredentials("KEY", "SECRET")
            });
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
