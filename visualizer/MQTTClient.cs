using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;
using System;
using System.Collections.Generic;
using static ActionHandler;

public class MQTTUnityClient : MonoBehaviour
{
    public GameplayUIManager gameplayUIManager;
    public ARTrackingHandler arTrackingHandler;
    public ActionHandler actionHandler;

    public int my_player_id;

    private MqttClient client;
    //public string brokerHostname = "test.mosquitto.org";
    public string brokerHostname = "172.26.190.101";
    public string gameStateTopic = "cg4002-b05/game/game_state";

    public string actionTopicBase = "cg4002-b05/";
    public string canSeeTopicBase = "cg4002-b05/";
    public string messageTopicBase = "cg4002-b05/";

    public string actionTopic;
    public string canSeeTopic;
    public string messageTopic;

    private Queue<Action> stuffToExecute = new Queue<Action>();
   

    [System.Serializable]
    public class ActionData
    {
        public int player_id;
        public string action;
    }

    [System.Serializable]
    public class PlayerStats
    {
        public int hp;
        public int bullets;
        public int grenades;
        public int shield_hp;
        public int deaths;
        public int shields;
    }

    [System.Serializable]
    public class GameStateData
    {
        public PlayerStats p1;
        public PlayerStats p2;
    }

    [System.Serializable]
    public class FullGameState
    {
        public GameStateData game_state;
    }

    [System.Serializable]
    public class VisibilityData
    {
        public bool is_visible;
    }

    [System.Serializable]
    public class MessageData
    {
        public string device;
        public bool is_connected;
    }

