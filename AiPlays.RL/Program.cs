using AiPlays.Brain;
using AiPlays.Brain.Services;
using AiPlays.Core.Interfaces;
using AiPlays.Perception.Services;
using AiPlays.Pilot;
using AiPlays.Pilot.Services;
using AiPlays.RL;
using AiPlays.RL.Environments;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RLMatrix;
using RLMatrix.Agents.Common;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BrainSettings>(builder.Configuration.GetSection("Settings"));
builder.Services.Configure<EmulatorSettings>(builder.Configuration.GetSection("EmulatorSettings"));

builder.Services.AddSingleton<CommandQueueService>();
builder.Services.AddSingleton<ScreenshotService>();
builder.Services.AddSingleton<Emulator>();

builder.Services.AddSingleton<IPilot, PilotService>();
builder.Services.AddSingleton<IPerception, PerceptionService>();
builder.Services.AddSingleton<TrainerService>();
builder.Services.AddSingleton<RLAgent>();

var app = builder.Build();

Emulator mgba = app.Services.GetRequiredService<Emulator>();
mgba.Start();


RLAgent agent = app.Services.GetRequiredService<RLAgent>();
agent.Start();

// builder.Configuration["gRPC"]
await app.RunAsync();














//// Set up how our AI will learn
//var learningSetup = new DQNAgentOptions(
//    batchSize: 32,      // Learn from 32 experiences at once
//    memorySize: 1000,   // Remember last 1000 attempts
//    gamma: 0.99f,       // Care a lot about future rewards
//    epsStart: 1f,       // Start by trying everything
//    epsEnd: 0.05f,      // Eventually stick to what works
//    epsDecay: 150f      // How fast to transition
//);

//// Create our environment
//var environment = new EmeraldEnvironment().RLInit();
//var env = new List<IEnvironmentAsync<float[]>> {
//    environment,
//    //new PatternMatchingEnvironment().RLInit() //you can add more than one to train in parallel
//};

//// Create our learning agent
//var agent = new LocalDiscreteRolloutAgent<float[]>(learningSetup, env);

//// Let it learn!
//for (int i = 0; i < 1000; i++)
//{
//    await agent.Step();

//    if ((i + 1) % 50 == 0)
//    {
//        Console.WriteLine($"Step {i + 1}/1000 - Last 50 steps accuracy: {environment.RecentAccuracy:F1}%");
//        environment.ResetStats();

//        Console.WriteLine("\nPress Enter to continue...");
//        Console.ReadLine();
//    }
//}

//Console.WriteLine("\nTraining complete!");
//Console.ReadLine();