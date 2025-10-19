using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour
{
    public Tilemap tilemap;
    public List<Vector3> tileWorldLocations;

    void Start()
    {
        tilemap = GetComponent<Tilemap>();
        tileWorldLocations = new List<Vector3>();

        foreach (var pos in tilemap.cellBounds.allPositionsWithin)
        {
            Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
            Vector3 place = tilemap.CellToWorld(localPlace);
            if (tilemap.HasTile(localPlace))
            {
                tileWorldLocations.Add(place);
            }
        }
    }

    public Vector3 GetRandomTile()
    {
        var targetTile = tileWorldLocations[Random.Range(0, tileWorldLocations.Count)];
        Debug.Log("TARGETTILE: " + targetTile);

        return targetTile;
    }

    // public TileBase[] allTiles;

    // void Start()
    // {
    //     Tilemap tilemap = GetComponent<Tilemap>();

    //     BoundsInt bounds = tilemap.cellBounds;
    //     TileBase[] allTiles = tilemap.GetTilesBlock(bounds);

    //     for (int x = 0; x < bounds.size.x; x++)
    //     {
    //         for (int y = 0; y < bounds.size.y; y++)
    //         {
    //             TileBase tile = allTiles[x + y * bounds.size.x];
    //             if (tile != null)
    //             {
    //                 Debug.Log("x:" + x + " y:" + y + " tile:" + tile.name);
    //             }
    //             else
    //             {
    //                 Debug.Log("x:" + x + " y:" + y + " tile: (null)");
    //             }
    //         }
    //     }
    // }

    // public void ShowArray() {
    //     Debug.Log("THE ARRAY: ", allTiles.ToString);
    // }
}
