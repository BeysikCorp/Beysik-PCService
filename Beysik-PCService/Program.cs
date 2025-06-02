using Beysik_Common;
using Beysik_PCService.Models;
using Beysik_PCService.Services;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Beysik_PCService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.Configure<ProductDatabaseSettings>(
            builder.Configuration.GetSection("ProductDatabase"));

            builder.Services.AddSingleton<RabbitMqHelper>(sp => new RabbitMqHelper(
                builder.Configuration.GetSection("RabbitMQ").GetSection("HostName").Value));

            //builder.Services.AddSingleton<RabbitMqConsumerService>(
            //);
            builder.Services.AddSingleton<RabbitMqEventAggregator>();

            builder.Services.AddHostedService<RabbitMqConsumerService>(sp =>
                new RabbitMqConsumerService(
                sp.GetRequiredService<RabbitMqHelper>(),
                sp.GetRequiredService<RabbitMqEventAggregator>(),
                "order.topc",
                ExchangeType.Topic,
                "*.fromorder"
                )
            );

            builder.Services.AddHostedService<RabbitMqConsumerService>(sp =>
                new RabbitMqConsumerService(
                sp.GetRequiredService<RabbitMqHelper>(),
                sp.GetRequiredService<RabbitMqEventAggregator>(),
                "order.topc",
                ExchangeType.Topic,
                "*.fromcart"
                )
            );

            builder.Services.AddHostedService<RabbitMqConsumerService>(sp =>
                new RabbitMqConsumerService(
                sp.GetRequiredService<RabbitMqHelper>(),
                sp.GetRequiredService<RabbitMqEventAggregator>(),
                "api",
                ExchangeType.Fanout,
                null
                )
            );

            builder.Services.AddHostedService<RabbitMqConsumerService>(sp =>
                new RabbitMqConsumerService(
                sp.GetRequiredService<RabbitMqHelper>(),
                sp.GetRequiredService<RabbitMqEventAggregator>(),
                "order",
                ExchangeType.Fanout,
                null
                )
            );

            builder.Services.AddSingleton<ProductService>();

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

