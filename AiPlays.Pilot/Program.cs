using AiPlays.Pilot;
using AiPlays.Pilot.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.Configure<EmulatorSettings>(context.Configuration.GetSection("EmulatorSettings"));

        services.AddSingleton<CommandQueueService>();
        services.AddHostedService(sp => sp.GetRequiredService<CommandQueueService>());

        services.AddSingleton<Emulator>();
    });

var app = builder.Build();
await app.StartAsync();

Emulator mgba = app.Services.GetRequiredService<Emulator>();
mgba.Start();

Console.WriteLine("Emulator started. Type 'exit' to leave.");

string input = "";
do
{
    input = Console.ReadLine();
    mgba.HandleManualInput(input);

} while (input.ToLower() != "exit");

await app.StopAsync();
