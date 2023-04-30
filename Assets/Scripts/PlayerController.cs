using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : BaseController
{
    
    private PlayerMovement controls;
    private BossController bossController;
    private FriendController friendController;

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
        if(CanMove(direction) && CheckIfMyTurn()) {
            Vector3Int prevCell = targetCell;
            targetCell += Vector3Int.RoundToInt(direction);
            UpdateOccupiedCell(targetCell, prevCell);
            Vector3 targetPosition = groundTilemap.CellToWorld(targetCell);
            transform.position = targetPosition;
            DecrementAndCheckCurrentMoveActions();
        }
    }

    void FinalBlow() {
        if(currentMoveActions == totalMoveActions) {
            if(IsAdjacentCellOccupiedByBoss()) {
                bossController.TakeDamage(CONTROLLER_NAME);
            }
        }
    }

    void Teleport() {
        if(currentMoveActions == totalMoveActions) {
            Vector3Int cellToTeleportTo =  FindFirstExistingAdjacentTile(friendController.targetCell);
            if(cellToTeleportTo == targetCell) {
                return;
            }
            UpdateOccupiedCell(cellToTeleportTo, targetCell);
            Vector3 targetPosition = groundTilemap.CellToWorld(cellToTeleportTo);
            targetCell = cellToTeleportTo;
            transform.position = targetPosition;
            turnController.NextTurn(CONTROLLER_NAME);
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
    }

    public void Win() {
        Debug.Log("You Win!");
    }
}
