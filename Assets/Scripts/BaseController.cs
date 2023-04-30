using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class BaseController : MonoBehaviour
{
    [SerializeField]
    protected Tilemap groundTilemap;
    [SerializeField]
    protected Tilemap colisionTilemap;
    [NonSerialized]
    public Vector3Int targetCell;
    protected string CONTROLLER_NAME = "not supported";
    protected float THINKING_TIME = 1;
    [SerializeField]
    protected int totalMoveActions = 1;
    protected int currentMoveActions;
    protected TurnController turnController;
    protected MapController mapController;

    protected void SharedSetUp() {
        turnController = FindObjectOfType<TurnController>();
        mapController = FindObjectOfType<MapController>();
        targetCell = groundTilemap.WorldToCell(transform.position);
        UpdateOccupiedCell(targetCell);
        transform.position = groundTilemap.CellToWorld(targetCell);
        currentMoveActions = totalMoveActions;
    }
    protected virtual void Move(Vector2 direction) {
        if(CanMove(direction) && CheckIfMyTurn()) {
            Vector3Int prevCell = targetCell;
            targetCell += Vector3Int.RoundToInt(direction);
            UpdateOccupiedCell(targetCell, prevCell);
            Vector3 targetPosition = groundTilemap.CellToWorld(targetCell);
            transform.position = targetPosition;
            DecrementAndCheckCurrentMoveActions();
        }
    }

    protected void DecrementAndCheckCurrentMoveActions() {
        currentMoveActions--;
        if(currentMoveActions <= 0) {
            turnController.NextTurn(CONTROLLER_NAME);
            currentMoveActions = totalMoveActions;
        }
    }

    protected Vector2 GetNewDirection(Vector2 direction) {
        if (direction == Vector2.down || direction == new Vector2(-1,1)) {
            return Vector2.left;
        }
        if (direction == Vector2.up || direction == new Vector2(1,-1)) {
            return Vector2.right;
        }
        if (direction == Vector2.right || direction == new Vector2(-1,-1)) {
            return Vector2.down;
        }
        if (direction == Vector2.left || direction == new Vector2(1,1)) {
            return Vector2.up;
        }
        return Vector2.zero;
    }

    protected bool CanMove(Vector2 direction) {
        Vector3Int tempTarget = targetCell;
        tempTarget += Vector3Int.RoundToInt(direction);
        if(!groundTilemap.HasTile(tempTarget) || colisionTilemap.HasTile(tempTarget)) {
            return false;
        }
        return true;
    }

    protected bool CheckIfMyTurn() {
        return turnController.IsMyTurn(CONTROLLER_NAME);
    }

    protected Vector2 GetVector2FromString(string vector2String){
        string[] temp = vector2String.Split(',');       
        float floatx = float.Parse(temp[0]);
        float floaty = float.Parse(temp[1]);;
        return new Vector2(floatx, floaty);
    }

    protected Vector2 GetVectorDirectionFromString(string vector2String){
        vector2String = vector2String.ToLower();
        switch(vector2String) {
            case "up":
                return Vector2.up;
            case "down":
                return Vector2.down;
            case "left":
                return Vector2.left;
            case "right":
                return Vector2.right;
            default:
                return GetVector2FromString(vector2String);
        }
    }

    protected bool IsCellOccupied(Vector3Int cell) {
        if(mapController.mapDict.ContainsKey(cell)){
            return mapController.mapDict[cell].isOccupied;
        }
        return false;
    }

    public void UpdateOccupiedCell(Vector3Int currCell, Vector3Int? prevCell = null) {
        if(prevCell != null) {
            mapController.mapDict[(Vector3Int)prevCell].isOccupied = false;
            mapController.mapDict[(Vector3Int)prevCell].isOccupiedBy = "none";
        }
        mapController.mapDict[currCell].isOccupied = true;
        mapController.mapDict[currCell].isOccupiedBy = CONTROLLER_NAME;
    }

    protected bool IsAdjacentCellOccupiedByBoss() {
        if(IsCellOccupied(targetCell + Vector3Int.up)) {
            if(mapController.mapDict[targetCell + Vector3Int.up].isOccupiedBy == "Boss") {
                return true;
            }
        } 
        if(IsCellOccupied(targetCell + Vector3Int.down)) {
            if(mapController.mapDict[targetCell + Vector3Int.down].isOccupiedBy == "Boss") {
                return true;
            }
        }
        if(IsCellOccupied(targetCell + Vector3Int.right)) {
            if(mapController.mapDict[targetCell + Vector3Int.right].isOccupiedBy == "Boss") {
                return true;
            }
        }
        if(IsCellOccupied(targetCell + Vector3Int.left)) {
            if(mapController.mapDict[targetCell + Vector3Int.left].isOccupiedBy == "Boss") {
                return true;
            }
        }
        return false;
    }

}
