using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(OutputGrid))]
public class WFC : MonoBehaviour
{
    public int width = 4;
    public int height = 4;
    public int depth = 4;

    [HideInInspector]
    public int gridsize = 1;

    private List<Tuile> listTuile;
    private int numberTiles;

    public SimpleTiledModelRules rules;

    private static bool[,] grid;

    private bool contradiction = false;

    void OnValidate()
    {
        BoxCollider bounds = this.GetComponent<BoxCollider>();
        bounds.size = new Vector3(width * gridsize, (height * gridsize), depth * gridsize);
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
        //Debug.Log("examine " + x);

        List<Tuile>[] test = fromGridToList();


        if ((int) ((x % (width * height)) / width) > 0)
        {
            if (sparse(x, x - width, Direction.Top))
            {
                WFCfunction(x - width);
            }
        }
        if ((x % (width * height)) % width < width - 1)
        {
            if (sparse(x, x + 1, Direction.Right))
            {
                WFCfunction(x + 1);
            }
        }
        //if (x % (width * height) < (height - 1) * width)
        if ((int) ((x % (width * height)) / width) < height - 1)
        {
            if (sparse(x, x + width, Direction.Bottom))
            {
                WFCfunction(x + width);
            }
        }
        //if (x % width > 0)
        if ((x % (width * height)) % width > 0)
        {
            if (sparse(x, x - 1, Direction.Left))
            {
                WFCfunction(x - 1);
            }
        }
        if ((int) (x / (width * height)) > 0)
        {
            if(sparse(x, x - width * height, Direction.Front))
            {
                WFCfunction(x - width * height);
            }
        }
        if ((int) (x / (width * height)) < (depth - 1))
        {
            if(sparse(x, x + width * height, Direction.Back))
            {
                WFCfunction(x + width * height);
            }
        }
    }

    public List<Tuile>[] fromGridToList()
    {
        List<Tuile>[] convertedGrid = new List<Tuile>[width * height * depth];
        for (int indice = 0; indice < width * height * depth; indice++)
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
        //Debug.Log("OK");
        listTuile = rules.getTuiles();
        numberTiles = listTuile.Count;

        // If the grid isn't initialized yet restart
        if (grid == null)
        {
            Restart();
        }

        // If the variables have changed restart
        if (grid.GetLength(0) != width * height * depth || numberTiles != listTuile.Count)
        {
            Restart();
        }

        // If the previous output was found restart
        if (Finished())
        {
            Restart();
        }

        // Run a step while the output is not totally filled in
        bool finished = false;
        while (!finished)
        {
            RunOneStep();
            finished = Finished();
        }

        // Print the final output
        this.gameObject.GetComponent<OutputGrid>().UpdateGrid(fromGridToList(), listTuile);

    }

    public void Restart()
    {
        // Initialize variables
        contradiction = false;
        listTuile = rules.getTuiles();
        numberTiles = listTuile.Count;

        grid = new bool[width * height * depth, numberTiles];
        
        for (int i = 0; i < width * height * depth; i++)
        {
            for (int j = 0; j < numberTiles; j++)
            {
                grid[i, j] = true;
            }
        }

        // Clear the canvas
        this.gameObject.GetComponent<OutputGrid>().Clear();
    }


    // If there is only one true per slot then the algorithm is finished
    private bool Finished()
    {
        if (grid == null)
        {
            return false;
        }
        else
        {
            for (int i = 0; i < width * height * depth; i++)
            {
                int numberTrue = 0;
                for (int j = 0; j < numberTiles; j++)
                {
                    if (grid[i, j])
                    {
                        numberTrue += 1;
                    }
                }

                if (numberTrue != 1)
                {
                    return false;
                }
            }
        }
        return true;
    }

    // Run one step of the WFC (until the end of the propagation)
    public void RunOneStep()
    {
        

        // If there were a contradiction or the grid doesn't exist then restart
        if (grid == null || contradiction)
        {
            Restart();
        }

        // If the size of the grid or if the size of the dictionnary of tiles changed restart
        // The dictionnary is still reload in case the new one is different but has the same number of tiles
        // TO DO: Must check the list itself because the dict will be reinitialized but not the grid
        listTuile = rules.getTuiles();
        if (grid.GetLength(0) != width * height * depth || numberTiles != listTuile.Count)
        {
            Restart();
        }

        if (!Finished())
        {
            List<int> entropy = new List<int>();
            int mins = int.MaxValue;
            for (int i = 0; i < width * height * depth; i++)
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
                    // If s=0, then there's no solution so this is a contradiction.
                    contradiction = true;
                }
                if (s > 1)
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

            // The WFC must run only if there is no contradiction
            if (!contradiction)
            {
                System.Random aleatoire = new System.Random();
                // b case random avec la plus petite entropie
                int b = aleatoire.Next(entropy.Count);
                int ind = entropy[b];
                // tuile a choisi random parmis les 'true' de la case b
                int a = aleatoire.Next(mins);
                //Debug.Log("a" + a);
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
            } else
            {
                Debug.Log("The program encounters a contradiction. Try again.");
            }
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







