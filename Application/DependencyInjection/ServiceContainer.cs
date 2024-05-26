using Application.Service;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DependencyInjection
{
    public static class ServiceContainer
    {
        public static IServiceCollection AddAplicationService(this IServiceCollection services)
        {
            services.AddScoped<IAcountService, AcountService>();
            return services;
        }
    }
}
