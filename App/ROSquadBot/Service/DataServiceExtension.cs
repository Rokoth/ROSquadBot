using Microsoft.Extensions.DependencyInjection;

namespace ROTGBot.Service
{
    public static class DataServiceExtension
    {
        public static IServiceCollection AddDataServices(this IServiceCollection services)
        {
            services.AddScoped<IUserDataService, UserDataService>();
            services.AddScoped<ICommandDataService, CommandDataService>();

            services.AddScoped<ITelegramMainService, TelegramMainService>();
            services.AddScoped<ITelegramMessageHandler, TelegramMessageHandler>();
            services.AddScoped<ITelegramBotWrapper, TelegramBotWrapper>();
            
            services.AddHostedService<TelegramHostedService>();
            return services;
        }


    }
}
