using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[ExecuteInEditMode]
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

    [SerializeField] private WaypointManager _waypointManager;
    public WaypointManager WaypointManager { get => _waypointManager; }

	[SerializeField] private GrassScatter _grassScatterer;
    public GrassScatter GrassScatterer { get => _grassScatterer; }



    private void Awake()
	{
		Instance = this;
	}

	private void Start()
	{
		_waypointManager.Start();

		remainingTime = gameLength;
	}
	
	public void FixClusters()
    {
		StartCoroutine(_waypointManager.FixClusters());
    }

	private void Update()
	{
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
		_waypointManager.FixedUpdate();
    }

    private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(105.0f, 5.0f, 300.0f, 50.0f));
		//GUILayout.Space(50.0f);
		GUILayout.BeginHorizontal();
		GUILayout.Label("Quality");
		var level = 0;
		QualitySettings.names.ToList().ForEach(l =>
		{
			if (GUILayout.Button(l)) QualitySettings.SetQualityLevel(level, true);
			level++;
		});
		GUILayout.EndHorizontal();
		GUILayout.EndArea();

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