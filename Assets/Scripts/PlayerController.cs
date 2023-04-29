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
    private Vector3Int targetCell;
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
        controls.Main.Movement.performed += context => Move(context.ReadValue<Vector2>());
        targetCell = groundTilemap.WorldToCell(transform.position);
        transform.position = groundTilemap.CellToWorld(targetCell);
    }

    void Move(Vector2 direction) {
        targetCell += Vector3Int.RoundToInt(direction);
        Vector3 targetPosition = groundTilemap.CellToWorld(targetCell);
        transform.position = targetPosition;
    }

    bool CanMove(Vector2 direction) {
        Vector3Int tempTarget = targetCell;
        Vector3 targetPosition = GetTargetPosition(direction, tempTarget);
        Vector3Int gridPosition = groundTilemap.WorldToCell(targetPosition);
        if(!groundTilemap.HasTile(gridPosition) || colisionTilemap.HasTile(gridPosition)) {
            return false;
        }
        return true;
    }

    Vector3 GetTargetPosition(Vector2 direction, Vector3Int target) {
        target += Vector3Int.RoundToInt(direction);
        return groundTilemap.CellToWorld(target);
    }
}
