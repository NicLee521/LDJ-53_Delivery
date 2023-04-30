using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;
using UnityEngine.Events;

public class FriendController : BaseController
{
    private PlayerController playerController;
    [SerializeField]
    private List<FriendAction> actions;
    private int currentAction = 0;

    void Awake() {
        CONTROLLER_NAME = "Friend";
    }    
    void Start()
    {
        SharedSetUp();
        playerController = FindObjectOfType<PlayerController>();
        turnController.turnOrderUpdated.AddListener(TakeTurn);
    }

    void TakeTurn() {
        StartCoroutine(WaitForTurn());
    }

    private IEnumerator WaitForTurn() {
        if(CheckIfMyTurn()) {
            yield return new WaitForSeconds(2);
            FriendAction action = actions[currentAction];
            switch(action.actionType) {
                case FriendAction.actionTypes.Move:
                    Move(GetVector2FromString(action.actionOrder));
                    break;
                default:
                    break;
            }
            currentAction++;
            turnController.NextTurn(CONTROLLER_NAME);
        }
    }

}
[System.Serializable]
public struct FriendAction {
    public enum actionTypes {
        Move
    }
    public actionTypes actionType;
    public string actionOrder;
}
