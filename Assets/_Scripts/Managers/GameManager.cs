using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] public int PlayerScore { get; set; }
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject gameOver;
    [SerializeField] private TextMeshProUGUI endScoreText;

    [SerializeField] private TextMeshProUGUI coinsCollected;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null && player.gameObject.activeSelf) 
        {
            PlayerScore += 1;
            scoreText.text = "Score : " + PlayerScore.ToString();
            coinsCollected.text = "Coins : " + player.GetComponent<PlayerController>().Coins.ToString();
        }
    }

    public void SetGameOverActive() 
    {
        endScoreText.text = scoreText.text;
        gameOver.SetActive(true);
    }
}
