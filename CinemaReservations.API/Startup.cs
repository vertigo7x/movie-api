using CinemaReservations.Application.Extensions;
using CinemaReservations.Services.Cache;
using CinemaReservations.Services.DataBase;
using CinemaReservations.Services.ExternalCinema;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace ApiApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddInMemoryDB();

            services.AddControllers();

            services.AddHttpClient();

            services.AddExternalCinemaGrpcClient(Configuration);

            services.AddRedisClient(Configuration);

            services.AddApplicationComponents();

            services.AddSwaggerGen(options =>
            {
                var ver = "v1";

                options.SwaggerDoc(ver, new OpenApiInfo
                {
                    Title = $"Cinema Reservations {ver}",
                    Version = ver,
                    Description = "Cinema Reservations"
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Foo API V1");
            });

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.AddSampleData();
        }
    }
}
