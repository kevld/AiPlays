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

    def __init__(self):
        super().__init__()

        # Up , Down, Left, Right, A, B, Start, Select
        self.action_space = gym.spaces.Discrete(8)
        
        # Retrieve from ML.Net
        self.observation_space = spaces.Box(low=0, high=255, shape=(84,84,1), dtype=np.uint8)

        # gRPC connect 
        self.channel = grpc.insecure_channel('localhost:5000')
        self.endpoint = trainer_api_pb2_grpc.TrainerApiStub(self.channel)

    def step(self, action):
        response = self.send_action_and_get_response(action+1)

        img = self.process_image(response.image_data)      

        reward = 0

        if response.game_state != 3:
            reward -= .1

        if self.last_frame is not None:
            # Calculate the difference between the current frame and the last frame
            diff_before_current = cv2.absdiff(self.last_frame, img)
            # Average pixel difference as reward
            change_score = np.sum(diff_before_current) / (7056) # 84 * 84 

            if change_score > 2:
                reward = 1
            else:
                reward = -.1
        
        self.last_frame = img

        return img, reward, response.game_state != 3, False, {}    

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
            super().reset(seed=seed)
            # Set initial state
            response = self.send_action_and_get_response(0) 
            self.last_frame = self.process_image(response.image_data)
            return self.last_frame, {}

    def close(self):
        if self.channel:
            self.channel.close()