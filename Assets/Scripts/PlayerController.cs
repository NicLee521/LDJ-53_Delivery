using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : BaseController
{
    
    private PlayerMovement controls;

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
        controls.Main.Movement.performed += context => Move(context.ReadValue<Vector2>());
    }

    public void Lose() {
        Debug.Log("You Lost");
    }

    public void Win() {
        Debug.Log("You Win!");
    }
}
