using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendController : BaseController
{
    private PlayerController playerController;
    private BossController bossController;
    [SerializeField]
    private List<FriendAction> actions;
    private int currentAction = 0;
    public bool useAi;

    void Awake() {
        CONTROLLER_NAME = "Friend";
    }    
    void Start()
    {
        SharedSetUp();
        playerController = FindObjectOfType<PlayerController>();
        bossController = FindObjectOfType<BossController>();
        turnController.turnOrderUpdated.AddListener(TakeTurn);
    }

    void TakeTurn() {
        if(useAi) {
            StartCoroutine(WaitForTurnAI());
            return;
        }
        StartCoroutine(WaitForTurn());
        
    }

    private IEnumerator WaitForTurn() {
        if(CheckIfMyTurn()) {
            yield return new WaitForSeconds(THINKING_TIME);
            FriendAction action = actions[currentAction];
            switch(action.actionType) {
                case FriendAction.actionTypes.Move:
                    Move(GetVectorDirectionFromString(action.actionOrder));
                    break;
                case FriendAction.actionTypes.Attack:
                    AttackBoss();
                    break;
                default:
                    break;
            }
            currentAction++;
            turnController.NextTurn(CONTROLLER_NAME);
        }
    }

    private IEnumerator WaitForTurnAI() {
        if(CheckIfMyTurn()) {
            yield return new WaitForSeconds(THINKING_TIME);
            if(IsAdjacentCellOccupiedByBoss()) {
                bossController.TakeDamage(CONTROLLER_NAME);
                turnController.NextTurn(CONTROLLER_NAME);
            } else {
                Vector2 direction = Vector2Int.RoundToInt(((Vector3)bossController.targetCell - (Vector3)targetCell).normalized);
                
                while(CheckIfMyTurn()) {
                    yield return new WaitForSeconds(.5f);
                    Move(direction);
                }
            }
        }
        yield break;
    }

    void AttackBoss() {
        if(IsAdjacentCellOccupiedByBoss()) {
            bossController.TakeDamage(CONTROLLER_NAME);
            return;
        }
        Debug.Log("Cannot Attack");
    }

    override protected void Move(Vector2 direction) {
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

    void DecrementAndCheckCurrentMoveActions() {
        currentMoveActions--;
        if(currentMoveActions <= 0) {
            turnController.NextTurn(CONTROLLER_NAME);
            currentMoveActions = totalMoveActions;
        }
    }

}
[System.Serializable]
public struct FriendAction {
    public enum actionTypes {
        Move,
        Attack
    }
    public actionTypes actionType;
    public string actionOrder;
}
