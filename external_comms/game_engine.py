from game_state import *
from threading import Thread, Event, Lock
import time


# TAKES IN SHOT, SHOT RECV, ACTION
# SENDS GAME STATE TO EVAL SERVER
# TAKES IN canSee FROM VISUALIZER
# RETURNS gameState: need to give it to a lot of places (eval server, visualizer, relay server...)

# SPECIFICS:
# get individual p1 p2 game states
# can maintain my own internal FULL game state, inclusive of shield up or down


BULLET_DAMAGE = 10
GRENADE_DAMAGE = 30
SPECIAL_DAMAGE = 10

class GameEngine:
    def __init__(self, game_to_visualizer_queue, game_to_eval_queue, game_to_relay_queue, mqtt_manager):
        self.game_state = GameState()   

        self.game_to_visualizer_queue = game_to_visualizer_queue
        self.game_to_eval_queue = game_to_eval_queue
        self.game_to_relay_queue = game_to_relay_queue

        self.mqtt_manager = mqtt_manager

        # This queue is for holding shoots and shot_recvs that have not been handled yet 

        self.p1_pending = [] # holds p1 shots and p2 recvs
        self.p2_pending = [] # holds p2 shots and p1 recvs

        # This segment below is for timeout stuff
        self.lock = Lock() # lock to prevent race conditions
        self._stop_timeout_thread = Event()
        self._timeout_thread = Thread(target=self._run_timeout_check)
        self._timeout_thread.start()
    
    def _run_timeout_check(self):
        while not self._stop_timeout_thread.is_set():
            with self.lock:
                self.timeout_check()
            time.sleep(0.1)  # Check every second

    def stop(self):  # Call this when you're shutting down the game
        self._stop_timeout_thread.set()
        self._timeout_thread.join()
        

    # Updates Game State values from Eval Server
    def update_game_state(self, new_game_state):
        with self.lock:
            self.game_state.p1.hp = new_game_state["p1"]["hp"]
            self.game_state.p1.shield_hp = new_game_state["p1"]["shield_hp"]
            self.game_state.p1.bullets = new_game_state["p1"]["bullets"]
            self.game_state.p1.grenades = new_game_state["p1"]["grenades"]
            self.game_state.p1.shields = new_game_state["p1"]["shields"]
            self.game_state.p1.deaths = new_game_state["p1"]["deaths"]

            self.game_state.p2.hp = new_game_state["p2"]["hp"]
            self.game_state.p2.shield_hp = new_game_state["p2"]["shield_hp"]
            self.game_state.p2.bullets = new_game_state["p2"]["bullets"]
            self.game_state.p2.grenades = new_game_state["p2"]["grenades"]
            self.game_state.p2.shields = new_game_state["p2"]["shields"]
            self.game_state.p2.deaths = new_game_state["p2"]["deaths"]




    # Updates game_state, sends to all queues
    # action: string
    # player: int
    # can_see: boolean
    def handle_action(self, action, player, can_see):
        with self.lock:
            enemy = 2 if player == 1 else 1

            current_time = time.time()
            if action in ['shoot', 'shot_recv']:
                if (player == 1 and action == "shoot") or (player == 2 and action == "shot_recv"):
                    queue = self.p1_pending
                elif (player == 2 and action == "shoot") or (player == 1 and action == "shot_recv"):
                    queue = self.p2_pending

                # If queue is empty, simply append the new action
                if not queue:
                    queue.append([player, action, current_time])
                    return
                else:
                    prev_action_data = queue[0]

                    # If the current and previous actions match, process them together`
                    if (prev_action_data[1] == "shoot" and action == "shot_recv") or \
                    (prev_action_data[1] == "shot_recv" and action == "shoot"):
                        if action == "shoot": # If the current action is shoot, process the previous action first
                            self.shoot(player)
                            self.shot_recv(prev_action_data[0])        
                            self.send_to_eval_queue(action, player) # MUST send the shooter                   
                        else: # action == shot_recv
                            self.shoot(prev_action_data[0])
                            self.shot_recv(player)
                            self.send_to_eval_queue(action, prev_action_data[0]) # MUST send the shooter

                        queue.pop(0)  # Remove the processed action
                        
                        self.send_to_visualizer_queue()
                        
                        self.send_to_relay_queue()
                        return
                    else:
                        # If they don't match, process the older one by itself and replace it with the new action
                        if prev_action_data[1] == "shoot":
                            self.shoot(prev_action_data[0])
                            backup_can_see = self.mqtt_manager.p1_can_see if player == 1 else self.mqtt_manager.p2_can_see
                            timer = time.time()
                            while backup_can_see is None:
                                # Timeout in case visualizer is not responding
                                if time.time() - timer > 1:
                                    print(f"Player {player} : Visibility failed")
                                    # logging.info("[GameEngine] Visualizer not responding. Using default value for can_see.")
                                    backup_can_see = True
                            # logging.info(f"[GameEngine] Received visibility response")
                            if backup_can_see:
                                self.shot_recv(enemy)
                        # else:
                            # self.shot_recv(prev_action_data[0])
                        queue[0] = [player, action, current_time]

                        self.send_to_visualizer_queue()
                        self.send_to_eval_queue(action, player)
                        self.send_to_relay_queue()
                        return
            
            elif action == 'reload':
                is_reloaded = self.reload(player)
                self.send_to_visualizer_queue()
                self.send_to_eval_queue(action, player) # action here is a string
                self.send_to_relay_queue()
                # play reload animation      
                if is_reloaded:
                    self.game_to_visualizer_queue.put({
                        "type": "action",
                        "data": {
                            "player_id": player,
                            "action": "reload"
                        }
                    })                
                return

            elif action == 'shield':
                self.shield(player)
            elif action == 'grenade':
                self.grenade(player, enemy, can_see)
            elif action in ['hammer', 'spear', 'web', 'portal', 'punch']:
                self.special(enemy, can_see)
            elif action == 'logout':
                pass
            else:
                print("Invalid action")

            self.send_to_visualizer_queue()
            self.send_to_eval_queue(action, player) # action here is a string
            self.send_to_relay_queue()
            return
    
    # SEND TO QUEUES
    def send_to_eval_queue(self, action, player):
        self.game_to_eval_queue.put({
            "player_id": player,
            "action": action if action not in ["shoot", "shot_recv"] else "gun",
            "game_state": self.game_state.get_dict()
        })
    
    def send_to_visualizer_queue(self):
        self.game_to_visualizer_queue.put({
            "type": "game_state",
            "data": {
                "game_state": self.game_state.get_dict()
            }
        })
    def send_to_relay_queue(self):
        self.game_to_relay_queue.put(self.game_state.get_dict())

    # GAME LOGIC FUNCTIONS. ONLY UPDATE GAME STATE

    # Process damage from successful action (shoot, grenade, special)
    def do_damage(self, damage, receipient_num):
        # shield is up, reduce shield hp
        receipient = self.get_player_by_number(receipient_num)
        if receipient.shield_hp > 0: # shield is up       
            new_shield_hp = max(0, receipient.shield_hp - damage)
            receipient.shield_hp = new_shield_hp
            damage = max(0, damage - receipient.shield_hp)
        
        # reduce hp
        receipient.hp = max(0, receipient.hp - damage)
        if receipient.hp == 0:
            self.respawn(receipient_num)
            receipient.deaths += 1

    def respawn(self, player_num):
        player = self.get_player_by_number(player_num)
        player.hp = 100
        player.shield_hp = 0
        player.bullets = 6
        player.grenades = 2
        player.shields = 3
    
    def shoot(self, shooter_num):
        shooter = self.get_player_by_number(shooter_num)
        if shooter.bullets == 0:
            return      
        shooter.bullets -= 1

    def shot_recv(self, receipient_num):
        self.do_damage(BULLET_DAMAGE, receipient_num)

    def reload(self, player_num):
        player = self.get_player_by_number(player_num)
        if player.bullets > 0:
            return False
        player.bullets = 6
        return True

    def shield(self, player_num):
        player = self.get_player_by_number(player_num)
        if player.shields == 0 or player.shield_hp != 0:
            return
        player.shields -= 1
        player.shield_hp = 30
        
    def grenade(self, player_num, opponent_num, can_see):
        player = self.get_player_by_number(player_num)
        opponent = self.get_player_by_number(opponent_num)
        if player.grenades == 0:
            return
        player.grenades -= 1
        if (can_see):
            self.do_damage(GRENADE_DAMAGE, opponent_num)
    
    def special(self, opponent_num, can_see):
        if (can_see):
            self.do_damage(SPECIAL_DAMAGE, opponent_num)
    
    def get_player_by_number(self, num):
        if num == 1:
            return self.game_state.p1
        elif num == 2:
            return self.game_state.p2
        else:
            raise ValueError("Invalid player number")

    # Checks if 1s has passed since the last shot/shot_recv action
    def timeout_check(self):
        current_time = time.time()

        # Checking timeout for p1_pending
        while self.p1_pending:
            shot = self.p1_pending[0]
            if current_time - shot[2] > 0.1:  # 0.1 second timeout
                player_num = shot[0]

                if shot[1] == 'shoot':
                    self.shoot(player_num)
                    self.send_to_visualizer_queue()
                    self.send_to_eval_queue(shot[1], player_num)
                    self.send_to_relay_queue()

                elif shot[1] == 'shot_recv':
                    pass
                    #self.shot_recv(player_num)

                self.p1_pending.pop(0)  # Remove the processed action
            else:
                break  # If the first action is not timed out, the others won't be either

        # Checking timeout for p2_pending
        while self.p2_pending:
            shot = self.p2_pending[0]
            if current_time - shot[2] > 0.1:  # 0.1 second timeout
                player_num = shot[0]

                if shot[1] == 'shoot':
                    self.shoot(player_num)
                    self.send_to_visualizer_queue()
                    self.send_to_eval_queue(shot[1], player_num)
                    self.send_to_relay_queue()
                elif shot[1] == 'shot_recv':
                    pass
                    #self.shot_recv(player_num)

                self.p2_pending.pop(0)  # Remove the processed action
            else:
                break  # If the first action is not timed out, the others won't be either
