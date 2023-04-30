using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using System.Linq;
using UnityEngine.Events;

public class BossController : BaseController
{
    private PlayerController playerController;
    [SerializeField]
    private List<BossAction> actions;
    private int currentAction = 0;
    public Tile damageTile;
    private bool damageTilesActivateNextTurn = false;
    [NonSerialized]
    public UnityEvent turnOffDamageTiles;
    [NonSerialized]
    public List<Vector3Int> currentDamageTiles = new List<Vector3Int>();
    private Vector3Int tilePlayerPulledTo;
    public int health = 5;
    private List<GameObject> hearts = new List<GameObject>();
    private GameObject finalBlowParent;


    void Awake() {
        if (turnOffDamageTiles == null) {
            turnOffDamageTiles = new UnityEvent();
        }
        CONTROLLER_NAME = "Boss";
    }
    void Start()
    {
        SharedSetUp();
        playerController = FindObjectOfType<PlayerController>();
        finalBlowParent = GameObject.FindWithTag("FinalBlow");
        finalBlowParent.SetActive(false);
        turnController.turnOrderUpdated.AddListener(TakeTurn);
        int actionsLeft = (actions.Count - currentAction);
        turnController.roundsLeft.text = actionsLeft.ToString();
        GameObject healthParent = GameObject.Find("HealthParent");
        int heartsActive = 0;
        Transform[] children = healthParent.GetComponentsInChildren<Transform>(true);
        foreach(Transform child in children) {
            if(heartsActive < health && child.gameObject.name != "HealthParent") {
                child.gameObject.SetActive(true);
                hearts.Add(child.gameObject);
                heartsActive++;
            }
        }
    }

    void TakeTurn() {
        StartCoroutine(WaitForTurn());
    }

    public void TakeDamage(string source) {
        health--;
        UpdateHearts();
        if(health <= 0) {
            if(source == "Player") {
                finalBlowParent.SetActive(true);
                Animator finalBlow = finalBlowParent.GetComponentInChildren<Animator>();
                finalBlow.Play("FinalBlow");
                StartCoroutine(WaitToWin());
                return;
            }
            playerController.Lose();
            return;
        }
    }
    
    IEnumerator WaitToWin() {
        yield return new WaitForSeconds(1.5f);
        playerController.Win();
    }

    void UpdateHearts() {
        bool updateHearts = true;
        foreach(GameObject heart in hearts) {
            if(heart.activeInHierarchy && updateHearts) {
                heart.SetActive(false);
                updateHearts = false;
                return;
            }
        }
    }

    private IEnumerator WaitForTurn() {
        if(CheckIfMyTurn()) {
            if(damageTilesActivateNextTurn && currentDamageTiles.Any()) { 
                yield return new WaitForSeconds(THINKING_TIME);
                FireDamageTiles();
            }
            yield return new WaitForSeconds(THINKING_TIME);
            BossAction action = actions[currentAction];
            switch(action.actionType) {
                case BossAction.actionTypes.Move:
                    Move(GetVectorDirectionFromString(action.actionOrder));
                    break;
                case BossAction.actionTypes.RandomAOE:
                    int amountOfAOEInstances = Int32.Parse(action.actionOrder);
                    RandomAOESpawn(amountOfAOEInstances);
                    break;
                case BossAction.actionTypes.PullPlayer:
                    PullPlayer();
                    break;
                case BossAction.actionTypes.ConeAttack:
                    Vector2 actionVector = Vector2.zero;
                    try {
                        actionVector = GetVector2FromString(action.actionOrder);
                    } catch (Exception) {}
                    StartConeAttack(actionVector);
                    break;
                default:
                    break;
            }
            UpdateCurrentActionAndRoundsLeft();
            turnController.NextTurn(CONTROLLER_NAME);
        }
    }

    void UpdateCurrentActionAndRoundsLeft() {
        currentAction++;
        int totalActions = actions.Count;
        int actionsLeft = (totalActions - currentAction);
        turnController.roundsLeft.text = actionsLeft.ToString();
        if(actionsLeft <= 0) {
            playerController.LoseAtEndOfTurn();
        }
    }

    void Move(Vector2 direction) {
        if(CanMove(direction) && CheckIfMyTurn()) {
            Vector3Int prevCell = targetCell;
            if(IsCellOccupied(targetCell + Vector3Int.RoundToInt(direction))) {
                Vector2 newDirection = GetNewDirection(Vector2Int.RoundToInt(direction));
                Move(newDirection);
                return;
            }
            targetCell += Vector3Int.RoundToInt(direction);
            UpdateOccupiedCell(targetCell, prevCell);
            Vector3 targetPosition = groundTilemap.CellToWorld(targetCell);
            transform.position = targetPosition;
            DecrementAndCheckCurrentMoveActions();
        } else if(!CanMove(direction) && CheckIfMyTurn()) {
            DecrementAndCheckCurrentMoveActions();
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
            Vector3Int oneWithNoZ1 = new Vector3Int(-1,1,0);
            if(mapController.mapDict.ContainsKey(randomTile.Key + oneWithNoZ1)) {
                tilesToChange.Add(randomTile.Key + oneWithNoZ1);
            }
            Vector3Int oneWithNoZ2 = new Vector3Int(1,-1,0);
            if(mapController.mapDict.ContainsKey(randomTile.Key + oneWithNoZ2)) {
                tilesToChange.Add(randomTile.Key + oneWithNoZ2);
            }
            Vector3Int oneWithNoZ3 = new Vector3Int(-1,-1,0);
            if(mapController.mapDict.ContainsKey(randomTile.Key + oneWithNoZ3)) {
                tilesToChange.Add(randomTile.Key + oneWithNoZ3);
            }
            currentDamageTiles.AddRange(tilesToChange);
            foreach (Vector3Int tileLoc in tilesToChange) {
                mapController.mapDict[tileLoc].MakeDamageTile(damageTile, turnOffDamageTiles);
            }
            groundTilemap.RefreshAllTiles();
            damageTilesActivateNextTurn = true;
            instance--;
        }
    }

