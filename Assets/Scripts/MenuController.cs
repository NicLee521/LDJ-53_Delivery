using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;


public class MenuController : MonoBehaviour
{

    void Start() {
        Scene scene = SceneManager.GetActiveScene();
        /* if(scene.name == "Main Menu") {
            GameObject audio = Resources.Load<GameObject>("Audio Source");
            GameObject newAud = Instantiate(audio);
            newAud.SetActive(true);
            DontDestroyOnLoad(newAud);
            GameObject[] objs = GameObject.FindGameObjectsWithTag("Audio");
            Debug.Log("Made source" + objs.Length);
            if(objs.Length > 1) {
                Debug.Log("delete source");
                Destroy(newAud);
                return;
            }
            return;
        } */
    }
    public void GetMainMenu() {
        SceneManager.LoadScene("Main Menu", LoadSceneMode.Single);   
    }

    public void GetBackstory() {
        SceneManager.LoadScene("Backstory", LoadSceneMode.Single);   
    }

    public void Quit() {
        Application.Quit();
    }
}
