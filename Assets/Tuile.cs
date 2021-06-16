using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Tuile : MonoBehaviour
{
    public string tilename;
    public Vector3 offset;

    public Tuile(string _name)
    {
        tilename = _name;
    }

}

public class TuileEqualityComparer : IEqualityComparer<Tuile>
{
    public bool Equals(Tuile t1, Tuile t2)
    {
        if (t1 == null && t2 == null)
            return true;
        else if (t1 == null || t2 == null)
            return false;
        else
            return t1.gameObject.name.Equals(t2.gameObject.name);
    }

    public int GetHashCode(Tuile tuile)
    {
        return tuile.gameObject.name.GetHashCode();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Tuile))]
public class TuileEditor : Editor
{
    
    public override void OnInspectorGUI()
    {
        GUILayout.Label("The pivot point should be at the center of the tile.");
        GUILayout.Label("Tilename   : Name of the tile.");
        GUILayout.Label("Offset   : Translation of the pivot point to put it in the center.");
        DrawDefaultInspector();
    }

}
#endif