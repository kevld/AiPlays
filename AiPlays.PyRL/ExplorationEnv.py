from operator import truediv
import numpy as np
import gymnasium as gym
from gymnasium import spaces
import grpc
import cv2
from typing import Optional

import trainer_api_pb2
import trainer_api_pb2_grpc


class ExplorationEnv(gym.Env):
    channel = None
    endpoint = None
    action_space = None
    observation_space = None
    last_frame = None
    total_reward = 0
    available_actions = [1, 2, 5, 7, 8, 9, 10]
    total_a = 0
    total_b = 0
    total_up = 0
    total_right = 0
    total_down = 0
    total_left = 0
    total_start = 0

    # currentaction = 8
    def __init__(self):
        super().__init__()

        self.action_space = gym.spaces.Discrete(len(self.available_actions))

        # Retrieve from ML.Net
        self.observation_space = spaces.Box(
            low=0, high=255, shape=(84, 84, 1), dtype=np.uint8
        )

        # gRPC connect
        self.channel = grpc.insecure_channel("localhost:5000")
        self.endpoint = trainer_api_pb2_grpc.TrainerApiStub(self.channel)

    def step(self, action):
        index_action = self.available_actions[action]

        if self.available_actions[action] == 1:
            self.total_a += 1
        if self.available_actions[action] == 2:
            self.total_b += 1
        if self.available_actions[action] == 5:
            self.total_start += 1
        if self.available_actions[action] == 7:
            self.total_up += 1
        if self.available_actions[action] == 8:
            self.total_right += 1
        if self.available_actions[action] == 9:
            self.total_down += 1
        if self.available_actions[action] == 10:
            self.total_left += 1

        terminated = False

        response = self.send_action_and_get_response(index_action)

        img = self.process_image(response.image_data)

        reward = 0

        if response.game_state != 3:
            reward -= 5
            terminated = True
        else:
            if self.last_frame is not None:
                # Calculate the difference between the current frame and the last frame
                diff_before_current = cv2.absdiff(self.last_frame, img)
                # Average pixel difference as reward
                change_score = np.sum(diff_before_current) / (7056)  # 84 * 84
                print(f"Change score: {change_score}")
                if change_score > 1.5:
                    reward = 1
                elif change_score > 0.227:
                    reward = -0.01
                else:
                    reward = -0.1

        reward = round(reward, 3)
        self.last_frame = img
        self.total_reward += reward
        self.total_reward = round(self.total_reward, 3)

        print(
            f"GameState: {response.game_state}, Reward: {reward}, Total Reward: {self.total_reward}"
        )
        print(
            f"Run : A : {self.total_a}, B : {self.total_b}, Start : {self.total_start}, Up : {self.total_up}, Right : {self.total_right}, Down : {self.total_down}, Left : {self.total_left}"
        )

        return img, reward, terminated, False, {}

    def process_image(self, image_data):
        # Gray scale image
        nparr = np.frombuffer(image_data, np.uint8)
        img = cv2.imdecode(nparr, cv2.IMREAD_GRAYSCALE)

        # Resize to lightweight
        img = cv2.resize(img, (84, 84))

        return np.expand_dims(img, axis=-1)

    def send_action_and_get_response(self, action):
        # Request
        request = trainer_api_pb2.ActionRequest(gba_key=action)
        response = self.endpoint.Step(request)

        return response
 
    def reset(self, seed: Optional[int] = None, options: Optional[dict] = None):
        print("Reset called.")
        super().reset(seed=seed)
        # Set initial state
        response = self.send_action_and_get_response(0)
        self.last_frame = self.process_image(response.image_data)
        self.total_reward = 0
        self.total_a = 0
        self.total_b = 0
        self.total_start = 0
        self.total_up = 0
        self.total_right = 0
        self.total_down = 0
        self.total_left = 0
        return self.last_frame, {}

    def close(self):
        if self.channel:
            self.channel.close()