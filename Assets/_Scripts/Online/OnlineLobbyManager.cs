using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;


public class OnlineLobbyManager : MonoBehaviour {

    public GameObject startPannel;
    public GameObject lobbyPannel;
    public Button startGameBtn;
	// Use this for initialization
	void Awake () {
        startPannel.SetActive(true);
        lobbyPannel.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnHostMatch() {
        startPannel.SetActive(false);
        lobbyPannel.SetActive(true);
    }

    public void OnCancelMatch() {
        lobbyPannel.SetActive(false);
        startPannel.SetActive(true);
    }
}
