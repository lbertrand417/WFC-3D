using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(BoxCollider))]
public class SemiInteractiveGrid : MonoBehaviour
{
    private float tileSize = 1; // Added for extension if we want to work with tiles bigger than unit size (not implemented)

    // Size of the canvas
    public int width = 2;
    public int height = 2;
    public int depth = 2;

    // Array of tiles
    [HideInInspector]
    public Tuile[] tuiles;

    void OnValidate()
    {
        BoxCollider bounds = this.GetComponent<BoxCollider>();
        bounds.center = new Vector3((width * tileSize) * 0.5f - tileSize * 0.5f, (height * tileSize) * 0.5f - tileSize * 0.5f, (depth * tileSize) * 0.5f - tileSize * 0.5f);
        bounds.size = new Vector3(width * tileSize, (height * tileSize), depth * tileSize);
    }

    public void Run()
    {
        tuiles = new Tuile[width * height * depth];
        int childs = this.gameObject.transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
        {
            Tuile t = this.gameObject.transform.GetChild(i).gameObject.GetComponent<Tuile>();

            if (t != null)
            {
                // Find indices inside the canvas
                // First + second terms: Find the position of the center of the tile
                //          - First term: position of the pivot of the tile
                //          - Second term: translate to have the pivot in the center of the tile
                // Third + Fourth terms: Find the position of the center of the first tile
                //          - Third term: Position of the canvas
                //          - Fourth term: Offset to consider the center of tile placement
                int indicex = (int)((t.transform.position.x + t.offset[0] - transform.position.x + tileSize / 2) / tileSize);
                int indicey = (int)((t.transform.position.y + t.offset[1] - transform.position.y + tileSize / 2) / tileSize);
                int indicez = (int)((t.transform.position.z + t.offset[2] - transform.position.z + tileSize / 2) / tileSize);

                Vector3 pos = t.transform.position;

                // Find the position of the tiles
                // First term: Position of the canvas
                // Second term: Move the pivot point to the good incides in the canvas
                // Third term: Change the offset to place the center of the tile at the center
                pos.x = transform.position.x + indicex * tileSize - t.offset[0];
                pos.y = transform.position.y + indicey * tileSize - t.offset[1];
                pos.z = transform.position.z + indicez * tileSize - t.offset[2];

                t.transform.position = pos;

                if (0 <= indicex && 0 <= indicey && 0 <= indicez && indicex < width && indicey < height && indicez < depth)
                {
                    int indice = indicex + width * indicey + width * height * indicez;
                    tuiles[indice] = t;
                    Debug.Log("(" + indicex + " " + indicey + " " + indicez + ") " + t.gameObject.name);
                    // Debug.Log("Tuile trouv�e � " + indicex + " et " + indicey + " et " + indicez);
                }
            }
        }
    }

    public void Clear()
    {
        tuiles = new Tuile[width * height * depth];

        // Reset and delete all objects
        int childs = this.gameObject.transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
        {
            DestroyImmediate(this.gameObject.transform.GetChild(i).gameObject);
        }
    }

    bool Contains(Tuile t)
    {
        foreach (Tuile aTuile in tuiles)
        {
            if (aTuile != null)
            {
                if (aTuile.GetInstanceID() == t.GetInstanceID())
                {
                    return true;
                }
            }
        }
        return false;
    }

    public void Clean()
    {
        int childs = this.gameObject.transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
        {
            if (!Contains(this.gameObject.transform.GetChild(i).gameObject.GetComponent<Tuile>()))
            {
                DestroyImmediate(this.gameObject.transform.GetChild(i).gameObject);
            }
        }
    }

    public void OnDrawGizmos()
    {
        // Canvas border is white
        Gizmos.color = Color.white;
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.DrawWireCube(new Vector3((width * tileSize) * 0.5f - tileSize * 0.5f, (height * tileSize) * 0.5f - tileSize * 0.5f, (depth * tileSize) * 0.5f - tileSize * 0.5f),
            new Vector3(width * tileSize, (height * tileSize), depth * tileSize));
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SemiInteractiveGrid))]
public class SemiInteractiveGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SemiInteractiveGrid me = (SemiInteractiveGrid)target;
        GUILayout.Label("The tiles must be childs of the canvas object.");
        GUILayout.Label("Run    : Put the tile in the canvas");
        GUILayout.Label("Warning ! You must run again if you modify either the width, \n height or depth.");
        GUILayout.Label("Clean    : Delete the tiles that are not in the canvas. ");
        GUILayout.Label("Warning ! You must run before cleaning");
        GUILayout.Label("Clear    : Delete childs of the canvas.");
        if (GUILayout.Button("RUN"))
        {
            me.Run();
        }
        if (GUILayout.Button("Clean"))
        {
            me.Clean();
        }
        if (GUILayout.Button("Clear"))
        {
            me.Clear();
        }
        DrawDefaultInspector();
    }

}
#endif
