using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameManager Instance;
    void Awake()
    {
        Instance = this;
    }

    public int gameLength = 180;
    public float remainingTime = 180.0f;
    public bool runClock = false;
    public Text remainingTimeText;


    public int remainingSheep;


    void Start()
    {
        remainingTime = gameLength;
    }


    void Update()
    {
        if(runClock)
        {
            remainingTime -= Time.deltaTime;
            remainingTime = remainingTime.Clamp(0, gameLength);
            remainingTimeText.text = remainingTime.CeilToInt().ToString();
            if (remainingTime <= 0.0f) TimeUp();
        }
    }


    public void CollectSheep()
    {
        remainingSheep--;
        if (remainingSheep < 0) remainingSheep = 0;
    }


    void TimeUp()
    {
        runClock = false;
    }
}
