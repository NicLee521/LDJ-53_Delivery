using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

public class TileData
{
    public Tile originalTile;
    public Vector3Int tilePos;
    public Tile currentTile;
    public bool willDamage;
    private MapController mapController;
    public bool isOccupied;
    public string isOccupiedBy = "none";
    public TileData(Tile tile, Vector3Int tilePosition, MapController mapController) {
        this.originalTile = tile;
        this.currentTile = tile;
        this.tilePos = tilePosition;
        this.mapController = mapController;
    }

    public void MakeDamageTile(Tile damageTile, UnityEvent turnOffDamageTiles) {
        this.currentTile = damageTile;
        this.mapController.groundTilemap.SetTile(this.tilePos, damageTile);
        turnOffDamageTiles.AddListener(UndoDamageTile);
    }

    public void UndoDamageTile() {
        this.currentTile = this.originalTile;
        this.mapController.groundTilemap.SetTile(this.tilePos, this.originalTile);
    }
}
