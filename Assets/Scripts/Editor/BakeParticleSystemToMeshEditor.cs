using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BakeParticleSystemToMesh))]
public class BakeParticleSystemToMeshEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Bake"))
        {
            ((BakeParticleSystemToMesh)target).SaveAsset();
        }
    }

}