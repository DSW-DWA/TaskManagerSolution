using LogServiceConsumer.Services;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) =>
    lc.WriteTo.Console()
      .WriteTo.File("logs/logconsumer-.log", rollingInterval: RollingInterval.Day));

var activitySource = new ActivitySource("LogServiceConsumer");
builder.Services.AddSingleton(activitySource);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("LogServiceConsumer"))
    .WithTracing(t =>
    {
        t.AddSource("LogServiceConsumer");

        t.AddOtlpExporter(opt =>
        {
            opt.Endpoint = new Uri("http://otel-collector:4317");
        });
    });

builder.Services.AddHostedService<RabbitMqConsumerService>();

var app = builder.Build();

app.Run();
