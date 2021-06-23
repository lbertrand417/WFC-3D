using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

//[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(OutputGrid))]
public class WFC : MonoBehaviour
{
    public int width = 4;
    public int height = 4;

    [HideInInspector]
    public int gridsize = 1;

    private List<Tuile> listTuile;
    private int numberTiles;

    public SimpleTiledModelRules rules;

    private static bool[,] grid;

    void OnValidate()
    {

        BoxCollider bounds = this.GetComponent<BoxCollider>();
        bounds.center = new Vector3((width * gridsize) * 0.5f - gridsize * 0.5f, (height * gridsize) * 0.5f - gridsize * 0.5f, 0f);
        bounds.size = new Vector3(width * gridsize, (height * gridsize), 0f);
    }

    public bool sparse(int x, int voisin, Direction d)
    {
        bool changement = false;
        bool[] result = new bool[numberTiles];
        for (int a = 0; a < numberTiles; a++)
        {
            if (grid[x, a])
            {
                for (int b = 0; b < numberTiles; b++)
                {
                    //Debug.Log(rules.check(listTuile[b], listTuile[a], d));
                    result[b] = result[b] || rules.check(listTuile[b], listTuile[a], d);
                }

            }
        }

        for (int j = 0; j < numberTiles; j++)
        {
            if (!result[j] && grid[voisin, j])
            {
                changement = true;
                grid[voisin, j] = false;
                //Debug.Log("pop("+voisin+","+j+")");
            }
        }

        return changement;

    }

    public void WFCfunction(int x)
    {
        //nouvelle itération de WFCfunction
        Debug.Log("examine " + x);

        List<Tuile>[] test = fromGridToList();


        if (x > width - 1)
        {
            if (sparse(x, x - width, Direction.Top))
            {
                WFCfunction(x - width);
            }
        }
        if (x % width < width - 1)
        {
            if (sparse(x, x + 1, Direction.Right))
            {
                WFCfunction(x + 1);
            }
        }
        if (x < (height - 1) * width)
        {
            if (sparse(x, x + width, Direction.Bottom))
            {
                WFCfunction(x + width);
            }
        }
        if (x % width > 0)
        {
            if (sparse(x, x - 1, Direction.Left))
            {
                WFCfunction(x - 1);
            }
        }
    }

    public List<Tuile>[] fromGridToList()
    {
        List<Tuile>[] convertedGrid = new List<Tuile>[width * height];
        for (int indice = 0; indice < width * height; indice++)
        {
            List<Tuile> possibilities = new List<Tuile>();
            for (int indiceDict = 0; indiceDict < numberTiles; indiceDict++)
            {
                if (grid[indice, indiceDict])
                {
                    possibilities.Add(rules.getTuiles()[indiceDict]);
                }
            }
            Debug.Log("Number of poss :" + possibilities.Count);
            convertedGrid[indice] = possibilities;
        }
        return convertedGrid;
    }

    public void Run()
    {
        Debug.Log("OK");
        listTuile = rules.getTuiles();
        numberTiles = listTuile.Count;

        grid = new bool[width * height, numberTiles];
        for (int i = 0; i < width * height; i++)
        {
            for (int j = 0; j < numberTiles; j++)
            {
                grid[i, j] = true;
            }
        }

        bool finished = false;
        while (!finished)
        {
            RunOneStep();
            finished = Finished();
        }

        this.gameObject.GetComponent<OutputGrid>().UpdateGrid(fromGridToList(), rules.getTuiles());

    }

    public void Restart()
    {
        grid = new bool[width * height, numberTiles];
        for (int i = 0; i < width * height; i++)
        {
            for (int j = 0; j < numberTiles; j++)
            {
                grid[i, j] = true;
            }
        }

        this.gameObject.GetComponent<OutputGrid>().Clear();
    }

    private bool Finished()
    {
        if (grid == null)
        {
            return false;
        }
        else
        {
            for (int i = 0; i < width * height; i++)
            {
                int numberTrue = 0;
                for (int j = 0; j < numberTiles; j++)
                {
                    if (grid[i, j])
                    {
                        numberTrue += 1;
                    }
                }

                if (numberTrue > 1)
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void RunOneStep()
    {
        listTuile = rules.getTuiles();
        numberTiles = listTuile.Count;

        if (grid == null)
        {
            Restart();
        }

        if (!Finished())
        {
            List<int> entropy = new List<int>();
            int mins = int.MaxValue;
            for (int i = 0; i < width * height; i++)
            {
                int s = 0;
                for (int j = 0; j < numberTiles; j++)
                {
                    if (grid[i, j])
                    {
                        s += 1;
                    }
                }
                if (s == 0)
                {
                    Debug.Log("aie une des cases n'a plus aucune possibilité");
                    // IMPORTANT il faudrait recommencer -> rappeler Run()
                    Restart();
                    RunOneStep();
                    break;
                }
                if (s >= 1)
                {
                    if (s == mins)
                    { // i est aussi d'entropie minimale -> on l'ajoute
                        entropy.Add(i);
                    }
                    if (s < mins)
                    { // i a une entropy strictement plus petite -> on remplace entropy
                        entropy.Clear();
                        entropy.Add(i);
                        mins = s;
                    }
                }
            }

            System.Random aleatoire = new System.Random();
            // b case random avec la plus petite entropie
            int b = aleatoire.Next(entropy.Count);
            int ind = entropy[b];
            // tuile a choisi random parmis les 'true' de la case b
            int a = aleatoire.Next(mins);
            for (int i = 0; i < numberTiles; i++)
            {
                if (grid[ind, i])
                {
                    if (a != 0)
                    {
                        grid[ind, i] = false;
                    }
                    a--;
                }
            }

            WFCfunction(ind);
        }
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(WFC))]
public class WFCEditor : Editor
{
    public override void OnInspectorGUI()
    {
        WFC me = (WFC)target;
        if (GUILayout.Button("RUN"))
        {
            Debug.Log("Run");
            me.Run();
        }

        if (GUILayout.Button("RUN one step"))
        {
            Debug.Log("Run one step");
            me.RunOneStep();
            me.gameObject.GetComponent<OutputGrid>().UpdateGrid(me.fromGridToList(), me.rules.getTuiles());

        }

        if (GUILayout.Button("RESTART"))
        {
            Debug.Log("Restart");
            me.Restart();
        }
        DrawDefaultInspector();
    }
}
#endif







