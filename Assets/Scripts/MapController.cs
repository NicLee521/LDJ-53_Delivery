using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


public class MapController : MonoBehaviour
{
    public Tilemap groundTilemap;
    public Dictionary<Vector3Int, TileData> mapDict = new Dictionary<Vector3Int, TileData>();

    // Start is called before the first frame update
    void Start()
    {
        foreach (Vector3Int position in groundTilemap.cellBounds.allPositionsWithin){
            Tile tile = groundTilemap.GetTile<Tile>(position);
            if(tile != null) {
                mapDict.Add(position, new TileData(tile, position, this));
            }
        }
    }
}
