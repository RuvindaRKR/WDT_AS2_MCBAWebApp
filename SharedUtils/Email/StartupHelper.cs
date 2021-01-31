using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharedUtils.Email
{
    public static class StartupHelper
    {
        public static IServiceCollection AddEmailSender(this IServiceCollection services,IConfiguration config)
        {
            services.AddScoped<IEmailSender, EmailSender>();
            services.Configure<SmtpSettings>(config.GetSection(nameof(SmtpSettings)));
            return services;
        }
    }
}
