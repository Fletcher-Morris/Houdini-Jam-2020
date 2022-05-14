using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptables/Sheep/Eye Preset", fileName = "New Eye Preset")]
public class EyePreset : ScriptableObject
{
    [SerializeField] private EyePosition.EyeProperties properties;
    public EyePosition.EyeProperties Properties { get => properties; set => properties = value; }
}