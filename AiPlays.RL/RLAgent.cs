using AiPlays.Brain.Services;
using AiPlays.RL.Environments;
using RLMatrix;
using RLMatrix.Agents.Common;

namespace AiPlays.RL
{
    public class RLAgent
    {
        private readonly DQNAgentOptions _learningSetup;
        private readonly TrainerService _trainerService;

        public RLAgent(TrainerService trainerService)
        {
            _learningSetup = new DQNAgentOptions(
                batchSize: 32,      // Learn from 32 experiences at once
                memorySize: 1000,   // Remember last 1000 attempts
                gamma: 0.99f,       // Care a lot about future rewards
                epsStart: 1f,       // Start by trying everything
                epsEnd: 0.05f,      // Eventually stick to what works
                epsDecay: 150f      // How fast to transition
            );

            _trainerService = trainerService;
        }

        public async Task Start()
        {
            // Create our environment
            var environment = new EmeraldEnvironment(_trainerService).RLInit();
            var env = new List<IEnvironmentAsync<float[]>> {
                environment,
                //new PatternMatchingEnvironment().RLInit() //you can add more than one to train in parallel
            };

            // Create our learning agent
            var agent = new LocalDiscreteRolloutAgent<float[]>(_learningSetup, env);

            // Let it learn!
            for (int i = 0; i < 1000; i++)
            {
                await agent.Step();

                if ((i + 1) % 50 == 0)
                {
                    Console.WriteLine($"Step {i + 1}/1000");

                    Console.WriteLine("\nPress Enter to continue...");
                    //Console.ReadLine();
                }

                Console.WriteLine($"Current reward: {environment.TotalReward}");
                await Task.Delay(800); // Small delay to simulate time between steps
            }

            Console.WriteLine("\nTraining complete!");
            Console.ReadLine();
        }
    }
}

