using CryptoExchange.Net;
using CryptoExchange.Net.Clients;
using CryptoExchange.Net.Interfaces;
using Gemini.Net;
using Gemini.Net.Clients;
using Gemini.Net.Interfaces;
using Gemini.Net.Interfaces.Clients;
using Gemini.Net.Objects.Options;
using Gemini.Net.SymbolOrderBooks;
using System;
using System.Net;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extensions for DI
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add the IGeminiClient and IGeminiSocketClient to the sevice collection so they can be injected
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <param name="defaultRestOptionsDelegate">Set default options for the rest client</param>
        /// <param name="defaultSocketOptionsDelegate">Set default options for the socket client</param>
        /// <param name="socketClientLifeTime">The lifetime of the IGeminiSocketClient for the service collection. Defaults to Singleton.</param>
        /// <returns></returns>
        public static IServiceCollection AddGemini(
            this IServiceCollection services,
            Action<GeminiRestOptions>? defaultRestOptionsDelegate = null,
            Action<GeminiSocketOptions>? defaultSocketOptionsDelegate = null,
            ServiceLifetime? socketClientLifeTime = null)
        {
            var restOptions = GeminiRestOptions.Default.Copy();

            if (defaultRestOptionsDelegate != null)
            {
                defaultRestOptionsDelegate(restOptions);
                GeminiRestClient.SetDefaultOptions(defaultRestOptionsDelegate);
            }

            if (defaultSocketOptionsDelegate != null)
                GeminiSocketClient.SetDefaultOptions(defaultSocketOptionsDelegate);

            services.AddHttpClient<IGeminiRestClient, GeminiRestClient>(options =>
            {
                options.Timeout = restOptions.RequestTimeout;
            }).ConfigurePrimaryHttpMessageHandler(() => {
                var handler = new HttpClientHandler();
                if (restOptions.Proxy != null)
                {
                    handler.Proxy = new WebProxy
                    {
                        Address = new Uri($"{restOptions.Proxy.Host}:{restOptions.Proxy.Port}"),
                        Credentials = restOptions.Proxy.Password == null ? null : new NetworkCredential(restOptions.Proxy.Login, restOptions.Proxy.Password)
                    };
                }
                return handler;
            });

            services.AddTransient<ICryptoRestClient, CryptoRestClient>();
            services.AddTransient<ICryptoSocketClient, CryptoSocketClient>();
            services.AddTransient<IGeminiOrderBookFactory, GeminiOrderBookFactory>();
            services.AddTransient<IGeminiTrackerFactory, GeminiFactory>();
            services.AddTransient(x => x.GetRequiredService<IGeminiRestClient>().SpotApi.CommonSpotClient);
            services.AddTransient(x => x.GetRequiredService<IGeminiRestClient>().FuturesApi.CommonFuturesClient);

            services.RegisterSharedRestInterfaces(x => x.GetRequiredService<IGeminiRestClient>().SpotApi.SharedClient);
            services.RegisterSharedSocketInterfaces(x => x.GetRequiredService<IGeminiSocketClient>().SpotApi.SharedClient);
            services.RegisterSharedRestInterfaces(x => x.GetRequiredService<IGeminiRestClient>().FuturesApi.SharedClient);
            services.RegisterSharedSocketInterfaces(x => x.GetRequiredService<IGeminiSocketClient>().FuturesApi.SharedClient);

            if (socketClientLifeTime == null)
                services.AddSingleton<IGeminiSocketClient, GeminiSocketClient>();
            else
                services.Add(new ServiceDescriptor(typeof(IGeminiSocketClient), typeof(GeminiSocketClient), socketClientLifeTime.Value));
            return services;
        }
    }
}
