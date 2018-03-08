using EventBus.Common;
using EventBus.Common.Subscriptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMqEventBus;
using RabbitMqEventBus.Config;
using Swashbuckle.AspNetCore.Swagger;

namespace EventPublisherApp
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
            services.AddMvc();
            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new Info { Title = "Event Publisher API", Version = "v1" }));
            //Configure EventBus
            RegisterEventBus(services);
        }

        private void RegisterEventBus(IServiceCollection services)
        {
            services.Configure<EventPublisherAppSettings>(Configuration);

            services.AddScoped<IEventBus, RabbitMqEventBus.EventBus>();
            services.AddScoped<ISubscriptionsManager, InMemorySubscriptionsManager>();
            services.AddSingleton(sp =>
            {
                var appSettings = sp.GetService<IOptions<EventPublisherAppSettings>>().Value;

                var retryCount = 20;
                if (!string.IsNullOrEmpty(Configuration["EventBusRetryCount"]))
                {
                    retryCount = int.Parse(Configuration["EventBusRetryCount"]);
                }

                return new RabbitMqConfig(appSettings.BrokerName, appSettings.ExchangeType, appSettings.QueueName, (uint)retryCount);
            });
            services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();
                    var rmqConfig = sp.GetService<RabbitMqConfig>();

                    var factory = new ConnectionFactory()
                    {
                        HostName = Configuration["EventBusConnection"]
                    };

                    if (!string.IsNullOrEmpty(Configuration["EventBusUserName"]))
                    {
                        factory.UserName = Configuration["EventBusUserName"];
                    }

                    if (!string.IsNullOrEmpty(Configuration["EventBusPassword"]))
                    {
                        factory.Password = Configuration["EventBusPassword"];
                    }

                    
                    return new DefaultRabbitMQPersistentConnection(factory, logger, rmqConfig);
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(c =>c.SwaggerEndpoint("/swagger/v1/swagger.json", "Event Publisher API V1"));

            app.UseMvc();
        }
    }
}