    void PullPlayer() {
        Vector3Int newPlayerLocation = FindFirstExistingAdjacentTile();
        Vector3 targetPosition = groundTilemap.CellToWorld(newPlayerLocation);
        tilePlayerPulledTo = newPlayerLocation;
        playerController.UpdateOccupiedCell(newPlayerLocation, playerController.targetCell);
        playerController.targetCell = newPlayerLocation;
        playerController.gameObject.transform.position = targetPosition;
    }

    void StartConeAttack(Vector2 attackTowardsLocation) {
        if(tilePlayerPulledTo == null) {
            tilePlayerPulledTo = new Vector3Int((int)attackTowardsLocation.x,(int)attackTowardsLocation.y, 0);
        }
        Vector3 direction = ((Vector3)tilePlayerPulledTo - (Vector3)targetCell).normalized;
        List<Vector3Int> tilesToChange = new List<Vector3Int>();
        if(direction == Vector3.down) {
            tilesToChange.Add(targetCell + Vector3Int.down);
            tilesToChange.Add(targetCell + Vector3Int.down + Vector3Int.right);
            tilesToChange.Add(targetCell + Vector3Int.down + Vector3Int.left);
            tilesToChange.Add(targetCell + (Vector3Int.down * 2));
            tilesToChange.Add(targetCell + (Vector3Int.down * 2) + Vector3Int.right);
            tilesToChange.Add(targetCell + (Vector3Int.down * 2) + Vector3Int.left);
            tilesToChange.Add(targetCell + (Vector3Int.down * 2) + (Vector3Int.right * 2));
            tilesToChange.Add(targetCell + (Vector3Int.down * 2) + (Vector3Int.left * 2));
        } else if (direction == Vector3.up) {
            tilesToChange.Add(targetCell + Vector3Int.up);
            tilesToChange.Add(targetCell + Vector3Int.up + Vector3Int.right);
            tilesToChange.Add(targetCell + Vector3Int.up + Vector3Int.left);
            tilesToChange.Add(targetCell + (Vector3Int.up * 2));
            tilesToChange.Add(targetCell + (Vector3Int.up * 2) + Vector3Int.right);
            tilesToChange.Add(targetCell + (Vector3Int.up * 2) + Vector3Int.left);
            tilesToChange.Add(targetCell + (Vector3Int.up * 2) + (Vector3Int.right * 2));
            tilesToChange.Add(targetCell + (Vector3Int.up * 2) + (Vector3Int.left * 2));
        } else if (direction == Vector3.left) {
            tilesToChange.Add(targetCell + Vector3Int.left);
            tilesToChange.Add(targetCell + Vector3Int.left + Vector3Int.up);
            tilesToChange.Add(targetCell + Vector3Int.left + Vector3Int.down);
            tilesToChange.Add(targetCell + (Vector3Int.left * 2));
            tilesToChange.Add(targetCell + (Vector3Int.left * 2) + Vector3Int.up);
            tilesToChange.Add(targetCell + (Vector3Int.left * 2) + Vector3Int.down);
            tilesToChange.Add(targetCell + (Vector3Int.left * 2) + (Vector3Int.up * 2));
            tilesToChange.Add(targetCell + (Vector3Int.left * 2) + (Vector3Int.down * 2));
        } else if (direction == Vector3.right) {
            tilesToChange.Add(targetCell + Vector3Int.right);
            tilesToChange.Add(targetCell + Vector3Int.right + Vector3Int.up);
            tilesToChange.Add(targetCell + Vector3Int.right + Vector3Int.down);
            tilesToChange.Add(targetCell + (Vector3Int.right * 2));
            tilesToChange.Add(targetCell + (Vector3Int.right * 2) + Vector3Int.up);
            tilesToChange.Add(targetCell + (Vector3Int.right * 2) + Vector3Int.down);
            tilesToChange.Add(targetCell + (Vector3Int.right * 2) + (Vector3Int.up * 2));
            tilesToChange.Add(targetCell + (Vector3Int.right * 2) + (Vector3Int.down * 2));
        }
        foreach (Vector3Int tileLoc in tilesToChange) {
            if (mapController.mapDict.ContainsKey(tileLoc)) {
                mapController.mapDict[tileLoc].MakeDamageTile(damageTile, turnOffDamageTiles);
            } else {
                Debug.Log("Tile doesnt exist in tilmap");
            }
        }
        currentDamageTiles.AddRange(tilesToChange);
        damageTilesActivateNextTurn = true;
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


    
}
[System.Serializable]
public struct BossAction {
    public enum actionTypes {
        Move,
        RandomAOE, 
        PullPlayer,
        ConeAttack
    }
    public actionTypes actionType;
    public string actionOrder;
}
