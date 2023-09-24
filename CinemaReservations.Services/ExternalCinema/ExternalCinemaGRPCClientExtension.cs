using CinemaReservations.Services.ExternalCinema.Interfaces;
using Grpc.Core;
using Grpc.Net.Client.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProtoDefinitions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace CinemaReservations.Services.ExternalCinema
{
    public static class ExternalCinemaGrpcClientExtension
    {
        public static void AddExternalCinemaGrpcClient(this IServiceCollection services, IConfiguration configuration)
        {
            var apiKey = configuration["ExternalCinemaGrpc:ApiKey"];
            var address = configuration["ExternalCinemaGrpc:Address"];

            var defaultMethodConfig = new MethodConfig
            {
                Names = { MethodName.Default },
                RetryPolicy = new RetryPolicy
                {
                    MaxAttempts = 5,
                    InitialBackoff = TimeSpan.FromSeconds(1),
                    MaxBackoff = TimeSpan.FromSeconds(5),
                    BackoffMultiplier = 1.5,
                    RetryableStatusCodes = { StatusCode.Unavailable }
                }
            };

            services.AddGrpcClient<MoviesApi.MoviesApiClient>(o =>
            {
                o.Address = new Uri(address);
            }).ConfigurePrimaryHttpMessageHandler(() =>
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator                  
                };
                return handler;
            })
            .AddCallCredentials((context, metadata) =>
            {
                metadata.Add("X-Apikey", apiKey);
                return Task.CompletedTask;
            })
            .ConfigureChannel(configureChannel =>
            {
                configureChannel.ServiceConfig = new ServiceConfig
                {
                    MethodConfigs = { defaultMethodConfig }
                };
            });

            services.AddTransient<IExternalCinemaGrpcService, ExternalCinemaGrpcService>();
        }
    }
}
