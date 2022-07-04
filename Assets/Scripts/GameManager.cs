using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int gameLength = 180;
    public float remainingTime = 180.0f;
    public bool runClock;
    public Text remainingTimeText;

    public int remainingSheep;

    public List<Sheep> SheepList = new List<Sheep>();

    [SerializeField] private Camera _cullingCam;

    [SerializeField, Required] private Tick.UpdateManager _updateManager;
    public Tick.UpdateManager UpdateManager { get => _updateManager; }

    [SerializeField, Required] private Pathing.WaypointManager _waypointManager;
    public Pathing.WaypointManager WaypointManager { get => _waypointManager; }

    [SerializeField] private Scatter.ObjectScatterer _grassScatterer;
    public Scatter.ObjectScatterer GrassScatterer { get => _grassScatterer; }

    [SerializeField] private DayNightCycle _dayNightCycle;
    public DayNightCycle DayNightCycle { get => _dayNightCycle; }

    private void OnApplicationQuit()
    {
        _updateManager.OnApplicationQuit();
    }

    private void Awake()
    {
        Instance = this;

        Application.targetFrameRate = 60;

        _grassScatterer.Scatter();
    }

    private void Start()
    {
        _waypointManager.Start();

        remainingTime = gameLength;

        _updateManager.AddToUpdateList(_dayNightCycle);
    }

    private void Update()
    {
        float delta = Time.deltaTime;
        _updateManager.OnUpdate(delta);

        if (Input.GetKeyDown(KeyCode.Home))
            SceneManager.LoadScene(0);
        else if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        if (runClock)
        {
            remainingTime -= Time.deltaTime;
            remainingTime = remainingTime.Clamp(0, gameLength);
            remainingTimeText.text = remainingTime.CeilToInt().ToString();
            if (remainingTime <= 0.0f) TimeUp();
        }

        _waypointManager?.DrawLines(_cullingCam);
    }

    private void FixedUpdate()
    {
        float delta = Time.fixedDeltaTime;
        _updateManager.OnFixedUpdate(delta);
    }

    private void OnGUI()
    {
        _waypointManager.DebugGui();
    }

    public static void CollectSheep(Sheep sheep)
    {
        Instance.SheepList.Remove(sheep);
        Instance.remainingSheep = Instance.SheepList.Count;
    }

    private void TimeUp()
    {
        runClock = false;
    }

    public static void AddSheep(Sheep sheep)
    {
        if (Instance == null) return;
        if (Instance.SheepList == null) Instance.SheepList = new List<Sheep>();
        Instance.SheepList.Add(sheep);
        Instance.remainingSheep = Instance.SheepList.Count;
    }
}