using AiPlays.Brain.Services;
using AiPlays.RL.Environments;
using RLMatrix;
using RLMatrix.Agents.Common;

namespace AiPlays.RL
{
    /// <summary>
    /// Reinforcement learning agent wrapper that configures and runs
    /// training sessions using the RLMatrix library and project environments.
    /// </summary>
    public class RLAgent
    {
        /// <summary>
        /// Options used to configure the DQN learning algorithm (batch size, memory,
        /// discount factor, and epsilon scheduling).
        /// </summary>
        private readonly DQNAgentOptions _learningSetup;

        /// <summary>
        /// Service that provides training-related helpers and access to environment data.
        /// </summary>
        private readonly TrainerService _trainerService;

        /// <summary>
        /// Initializes a new instance of <see cref="RLAgent"/> with the provided
        /// <see cref="TrainerService"/>.
        /// </summary>
        /// <param name="trainerService">The trainer service used to create and manage environments.</param>
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

        /// <summary>
        /// Starts the training loop asynchronously. This will create the configured
        /// environment(s), initialize the learning agent and run a series of training
        /// steps until completion.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
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

