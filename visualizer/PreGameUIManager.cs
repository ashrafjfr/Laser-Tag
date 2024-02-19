using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PreGameUIManager : MonoBehaviour
{

    public GameObject preGameCanvas;
    public GameObject gameplayCanvasLeft;
    public GameObject gameplayCanvasRight;

    public GameObject arCamera;


    void Start()
    {
        arCamera.SetActive(false);
    }

    
    void Update()
    {
        
    }
}
