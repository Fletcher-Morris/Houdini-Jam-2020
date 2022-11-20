using System.Collections;
using System.Collections.Generic;
using Tick;
using UnityEngine;
using UnityEngine.UI;

public class SheepUi : MonoBehaviour, IManualUpdate
{
    private Sheep _sheep;

    [SerializeField] private Text _nameText;
    [SerializeField] private Text _stateText;
    [SerializeField] private Text _hungerText;

    [SerializeField] private UpdateManager _updateManager;

    private void Awake()
    {
        _updateManager.AddToUpdateList(this);        
    }

    public void SetSheep(Sheep sheep)
    {
        _sheep = sheep;

        if(_sheep == null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(true);

            _nameText.text = _sheep.gameObject.name;
        }
    }

    UpdateManager IManualUpdate.GetUpdateManager()
    {        
        return _updateManager;
    }

    bool IManualUpdate.IsEnabled()
    {
        return _sheep != null;
    }

    void IManualUpdate.OnApplicationQuit()
    {
        SetSheep(null);
    }

    bool IManualUpdate.OnInitialise()
    {
        return true;
    }

    void IManualUpdate.OnManualFixedUpdate(float delta)
    {
    }

    void IManualUpdate.OnManualUpdate(float delta)
    {
    }

    void IManualUpdate.OnTick(float delta)
    {
        _stateText.text = "State : " + _sheep.CurrentHungerState;
        _hungerText.text = "Hunger : " + _sheep.CurrentHungerValue.Round(2);
    }
}
