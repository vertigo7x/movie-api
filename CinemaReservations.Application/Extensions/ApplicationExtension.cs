using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace CinemaReservations.Application.Extensions
{
    public static class ApplicationExtension
    {
        public static void AddApplicationComponents(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
