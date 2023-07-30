using Quality;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int remainingSheep;

    public List<Sheep> SheepList = new List<Sheep>();

    [SerializeField, Required] private Camera _cullingCam;

    [SerializeField, Required] private Tick.UpdateManager _updateManager;
    public Tick.UpdateManager UpdateManager { get => _updateManager; }

    [SerializeField, Required] private Pathing.WaypointManager _waypointManager;
    public Pathing.WaypointManager WaypointManager { get => _waypointManager; }

    [SerializeField, Required] private Scatter.ObjectScatterer _grassScatterer;
    public Scatter.ObjectScatterer GrassScatterer { get => _grassScatterer; }

    [SerializeField, Required] private DayNightCycle _dayNightCycle;
    public DayNightCycle DayNightCycle { get => _dayNightCycle; }
    public Camera CullingCam { get => _cullingCam; }

    [SerializeField, Required] private Quality.QualitySettingsManager _qualityManager;
    public QualitySettingsManager QualityManager { get => _qualityManager; }

    private void OnApplicationQuit()
    {
        _updateManager.OnApplicationQuit();
    }

    private void Awake()
    {
        Instance = this;

        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            Application.targetFrameRate = 120;
        }

        _grassScatterer.Scatter();
    }

    private void Start()
    {
        _updateManager.Start();
    }

    private void Update()
    {
        _updateManager.OnUpdate(Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Home))
        {
            SceneManager.LoadScene(0);
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    private void FixedUpdate()
    {
        _updateManager.OnFixedUpdate(Time.fixedDeltaTime);
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

    public static void AddSheep(Sheep sheep)
    {
        if (Instance == null) return;
        if (Instance.SheepList == null) Instance.SheepList = new List<Sheep>();
        Instance.SheepList.Add(sheep);
        Instance.remainingSheep = Instance.SheepList.Count;
    }
}