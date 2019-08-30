using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilesStash : ScriptableObject
{
    private  static List<Tile> tiles = new List<Tile>();
    
    public static Tile GetTile()
    {
        Tile tile = null;
        if (tiles.Count > 0)
        {
            tile =  tiles[0];
            tiles.RemoveAt(0);
        }

        return tile;
    }

    public static void AddTile(Tile tile)
    {
        if (!tiles.Exists(item => tile.gameObject == tile.gameObject)) //Check for tile already exists in stash
            tiles.Add(tile);

        tile.gameObject.SetActive(false);
    }
}
