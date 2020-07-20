using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameRefresher : MonoBehaviour
{
    public GameObject menuPanel;
    public GameObject[] gameElements;
    public GameObject gamePanel;
    public GameObject gameOverPanel;
    void Start()
    {
        menuPanel.SetActive(true);
    
        for(int i=0; i<gameElements.Length; i++)
        {
            gameElements[i].SetActive(true);
            Debug.Log(gameElements[i].name);

        }
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }


}
