using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BakeParticleSystemToMesh))]
public class BakeParticleSystemToMeshEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Random Seed")) ((BakeParticleSystemToMesh) target).NewSeed();
        if (GUILayout.Button("Bake LOD 0")) ((BakeParticleSystemToMesh) target).BakeLod0();
        if (GUILayout.Button("Bake LOD 1")) ((BakeParticleSystemToMesh) target).BakeLod1();
        if (GUILayout.Button("Bake LOD 2")) ((BakeParticleSystemToMesh) target).BakeLod2();
        if (GUILayout.Button("Bake LOD 3")) ((BakeParticleSystemToMesh) target).BakeLod3();
    }

}
