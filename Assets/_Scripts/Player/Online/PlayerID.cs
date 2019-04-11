using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerID : NetworkBehaviour {

    [SyncVar]
    private string playerUniqueIdentity;
    private NetworkInstanceId playerNetID;
    private Transform myTransform;
    // Use this for initialization
    public override void OnStartLocalPlayer() {
        GetNetIdentity();
        SetIdentity();
    }

    void Awake() {
        myTransform = transform;
    }

    // Update is called once per frame
    void Update() {
        if (myTransform.name == "" || myTransform.name == "PlayerControllerOnlinePhy(Clone)") {
            SetIdentity();
        }
    }

    [Client]
    void GetNetIdentity() {
        playerNetID = GetComponent<NetworkIdentity>().netId;
        CmdTellServerMyIdentity(MakeUniqueIdentity());
    }


    void SetIdentity() {
        if (!isLocalPlayer) {
            myTransform.name = playerUniqueIdentity;
        } else {
            myTransform.name = MakeUniqueIdentity();
        }
    }

    string MakeUniqueIdentity() {
        string uniqueName = "Player " + playerNetID.ToString();
        return uniqueName;
    }

    [Command]
    void CmdTellServerMyIdentity(string name) {
        playerUniqueIdentity = name;
    }

}
