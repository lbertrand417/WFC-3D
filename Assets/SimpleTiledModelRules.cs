using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(SemiInteractiveGrid))]
public class SimpleTiledModelRules : MonoBehaviour
{
    private static int NUM_DIRECTIONS = 6;

    private Tuile[] tiles;
    private int width; // x-axis
    private int height; // y-axis
    private int depth; // z-axis
    private Dictionary<Tuile, int> tileIndices;
    private List<Tuile> indexTiles;
    private int numTiles;
    private bool[,,] rules;

    /*
        check(t1, t2, left)
        [t1][t2]

        check(t1, t2, right)
        [t2][t1]

        check(t1, t2, top)
        [t1]
        [t2]

        check(t1, t2, bottom)
        [t2]
        [t1]

        check(t1, t2, front)
        
        check(t1, t2, back)
    */

    public List<Tuile> getTuiles()
    {
        return indexTiles;
    }


    public bool check(Tuile tile1, Tuile tile2, Direction direction)
    {
        return rules[tileIndices[tile1], tileIndices[tile2], (int) direction];
    }
    public List<Tuile> getTuiles()
    {
        return indexTiles;
    }

    public void generateRules()
    {
        rules = new bool[numTiles, numTiles, NUM_DIRECTIONS];
        int leftCheck = width - 1;
        int xyArea = width * height;
        int topCheck = xyArea - width;
        int frontCheck = tiles.Length - xyArea;
        for (int i = 0; i < tiles.Length; ++i)
        {
            // right
            if (i % width != 0)
            {
                rules[tileIndices[tiles[i - 1]], tileIndices[tiles[i]], (int) Direction.Right] = true;
            }

            // left
            if (i % width != leftCheck)
            {
                rules[tileIndices[tiles[i + 1]], tileIndices[tiles[i]], (int) Direction.Left] = true;
            }

            // bottom
            if (i % xyArea >= width)
            {
                rules[tileIndices[tiles[i - width]], tileIndices[tiles[i]], (int) Direction.Bottom] = true;
            }

            // top
            if (i % xyArea < topCheck)
            {
                rules[tileIndices[tiles[i + width]], tileIndices[tiles[i]], (int) Direction.Top] = true;
            }

            // back
            if (i >= xyArea)
            {
                rules[tileIndices[tiles[i - xyArea]], tileIndices[tiles[i]], (int) Direction.Back] = true;
            }

            // front
            if (i < frontCheck)
            {
                rules[tileIndices[tiles[i + xyArea]], tileIndices[tiles[i]], (int) Direction.Front] = true;
            }
        }
    }

    public void generateIndices()
    {
        TuileEqualityComparer equalityComparer = new TuileEqualityComparer();
        tileIndices = new Dictionary<Tuile, int>(equalityComparer);
        numTiles = 0;
        indexTiles = new List<Tuile>();
        foreach (Tuile tile in tiles)
        {
            if (!tileIndices.ContainsKey(tile))
            {
                indexTiles.Add(tile);
                tileIndices.Add(tile, numTiles++);
            }
        }
        Debug.Log("numTiles = " + numTiles);
    }

    public void testSampleTiles()
    {
        foreach (Tuile tile in tiles)
        {
            Debug.Log(tile.tilename);
        }
    }

    public void testGenerateIndices()
    {
        foreach (KeyValuePair<Tuile, int> pair in tileIndices)
        {
            Debug.Log(pair.Key.gameObject.name + " " + pair.Value + " " + indexTiles[pair.Value].gameObject.name);
        }
        Debug.Log("numTiles " + numTiles);
    }

    public void testGenerateRules()
    {
        string[] tileNames = new string[numTiles];
        string[] directions = new string[] {"Left", "Right", "Top", "Bottom", "Front", "Back"};
        foreach (KeyValuePair<Tuile, int> pair in tileIndices)
        {
            Debug.Log("Tile " + pair.Key.gameObject.name + " index " + pair.Value);
            tileNames[pair.Value] = pair.Key.gameObject.name;
        }
        for (int t1 = 0; t1 < numTiles; ++t1)
        {
            for (int t2 = 0; t2 < numTiles; ++t2)
            {
                for (int d = 0; d < NUM_DIRECTIONS; ++d)
                {
                    if (rules[t1, t2, d])
                    {
                        Debug.Log("t1 = " + tileNames[t1] + "\t\t\tt2 = " + tileNames[t2] + "\t\t\tdirection = " + directions[d]);
                    }
                }
            }
        }
    }

    public void setTiles(Tuile[] t)
    {
        tiles = t;
    }

    public void setWidth(int w)
    {
        width = w;
    }

    public void setHeight(int h)
    {
        height = h;
    }

    public void setDepth(int d)
    {
        depth = d;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SimpleTiledModelRules))]
public class SimpleTiledModelRulesEditor : Editor
{

    public override void OnInspectorGUI()
    {
        SimpleTiledModelRules me = (SimpleTiledModelRules)target;
        GUILayout.Label("The canvas must be entirely filled up");
        GUILayout.Label("Generate Rules        : Generate rules using the canvas");
        if (GUILayout.Button("Generate Rules"))
        {

            me.setTiles(me.GetComponent<SemiInteractiveGrid>().tuiles);
            me.setWidth(me.GetComponent<SemiInteractiveGrid>().width);
            me.setHeight(me.GetComponent<SemiInteractiveGrid>().height);
            me.setDepth(me.GetComponent<SemiInteractiveGrid>().depth);

            // me.testSampleTiles();
            //me.sampleTiles();
            me.generateIndices();
            me.testGenerateIndices();
            me.generateRules();
            me.testGenerateRules();
        }
        DrawDefaultInspector();
    }
}
#endif