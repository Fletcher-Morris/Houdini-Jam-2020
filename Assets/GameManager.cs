using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
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
        WaypointManager.Instance.UpdateWaypoints();
        GrassScatter.Instance.Scatter();


        remainingTime = gameLength;
    }


    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Home))
        {
            SceneManager.LoadScene(0);
        }

        if(runClock)
        {
            remainingTime -= Time.deltaTime;
            remainingTime = remainingTime.Clamp(0, gameLength);
            remainingTimeText.text = remainingTime.CeilToInt().ToString();
            if (remainingTime <= 0.0f) TimeUp();
        }
    }


    public static void CollectSheep(Sheep sheep)
    {
        Instance.SheepList.Remove(sheep);
        Instance.remainingSheep = Instance.SheepList.Count;
    }


    void TimeUp()
    {
        runClock = false;
    }


    public List<Sheep> SheepList = new List<Sheep>();
    public static void AddSheep(Sheep sheep)
    {
        if(Instance == null) return;
        if(Instance.SheepList == null) Instance.SheepList = new List<Sheep>();
        Instance.SheepList.Add(sheep);
        Instance.remainingSheep = Instance.SheepList.Count;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(105.0f,5.0f,200.0f,50.0f));
        //GUILayout.Space(50.0f);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Quality");
        int level = 0;
        QualitySettings.names.ToList().ForEach(l=>
        {
            if(GUILayout.Button(l))
            {
                QualitySettings.SetQualityLevel(level, true);
            }
            level++;
        });
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}
