using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : BaseController
{
    
    private PlayerMovement controls;
    private BossController bossController;
    private FriendController friendController;
    private bool willLose = false;

    void Awake() {
        controls = new PlayerMovement();
        CONTROLLER_NAME = "Player";
    }

    void OnEnable() {
        controls.Enable();
    }

    void OnDisable() {
        controls.Disable();
    }
    // Start is called before the first frame update
    void Start()
    {
        SharedSetUp();
        bossController = FindObjectOfType<BossController>();
        friendController = FindObjectOfType<FriendController>();
        
        controls.Main.Movement.performed += context => Move(context.ReadValue<Vector2>());
        controls.Main.Teleport.performed += context => Teleport();
        controls.Main.FinalBlow.performed += context => FinalBlow();
    }

    void Move(Vector2 direction) {
        if(CanMove(direction) && CheckIfMyTurn() && !IsCellOccupied(targetCell + Vector3Int.RoundToInt(direction))) {
            Vector3Int prevCell = targetCell;
            targetCell += Vector3Int.RoundToInt(direction);
            UpdateOccupiedCell(targetCell, prevCell);
            Vector3 targetPosition = groundTilemap.CellToWorld(targetCell);
            transform.position = targetPosition;
            if(currentMoveActions <= 1 && willLose) { 
                Lose();
            }
            DecrementAndCheckCurrentMoveActions();
        }
    }

    void FinalBlow() {
        if(currentMoveActions == totalMoveActions && CheckIfMyTurn()) {
            if(IsNearCellOccupiedByBoss()) {
                bossController.TakeDamage(CONTROLLER_NAME);
                if(willLose && bossController.health > 0) {
                    Lose();
                }
            }
        }
    }

    protected bool IsNearCellOccupiedByBoss() {
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
        if(IsCellOccupied(new Vector3Int(1,1,0))) {
            if(mapController.mapDict[new Vector3Int(1,1,0)].isOccupiedBy == "Boss") {
                return true;
            }
        }
        if(IsCellOccupied(new Vector3Int(-1,-1,0))) {
            if(mapController.mapDict[new Vector3Int(-1,-1,0)].isOccupiedBy == "Boss") {
                return true;
            }
        }
        if(IsCellOccupied(new Vector3Int(1,-1,0))) {
            if(mapController.mapDict[new Vector3Int(1,-1,0)].isOccupiedBy == "Boss") {
                return true;
            }
        }
        if(IsCellOccupied(new Vector3Int(-1,1,0))) {
            if(mapController.mapDict[new Vector3Int(-1,1,0)].isOccupiedBy == "Boss") {
                return true;
            }
        }
        return false;
    }

    void Teleport() {
        if(currentMoveActions == totalMoveActions && CheckIfMyTurn()) {
            Vector3Int cellToTeleportTo =  FindFirstExistingAdjacentTile(friendController.targetCell);
            if(cellToTeleportTo == targetCell) {
                return;
            }
            UpdateOccupiedCell(cellToTeleportTo, targetCell);
            Vector3 targetPosition = groundTilemap.CellToWorld(cellToTeleportTo);
            targetCell = cellToTeleportTo;
            transform.position = targetPosition;
            turnController.NextTurn(CONTROLLER_NAME);
            if(willLose) {
                Lose();
            }
        }
    }

    Vector3Int FindFirstExistingAdjacentTile(Vector3Int cell) {
        if(mapController.mapDict.ContainsKey(cell + Vector3Int.down)) {
            return(cell + Vector3Int.down);
        }
        if(mapController.mapDict.ContainsKey(cell + Vector3Int.up)) {
            return(cell + Vector3Int.up);
        }
        if(mapController.mapDict.ContainsKey(cell + Vector3Int.left)) {
            return(cell + Vector3Int.left);
        }
        if(mapController.mapDict.ContainsKey(cell + Vector3Int.right)) {
            return(cell + Vector3Int.right);
        }
        return targetCell;
    }

    public void Lose() {
        Debug.Log("You Lost");
        SceneManager.LoadScene("Lose");
    }

    public void LoseAtEndOfTurn() {
        willLose = true;
    }

    public void Win() {
        Debug.Log("You Win!");
        SceneManager.LoadScene("WinScreen");
    }
}