    // MQTT methods
    private void SubscribeTopics()
    {
        client.Subscribe(new string[] { gameStateTopic, actionTopic, messageTopic },
            new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
    }

    private void Reconnect()
    {
        // Disconnect if still connected (for cleanup)
        if (client.IsConnected)
        {
            client.Disconnect();
        }

        // Wait before trying to reconnect
        System.Threading.Thread.Sleep(1000);

        // Attempt to reconnect
        try
        {
            string clientId = System.Guid.NewGuid().ToString();
            client.Connect(clientId);
            SubscribeTopics();
        }
        catch (Exception e)
        {
            Debug.LogError("MQTT Reconnect failed with exception: " + e.Message);
            // Schedule another reconnect attempt
            Invoke("Reconnect", 5f);
        }

    }

    public void SetPlayerID(int id)
    {
        my_player_id = id;
        actionTopic = actionTopicBase + $"p{id}/action";
        canSeeTopic = canSeeTopicBase + $"p{id}/can_see";
        messageTopic = messageTopicBase + $"p{id}/message";


        // Subscribe to topics
        //client.Subscribe(new string[] { gameStateTopic, actionTopic, messageTopic },
        //    new byte[] { MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE });
        SubscribeTopics();
    }

    private void Start()
    {
        Debug.Log("Starting MQTT stuff now!");
        // Create the client instance
        client = new MqttClient(brokerHostname);

        // Register to message received event
        client.MqttMsgPublishReceived += Client_MqttMsgPublishReceived;

        // Create a unique client ID and connect
        string clientId = System.Guid.NewGuid().ToString();
        client.Connect(clientId);
    
    }

    private void Update()
    {
        // Dequeue and execute all actions
        while (stuffToExecute.Count > 0)
        {
            stuffToExecute.Dequeue().Invoke();
        }

        if (!client.IsConnected)
        {
            Debug.LogWarning("MQTT Client disconnected, attempting to reconnect...");
            Reconnect();
        }
    }

    private void Client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
    {
        string receivedMessage = Encoding.UTF8.GetString(e.Message);

        // Handle game state update
        if (e.Topic == gameStateTopic)
        {
            FullGameState receivedData = JsonUtility.FromJson<FullGameState>(receivedMessage);

            // Handle the received game state and action to display
            //actionHandler.CurrentGameState = receivedData.game_state;

            if (my_player_id == 1)
            {
                // Set Player stats to p1 stats
                actionHandler.CurrentGameState.Player.HP = receivedData.game_state.p1.hp;
                actionHandler.CurrentGameState.Player.Bullets = receivedData.game_state.p1.bullets;
                actionHandler.CurrentGameState.Player.Grenades = receivedData.game_state.p1.grenades;
                actionHandler.CurrentGameState.Player.ShieldPoints = receivedData.game_state.p1.shield_hp;
                actionHandler.CurrentGameState.Player.Deaths = receivedData.game_state.p1.deaths;
                actionHandler.CurrentGameState.Player.Shields = receivedData.game_state.p1.shields;

                // Set Enemy stats to p2 stats
                actionHandler.CurrentGameState.Enemy.HP = receivedData.game_state.p2.hp;
                actionHandler.CurrentGameState.Enemy.Bullets = receivedData.game_state.p2.bullets;
                actionHandler.CurrentGameState.Enemy.Grenades = receivedData.game_state.p2.grenades;
                actionHandler.CurrentGameState.Enemy.ShieldPoints = receivedData.game_state.p2.shield_hp;
                actionHandler.CurrentGameState.Enemy.Deaths = receivedData.game_state.p2.deaths;
                actionHandler.CurrentGameState.Enemy.Shields = receivedData.game_state.p2.shields;
            }
            else if (my_player_id == 2)
            {
                // Set Player stats to p2 stats
                actionHandler.CurrentGameState.Player.HP = receivedData.game_state.p2.hp;
                actionHandler.CurrentGameState.Player.Bullets = receivedData.game_state.p2.bullets;
                actionHandler.CurrentGameState.Player.Grenades = receivedData.game_state.p2.grenades;
                actionHandler.CurrentGameState.Player.ShieldPoints = receivedData.game_state.p2.shield_hp;
                actionHandler.CurrentGameState.Player.Deaths = receivedData.game_state.p2.deaths;
                actionHandler.CurrentGameState.Player.Shields = receivedData.game_state.p2.shields;

                // Set Enemy stats to p1 stats
                actionHandler.CurrentGameState.Enemy.HP = receivedData.game_state.p1.hp;
                actionHandler.CurrentGameState.Enemy.Bullets = receivedData.game_state.p1.bullets;
                actionHandler.CurrentGameState.Enemy.Grenades = receivedData.game_state.p1.grenades;
                actionHandler.CurrentGameState.Enemy.ShieldPoints = receivedData.game_state.p1.shield_hp;
                actionHandler.CurrentGameState.Enemy.Deaths = receivedData.game_state.p1.deaths;
                actionHandler.CurrentGameState.Enemy.Shields = receivedData.game_state.p1.shields;
            }

            // Handle the received game state and action to display
            stuffToExecute.Enqueue(() => actionHandler.UpdateGameState());

        }

        // Handle action event and visibility
        else if (e.Topic == actionTopic)
        {
            ActionData receivedAction = JsonUtility.FromJson<ActionData>(receivedMessage);
            Debug.Log($"Received action: Player ID: {receivedAction.player_id}, Action: {receivedAction.action}");

            // Enqueue AR effects based on action
            EnqueueActionEffect(receivedAction.action);

            // Publish 'canSee' data to game engine
            SendVisibilityReply();
        }
        
        // Handle connection message event
        else if (e.Topic == messageTopic)
        {
            MessageData receivedMessageData = JsonUtility.FromJson<MessageData>(receivedMessage);
            string device = receivedMessageData.device;
            bool is_connected = receivedMessageData.is_connected;
            Debug.Log($"QIAN Received connection status: Device: {receivedMessageData.device}, Is Connected: {receivedMessageData.is_connected}");

            // Update UI to show connection status
            switch (device)
            {
                case "gun":
                    stuffToExecute.Enqueue(() => gameplayUIManager.UpdateGunConnectionStatus(is_connected));
                    break;
                case "vest":
                    stuffToExecute.Enqueue(() => gameplayUIManager.UpdateVestConnectionStatus(is_connected));
                    break;
                case "imu":
                    stuffToExecute.Enqueue(() => gameplayUIManager.UpdateGloveConnectionStatus(is_connected));
                    break;
            }
            
        }
    }


    // Function to enqueue effects of the action
    private void EnqueueActionEffect(string action)
    {
        stuffToExecute.Enqueue(() => gameplayUIManager.ShowDetectedAction(action));

        switch (action)
        {
            case "shoot":
                stuffToExecute.Enqueue(() => actionHandler.PlayerShoot());
                Debug.Log("SHOULD SHOOT");
                break;

            case "shot_recv":
                stuffToExecute.Enqueue(() => actionHandler.EnemyShoot());
                break;

            case "reload":
                stuffToExecute.Enqueue(() => actionHandler.TriggerPlayerReload());
                break;

            case "shield":
                stuffToExecute.Enqueue(() => actionHandler.PlayerShield());
                break;

            case "grenade":
                SendVisibilityReply();
                stuffToExecute.Enqueue(() => actionHandler.PlayerGrenade());
                Debug.Log("SHOULD THROW GRENADE");
                break;

            case "hammer":
                SendVisibilityReply();
                stuffToExecute.Enqueue(() => actionHandler.PlayerHammer());
                break;

            case "spear":
                SendVisibilityReply();
                stuffToExecute.Enqueue(() => actionHandler.PlayerSpear());
                break;

            case "punch":
                SendVisibilityReply();
                stuffToExecute.Enqueue(() => actionHandler.PlayerFist());
                break;

            case "web":
                SendVisibilityReply();
                stuffToExecute.Enqueue(() => actionHandler.PlayerWeb());
                break;

            case "portal":
                SendVisibilityReply();
                stuffToExecute.Enqueue(() => actionHandler.PlayerPortal());
                break;

            case "logout":
                Debug.Log("LOGOUT");
                stuffToExecute.Enqueue(() => actionHandler.PlayerLogout());
                break;

            default:
                Debug.LogWarning($"Unknown action received: {action}");
                break;
        }
    }


    

    private void SendVisibilityReply()
    {
        VisibilityData dataToSend = new VisibilityData { is_visible = arTrackingHandler.canSeeEnemy };
        string jsonMessage = JsonUtility.ToJson(dataToSend);
        client.Publish(canSeeTopic, Encoding.UTF8.GetBytes(jsonMessage));
        Debug.Log($"Sent visibility data: Is Visible: {arTrackingHandler.canSeeEnemy}");
    }


    private void OnDestroy()
    {
        if (client != null)
        {
            client.Disconnect();
        }

        // Cancel any pending reconnection attempts
        CancelInvoke("Reconnect");
    }
}
