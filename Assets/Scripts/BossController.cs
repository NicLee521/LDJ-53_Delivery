using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Linq;
using UnityEngine.Events;

public class BossController : MonoBehaviour
{
    private Vector3Int targetCell;
    [SerializeField]
    private Tilemap groundTilemap;
    [SerializeField]
    private Tilemap colisionTilemap;
    private TurnController turnController;
    private MapController mapController;
    private PlayerController playerController;
    [SerializeField]
    private List<Action> actions;
    private int currentAction = 0;
    public Tile damageTile;
    private bool damageTilesActivateNextTurn = false;
    [NonSerialized]
    public UnityEvent turnOffDamageTiles;
    public List<Vector3Int> currentDamageTiles;
    const string CONTROLLER_NAME = "Boss";

    void Awake() {
        if (turnOffDamageTiles == null) {
            turnOffDamageTiles = new UnityEvent();
        }
    }
    void Start()
    {
        turnController = FindObjectOfType<TurnController>();
        mapController = FindObjectOfType<MapController>();
        playerController = FindObjectOfType<PlayerController>();
        turnController.turnOrderUpdated.AddListener(TakeTurn);
        targetCell = groundTilemap.WorldToCell(transform.position);
        transform.position = groundTilemap.CellToWorld(targetCell);
    }

    void TakeTurn() {
        StartCoroutine(WaitForTurn());
    }

    private IEnumerator WaitForTurn() {
        if(CheckIfMyTurn()) {
            if(damageTilesActivateNextTurn && currentDamageTiles.Any()) { 
                yield return new WaitForSeconds(2);
                FireDamageTiles();
            }
            yield return new WaitForSeconds(2);
            Action action = actions[currentAction];
            switch(action.actionType) {
                case Action.actionTypes.Move:
                    Move(GetVector2FromString(action.actionOrder));
                    break;
                case Action.actionTypes.RandomAOE:
                    int amountOfAOEInstances = Int32.Parse(action.actionOrder);
                    RandomAOESpawn(amountOfAOEInstances);
                    break;
                case Action.actionTypes.PullPlayer:
                    PullPlayer();
                    break;
                case Action.actionTypes.ConeAttack:

                    break;
                default:
                    break;
            }
            currentAction++;
            turnController.NextTurn(CONTROLLER_NAME);
        }
    }
    void Move(Vector2 direction) {
        if(CanMove(direction)) {
            targetCell += Vector3Int.RoundToInt(direction);
            Vector3 targetPosition = groundTilemap.CellToWorld(targetCell);
            transform.position = targetPosition;
            turnController.NextTurn(CONTROLLER_NAME);
        }
    }

    void RandomAOESpawn(int instance) {
        while(instance > 0) {
            int randomTileIndex = UnityEngine.Random.Range(0, mapController.mapDict.Count());
            KeyValuePair<Vector3Int, TileData> randomTile = mapController.mapDict.ElementAt(randomTileIndex);
            List<Vector3Int> tilesToChange = new List<Vector3Int>();

            tilesToChange.Add(randomTile.Key);
            if(mapController.mapDict.ContainsKey(randomTile.Key + Vector3Int.down)) {
                tilesToChange.Add(randomTile.Key + Vector3Int.down);
            }
            if(mapController.mapDict.ContainsKey(randomTile.Key + Vector3Int.up)) {
                tilesToChange.Add(randomTile.Key + Vector3Int.up);
            }
            if(mapController.mapDict.ContainsKey(randomTile.Key + Vector3Int.left)) {
                tilesToChange.Add(randomTile.Key + Vector3Int.left);
            }
            if(mapController.mapDict.ContainsKey(randomTile.Key + Vector3Int.right)) {
                tilesToChange.Add(randomTile.Key + Vector3Int.right);
            }
            Vector3Int oneWithNoZ = new Vector3Int(1,1,0);
            if(mapController.mapDict.ContainsKey(randomTile.Key + oneWithNoZ)) {
                tilesToChange.Add(randomTile.Key + oneWithNoZ);
            }
            currentDamageTiles.AddRange(tilesToChange);
            foreach (Vector3Int tileLoc in tilesToChange) {
                mapController.mapDict[tileLoc].MakeDamageTile(damageTile, turnOffDamageTiles);
            }
            damageTilesActivateNextTurn = true;
            instance--;
        }
    }

    void PullPlayer() {
        Vector3Int newPlayerLocation = FindFirstExistingAdjacentTile();
        Vector3 targetPosition = groundTilemap.CellToWorld(newPlayerLocation);
        playerController.targetCell = newPlayerLocation;
        playerController.gameObject.transform.position = targetPosition;
    }

    Vector3Int FindFirstExistingAdjacentTile() {
        if(mapController.mapDict.ContainsKey(targetCell + Vector3Int.down)) {
            return(targetCell + Vector3Int.down);
        }
        if(mapController.mapDict.ContainsKey(targetCell + Vector3Int.up)) {
            return(targetCell + Vector3Int.up);
        }
        if(mapController.mapDict.ContainsKey(targetCell + Vector3Int.left)) {
            return(targetCell + Vector3Int.left);
        }
        if(mapController.mapDict.ContainsKey(targetCell + Vector3Int.right)) {
            return(targetCell + Vector3Int.right);
        }
        Vector3Int oneWithNoZ = new Vector3Int(1,1,0);
        if(mapController.mapDict.ContainsKey(targetCell + oneWithNoZ)) {
            return(targetCell + oneWithNoZ);
        }
        return playerController.targetCell;
    }
    
    void FireDamageTiles() {
        if(currentDamageTiles.Contains(playerController.targetCell)) {
            playerController.Lose();
        }
        turnOffDamageTiles.Invoke();
        currentDamageTiles.Clear();
        damageTilesActivateNextTurn = false;
    }

    bool CanMove(Vector2 direction) {
        Vector3Int tempTarget = targetCell;
        tempTarget += Vector3Int.RoundToInt(direction);
        Vector3 targetPosition = groundTilemap.CellToWorld(tempTarget);
        Vector3Int gridPosition = groundTilemap.WorldToCell(targetPosition);
        if(!groundTilemap.HasTile(gridPosition) || colisionTilemap.HasTile(gridPosition)) {
            return false;
        }
        return true;
    }

    bool CheckIfMyTurn() {
        return turnController.IsMyTurn(CONTROLLER_NAME);
    }

    private Vector2 GetVector2FromString(string vector2String)
    {
        string[] temp = vector2String.Split(',');       
        float floatx = float.Parse(temp[0]);
        float floaty = float.Parse(temp[1]);;
        return new Vector2(floatx, floaty);
    }
}
[System.Serializable]
public struct Action {
    public enum actionTypes {
        Move,
        RandomAOE, 
        PullPlayer,
        ConeAttack
    }
    public actionTypes actionType;
    public string actionOrder;
}
