using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int playerScore = 0;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player.gameObject.activeSelf) 
        {
            playerScore += 1;
        }

        scoreText.text = "Score : " + playerScore.ToString();
    }
}
