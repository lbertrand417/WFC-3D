using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class OutputGrid : MonoBehaviour
{

    public List<Tuile> globalList;

    // Raw values
    private int width = 2;
    private int height = 2;
    private int depth = 2;
    private int tileSize = 1;

#if UNITY_EDITOR
    void OnValidate()
    {
        // Should be in the "main" script
        BoxCollider bounds = this.GetComponent<BoxCollider>();
        bounds.size = new Vector3(width * tileSize, (height * tileSize), depth * tileSize);
    }

    // Return true if the tile is possible at a given tile placement
    // possibilities : List of possible tiles at a tile placement
    // myTuile : The tile for which we want to know if it is possible
    public bool isPossible(List<Tuile> possibilities, Tuile myTuile)
    {
        foreach (Tuile p in possibilities)
        {
            //if (myTuile.tilename.Equals(p.tilename))
            if (myTuile.GetInstanceID() == p.GetInstanceID())
            {
                return true;
            }
        }
        return false;
    }

    // Print the grid of a given step
    // tuile : Array of list. One element of the array represents a tile placement and the list represents
    //          the possible tiles at the given placement
    // dictTuile : All existing tiles 
    public void UpdateGrid(List<Tuile>[] tuile, List<Tuile> dictTuile)
    {
        // Size of the canvas
        float gridW = width * tileSize;
        float gridH = height * tileSize;
        float gridD = depth * tileSize;

        // For each tile placement
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                for (int dep = 0; dep < depth; dep++)
                {
                    // Give the position of the tile
                    // First term : Find the right location for the given "big" tile
                    // Second term + third term : translation of the entire tile grid to the corner of the canvas
                    //      - second term : center of the first tile to the corner of the canvas
                    //      - third term : shift to put the corner of the first tile to the corner of the canvas
                    // Fourth term : Take the offset of the pivot point into account
                    float posX = col * tileSize - gridW / 2 + (float)tileSize / 2 + tuile[col + width * row + height * width * dep][0].offset[0];
                    float posY = row * -tileSize + gridH / 2 - (float)tileSize / 2 - tuile[col + width * row + height * width * dep][0].offset[1];
                    float posZ = dep * tileSize - gridD / 2 + (float)tileSize / 2 + tuile[col + width * row + height * width * dep][0].offset[2];

                    //Debug.Log(tuile[col + width * row + height * width * dep].Count);

                    // If final tile found
                    if (tuile[col + width * row + height * width * dep].Count == 1)
                    {
                        Tuile tile = Instantiate(tuile[col + width * row + height * width * dep][0], transform);

                        tile.transform.localPosition = new Vector3(posX, posY, posZ);


                    }
                    else
                    {
                        // Size of the subgrid (divid x divid)
                        int divid = (int)Math.Ceiling(Math.Pow(dictTuile.Count, (float) 1/3));
                        //Debug.Log("divid :" + divid);

                        for (int i = 0; i < dictTuile.Count; i++)
                        {
                            // If the tile is a possibility for this spot
                            if (isPossible(tuile[col + width * row + height * width * dep], dictTuile[i]))
                            {
                                // Create the tile
                                Tuile tile = Instantiate(dictTuile[i], transform);

                                // Change the scale of the tile
                                tile.transform.localScale = tile.transform.localScale / divid;

                                // Indices in the sub grid of the given "small" tile
                                // i = indicex + divid * indicey + divid² * indicez
                                // Take the modulo of divid² to retrieve indicex + divid * indicey
                                int temp = (int)i % (divid * divid);
                                int indicex = (int)temp % divid;
                                //Debug.Log("x = " + indicex);
                                
                                int indicey = (int)temp / divid;
                                //Debug.Log("y = " + indicey);

                                // Take the int part of i / divid² to retrieve indicez
                                int indicez = (int) i / (divid * divid);
                                //Debug.Log("z = " + indicez);

                                // Give position of the given "small" tile
                                // First term : position of the "big" tile (the subgrid starts at the center of the big tile)
                                // Second term + third term : translation of the entire sub grid to the corner of the "big" tile
                                //      - second term : center of the first "small" tile to the corner of the "big" tile
                                //      - third term : shift to put the corner of the first "small" tile to the corner of the "big" tile
                                // Fourth term : Find the right location for the given "small" tile

                                float localPosX = posX - (float)divid / 2 * tile.transform.localScale.x + (float)tile.transform.localScale.x / 2 + indicex * tile.transform.localScale.x;
                                float localPosY = posY + (float)divid / 2 * tile.transform.localScale.y + (float)tile.transform.localScale.y / 2 - indicey * tile.transform.localScale.y;
                                float localPosZ = posZ - (float)divid / 2 * tile.transform.localScale.z + (float)tile.transform.localScale.z / 2 + indicez * tile.transform.localScale.z;

                                // Place the tile
                                tile.transform.localPosition = new Vector3(localPosX, localPosY, localPosZ);

                            }
                        }
                    }
                }
            }
        }
    }

    // Remove game objects of the previous step
    public void Clear()
    {
        // Find all childs
        int childs = this.gameObject.transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
        {
            DestroyImmediate(this.gameObject.transform.GetChild(i).gameObject);
        }
    }

    // Remove markers of the tile (here, red square at the corner)
    public void Clean()
    {
        // Find all childs
        int childs = this.gameObject.transform.childCount;
        for (int i = childs - 1; i >= 0; i--)
        {
            GameObject subChild = this.gameObject.transform.GetChild(i).gameObject;

            // Find childs of the child
            int subChilds = subChild.transform.childCount;
            for (int j = subChilds - 1; j >= 0; j--)
            {
                DestroyImmediate(subChild.transform.GetChild(j).gameObject);
            }
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(OutputGrid))]
public class OutputGridEditor : Editor
{
    public override void OnInspectorGUI()
    {
        OutputGrid me = (OutputGrid)target;
        if (GUILayout.Button("RUN"))
        {

            List<Tuile>[] myGrid = new List<Tuile>[8];

            List<Tuile> list1 = new List<Tuile>();
            list1.Add(me.globalList[0]);
            myGrid[0] = list1;

            List<Tuile> list2 = new List<Tuile>();
            list2.Add(me.globalList[0]);
            list2.Add(me.globalList[1]);
            myGrid[1] = list2;

            List<Tuile> list3 = new List<Tuile>();
            list3.Add(me.globalList[1]);
            myGrid[2] = list3;

            List<Tuile> list4 = new List<Tuile>();
            list4.Add(me.globalList[0]);
            myGrid[3] = list4;

            List<Tuile> list5 = new List<Tuile>();
            list5.Add(me.globalList[1]);
            myGrid[4] = list5;

            List<Tuile> list6 = new List<Tuile>();
            list6.Add(me.globalList[1]);
            myGrid[5] = list6;

            List<Tuile> list7 = new List<Tuile>();
            list7.Add(me.globalList[0]);
            list7.Add(me.globalList[1]);
            list7.Add(me.globalList[2]);
            myGrid[6] = list7;

            List<Tuile> list8 = new List<Tuile>();
            list8.Add(me.globalList[0]);
            myGrid[7] = list8;

            me.Clear();
            me.UpdateGrid(myGrid, me.globalList);
            me.Clean();
        }
        DrawDefaultInspector();
    }

}
#endif
