using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;




public class TurnController : MonoBehaviour
{
    [SerializeField]
    public string[] turnOrder;
    private Dictionary<string, bool> turnTracker = new Dictionary<string, bool>();
    [NonSerialized]
    public UnityEvent turnOrderUpdated;

    void Awake() {
        for(int i = 0; i < turnOrder.Length; i ++) {
            if(i == 0) {
                turnTracker.Add(turnOrder[i], true);
                continue;
            }
            turnTracker.Add(turnOrder[i], false);
        }
        if (turnOrderUpdated == null) {
            turnOrderUpdated = new UnityEvent();
        }
    }

    public bool IsMyTurn(string controller) {
        return turnTracker[controller];
    }

    public void NextTurn(string controller) {
        turnTracker[controller] = false;
        int index = Array.IndexOf(turnOrder, controller);
        index++;
        if(index > turnOrder.Length - 1) {
            index = 0;
        }
        turnTracker[turnOrder[index]] = true;
        turnOrderUpdated.Invoke();
    }
    
}
