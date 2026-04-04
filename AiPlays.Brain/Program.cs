using AiPlays.Brain;
using AiPlays.Pilot;
using AiPlays.Pilot.Services;
using AiPlays_Perception;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tensorflow.Contexts;


var builder = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.Configure<BrainSettings>(context.Configuration.GetSection("Settings"));
        services.Configure<EmulatorSettings>(context.Configuration.GetSection("EmulatorSettings"));

        services.AddSingleton<CommandQueueService>();
        services.AddHostedService(sp => sp.GetRequiredService<CommandQueueService>());

        services.AddSingleton<ScreenshotService>();
        services.AddHostedService(sp => sp.GetRequiredService<ScreenshotService>());

        services.AddSingleton<Emulator>();
        services.AddSingleton<Brain>();
    });

var app = builder.Build();
await app.StartAsync();

Emulator mgba = app.Services.GetRequiredService<Emulator>();
mgba.Start();

Brain brain = app.Services.GetRequiredService<Brain>();
brain.Start();


// Action

Console.WriteLine("Brain started. Type 'exit' to exit.");
string input = "";
do
{
    input = Console.ReadLine();
} while (input.ToLower() != "exit");

await app.StopAsync();
