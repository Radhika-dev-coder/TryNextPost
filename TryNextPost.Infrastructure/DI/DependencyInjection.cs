using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Application.IServices.Interface.Courier;
using TryNextPost.Application.IServices.Interface.ICourier;
using TryNextPost.Infrastructure.CourierAdapters;
using TryNextPost.Infrastructure.Service;

namespace TryNextPost.Infrastructure.DI
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services)
        {
            services.AddTransient<ICourierService, CourierService>();

            services.AddTransient<ICourierAdapter, DelhiveryAdapter>();
            services.AddTransient<ICourierAdapter, BlueDartAdapter>();

            // XpressBees HttpClient
            services.AddHttpClient<XpressbeesAdapter>()
                .ConfigurePrimaryHttpMessageHandler(() =>
                    new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler
                                .DangerousAcceptAnyServerCertificateValidator
                    });

            // Register same typed adapter as ICourierAdapter
            services.AddTransient<ICourierAdapter>(sp =>
                sp.GetRequiredService<XpressbeesAdapter>());

            services.AddTransient<ICourierAdapter, DtdcAdapter>();
            services.AddTransient<ICourierAdapter, EkartAdapter>();
            services.AddTransient<ICourierAdapter, IndiaPostAdapter>();
            services.AddTransient<ICourierAdapter, ShadowfaxAdapter>();

            return services;
        }
    }
}
