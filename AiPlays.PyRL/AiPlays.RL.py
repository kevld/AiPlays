import ExplorationEnv
import torch
from datetime import datetime
from stable_baselines3 import PPO
import os
import glob

def main():
    env = ExplorationEnv.ExplorationEnv()

    device = "cpu"

    if torch.cuda.is_available():
        device = "cuda"

    print(f"Using {device}")

    if not os.path.exists("ppo"):
        os.makedirs("ppo")

    ppo_list = glob.glob("ppo/ppo_exploration_*.zip")

    if ppo_list:
        latest_ppo = max(ppo_list, key=os.path.getctime)
        print(f"Loading model from {latest_ppo}")
        model = PPO.load(latest_ppo, env=env, device=device)
        print(f"Model {latest_ppo} loaded successfully.")
    else:
        model = PPO(
            "CnnPolicy",
            env,
            verbose=2,
            learning_rate = .0003,
            n_steps=2048,
            batch_size=64,
            device=device
        )
        print("New model initialised.")

    print("Starting training... Ctrl+C to stop")
    try:
        model.learn(total_timesteps=2048*10)
        file_name = "ppo/ppo_exploration_" + datetime.now().strftime("%Y%m%d_%H%M%S")
        model.save(file_name)

    except KeyboardInterrupt:
        print("Training stopped by user.")
        file_name = "ppoi/ppo_exploration_" + datetime.now().strftime("%Y%m%d_%H%M%S") + "_i"
        model.save(file_name)
    
    finally:
        print("Training finished. Model saved.")
        env.close()

if __name__ == "__main__":
    main()