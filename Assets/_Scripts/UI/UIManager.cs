using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {

    public GameObject ExitMenu;
    public List<GameObject> TextToDisable;
    public RayCastDebug PlayerShoot;
    public UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController PlayerBody;
    public bool isHint;
	// Use this for initialization
	void Start () {
        isHint = true;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            ExitMenu.SetActive(true);
            PlayerShoot.enabled = false;
            Time.timeScale = 0.0001f;
            //PlayerBody.enabled = false;
        }

        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (isHint) {
                foreach(var obj in TextToDisable) {
                    obj.SetActive(false);
                    isHint = false;
                }
            } else {
                foreach(var obj in TextToDisable) {
                    obj.SetActive(true);
                    isHint = true;
                }
            }
        }
	}

    public void ExitGame() {
        Application.Quit();
    }

    public void NoExit() {
        ExitMenu.SetActive(false);
        PlayerShoot.enabled = true;
        Time.timeScale = 1;
        //PlayerBody.enabled = true;

    }
}
