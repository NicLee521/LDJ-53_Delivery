using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BaseController : MonoBehaviour
{
    [SerializeField]
    protected Tilemap groundTilemap;
    [SerializeField]
    protected Tilemap colisionTilemap;
    public Vector3Int targetCell;
    protected string CONTROLLER_NAME = "not supported";
    protected TurnController turnController;
    protected MapController mapController;

    protected void SharedSetUp() {
        turnController = FindObjectOfType<TurnController>();
        mapController = FindObjectOfType<MapController>();
        targetCell = groundTilemap.WorldToCell(transform.position);
        transform.position = groundTilemap.CellToWorld(targetCell);

    }
    // Start is called before the first frame update
    protected void Move(Vector2 direction) {
        if(CanMove(direction)) {
            targetCell += Vector3Int.RoundToInt(direction);
            Vector3 targetPosition = groundTilemap.CellToWorld(targetCell);
            transform.position = targetPosition;
            turnController.NextTurn(CONTROLLER_NAME);
        }
    }

    protected bool CanMove(Vector2 direction) {
        Vector3Int tempTarget = targetCell;
        tempTarget += Vector3Int.RoundToInt(direction);
        Vector3 targetPosition = groundTilemap.CellToWorld(tempTarget);
        Vector3Int gridPosition = groundTilemap.WorldToCell(targetPosition);
        if(!groundTilemap.HasTile(gridPosition) || colisionTilemap.HasTile(gridPosition) || !CheckIfMyTurn()) {
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
}
