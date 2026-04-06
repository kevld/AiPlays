import ExplorationEnv
import torch
from datetime import datetime

def main():
    env = ExplorationEnv()

    device = "cpu"

    if torch.cuda.is_available():
        device = "cuda"

    print(f"Using {device}")

    model = PPO(
        "CnnPolicy",
        env,
        verbose=1,
        learning_rate = .0003,
        n_steps=2048,
        batch_size=64,
        device=device
    )

    print("Starting training... Ctrl+C to stop")
    try:
        model.learn(total_timesteps=50000)
        file_name = "ppo_exploration_" + datetime.now().strftime("%Y%m%d%_H%M%S")
        model.save(file_name)

    except KeyboardInterrupt:
        print("Training stopped by user.")
        file_name = "ppo_exploration_" + datetime.now().strftime("%Y%m%d%_H%M%S") + "_i"
        model.save(file_name)
    
    finally:
        print("Training finished. Model saved.")
        env.close()

if __name__ == "__main__":
    main()