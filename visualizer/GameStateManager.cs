using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance;
    public GameState currentState;
    public GameObject preGameCanvas;
    public GameObject GameTitle;
    public GameObject ChoosePlayer1;
    public GameObject ChoosePlayer2;

    public CanvasGroup preGameCanvasGroup;

    public GameObject gamePlayCanvasLeft;
    public GameObject gamePlayCanvasRight;
    

    //public GameObject arCamera;
    public GameObject arCamera_left;
    public GameObject arCamera_right;

    public MQTTUnityClient mqttHandler;

    public enum GameState
    {
        PreGame,
        InGame,
        PostGame
    }


    private void Awake()
    {
        if (instance == null)
        {
            instance = this; 
            DontDestroyOnLoad(gameObject);

        } else 
        { 
            Destroy(gameObject);
        }
        currentState = GameState.PreGame;
    }

    public void ChangeState(GameState newState)
    {
        switch (currentState)
        {
            case GameState.PreGame:
                //arCamera.SetActive(false);
                arCamera_left.SetActive(false);
                arCamera_right.SetActive(false);
                preGameCanvas.SetActive(true);
                gamePlayCanvasLeft.SetActive(false);
                gamePlayCanvasRight.SetActive(false);
                break;
            case GameState.InGame:
                //arCamera.SetActive(true);
                arCamera_left.SetActive(true);
                arCamera_right.SetActive(true);
                preGameCanvas.SetActive(false);
                gamePlayCanvasLeft.SetActive(true);
                gamePlayCanvasRight.SetActive(true);
                break;
            //case GameState.PostGame: // to be implemented
            //    preGameCanvas.SetActive(false);
            //    gamePlayCanvasLeft.SetActive(false);
            //    break;
        }
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        switch(currentState)
        {
            case GameState.PreGame:
                //arCamera.SetActive(false);
                arCamera_left.SetActive(false);
                arCamera_right.SetActive(false);
                preGameCanvas.SetActive(true);
                preGameCanvasGroup.alpha = 1;

                gamePlayCanvasLeft.SetActive(false);
                gamePlayCanvasRight.SetActive(false);
                break;
            case GameState.InGame:
                //arCamera.SetActive(true);
                arCamera_left.SetActive(true);
                arCamera_right.SetActive(true);
                preGameCanvas.SetActive(false);
                preGameCanvasGroup.alpha = 0;
                gamePlayCanvasLeft.SetActive(true);
                gamePlayCanvasRight.SetActive(true);
                break;
            //case GameState.PostGame: // to be implemented
            //    break;
        }
    }

    public void StartGame(int playerId)
    {
        currentState = GameState.InGame;
        preGameCanvas.SetActive(false);
        preGameCanvasGroup.alpha = 0;
        GameTitle.SetActive(false);
        ChoosePlayer1.SetActive(false);
        ChoosePlayer2.SetActive(false);
        mqttHandler.SetPlayerID(playerId);  // Assumes you have a Singleton or a public method to set player ID
    }
}
