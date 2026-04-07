using AiPlays.Brain;
using AiPlays.Brain.Services;
using AiPlays.Core.Interfaces;
using AiPlays.Perception.Services;
using AiPlays.Pilot;
using AiPlays.Pilot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGrpc();

builder.Services.Configure<BrainSettings>(builder.Configuration.GetSection("Settings"));
builder.Services.Configure<EmulatorSettings>(builder.Configuration.GetSection("EmulatorSettings"));

builder.Services.AddSingleton<CommandQueueService>();
builder.Services.AddSingleton<ScreenshotService>();
builder.Services.AddSingleton<Emulator>();

builder.Services.AddSingleton<IPilot, PilotService>();
builder.Services.AddSingleton<IPerception, PerceptionService>();

var app = builder.Build();

Emulator mgba = app.Services.GetRequiredService<Emulator>();
mgba.Start();

app.MapGrpcService<TrainerApiService>();
app.MapGet("/", () => "gRPC running...");
// builder.Configuration["gRPC"]
await app.RunAsync();
