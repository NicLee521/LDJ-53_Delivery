using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Tilemap groundTilemap;
    [SerializeField]
    private Tilemap colisionTilemap;
    private PlayerMovement controls;
    public Vector3Int targetCell;
    private TurnController turnController;
    const string CONTROLLER_NAME = "Player";

    void Awake() {
        controls = new PlayerMovement();
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
        turnController = FindObjectOfType<TurnController>();
        controls.Main.Movement.performed += context => Move(context.ReadValue<Vector2>());
        targetCell = groundTilemap.WorldToCell(transform.position);
        transform.position = groundTilemap.CellToWorld(targetCell);
    }

    void Move(Vector2 direction) {
        if(CanMove(direction)) {
            targetCell += Vector3Int.RoundToInt(direction);
            Vector3 targetPosition = groundTilemap.CellToWorld(targetCell);
            transform.position = targetPosition;
            turnController.NextTurn(CONTROLLER_NAME);
        }
    }

    bool CanMove(Vector2 direction) {
        Vector3Int tempTarget = targetCell;
        tempTarget += Vector3Int.RoundToInt(direction);
        Vector3 targetPosition = groundTilemap.CellToWorld(tempTarget);
        Vector3Int gridPosition = groundTilemap.WorldToCell(targetPosition);
        if(!groundTilemap.HasTile(gridPosition) || colisionTilemap.HasTile(gridPosition) || !CheckIfMyTurn()) {
            return false;
        }
        return true;
    }

    bool CheckIfMyTurn() {
        return turnController.IsMyTurn(CONTROLLER_NAME);
    }

    public void Lose() {
        Debug.Log("You Lost");
    }
}
