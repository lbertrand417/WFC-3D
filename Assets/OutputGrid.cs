using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(WFC))]
public class OutputGrid : MonoBehaviour
{

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

        // Delete the previous display
        Clear();

        // Retrieve number of rows and cols and tilesize
        int width = this.gameObject.GetComponent<WFC>().width;
        int height = this.gameObject.GetComponent<WFC>().height;
        int depth = this.gameObject.GetComponent<WFC>().depth;
        int tileSize = this.gameObject.GetComponent<WFC>().gridsize;

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
                    float posX = col * tileSize - gridW / 2 + (float)tileSize / 2;
                    float posY = row * -tileSize + gridH / 2 - (float)tileSize / 2;
                    float posZ = dep * tileSize - gridD / 2 + (float)tileSize / 2;

                    //Debug.Log(tuile[col + width * row + height * width * dep].Count);

                    // If final tile found
                    if (tuile[col + width * row + height * width * dep].Count == 1)
                    {
                        Tuile tile = Instantiate(tuile[col + width * row + height * width * dep][0], transform);

                        // Fourth term : Take the offset of the pivot point into account
                        float offsetPosX = posX + tuile[col + width * row + height * width * dep][0].offset[0];
                        float offsetPosY = posY - tuile[col + width * row + height * width * dep][0].offset[1];
                        float offsetPosZ = posZ + tuile[col + width * row + height * width * dep][0].offset[2];

                        tile.transform.localPosition = new Vector3(offsetPosX , offsetPosY, offsetPosZ);


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
                                // i = indicex + divid * indicey + divid� * indicez
                                // Take the modulo of divid� to retrieve indicex + divid * indicey
                                int temp = (int)i % (divid * divid);
                                int indicex = (int)temp % divid;
                                //Debug.Log("x = " + indicex);
                                
                                int indicey = (int)temp / divid;
                                //Debug.Log("y = " + indicey);

                                // Take the int part of i / divid� to retrieve indicez
                                int indicez = (int) i / (divid * divid);
                                //Debug.Log("z = " + indicez);

                                // Give position of the given "small" tile
                                // First term : position of the "big" tile (the subgrid starts at the center of the big tile)
                                // Second term : Find the right location for the given "small" tile in the subgrid
                                // Third term + Fourth term : translation of the entire sub grid to the corner of the "big" tile
                                //      - Third term : center of the first "small" tile to the corner of the "big" tile
                                //      - Forth term : shift to put the corner of the first "small" tile to the corner of the "big" tile
                                // Fifth term : Take into account the offset of the tile
                                float localPosX = posX + indicex * tile.transform.localScale.x - (float) tileSize / 2 + (float)tile.transform.localScale.x / 2 + tile.offset[0] * tile.transform.localScale.x;
                                float localPosY = posY - indicey * tile.transform.localScale.y + (float) tileSize / 2 - (float)tile.transform.localScale.y / 2 - tile.offset[1] * tile.transform.localScale.y;
                                float localPosZ = posZ + indicez * tile.transform.localScale.z - (float) tileSize / 2 + (float)tile.transform.localScale.z / 2 + tile.offset[2] * tile.transform.localScale.z;

                                // Place the tile
                                tile.transform.localPosition = new Vector3(localPosX, localPosY, localPosZ);

                            }
                        }
                    }
                }
            }
        }

        // Remove red markers
        Clean();
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
}
