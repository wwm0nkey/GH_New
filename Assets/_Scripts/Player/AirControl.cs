using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class AirControl : MonoBehaviour {
    public Vector3 move;
    public Transform ca;
    public Vector3 camForward;
    public int movePower;
    public bool airMovement;
	// Use this for initialization
	void Start () {
		
	}
    private void Awake() {
        ca = Camera.main.transform;
    }

    // Update is called once per frame
    void Update () {
            float h = CrossPlatformInputManager.GetAxis("Horizontal");
            float v = CrossPlatformInputManager.GetAxis("Vertical");
            camForward = Vector3.Scale(ca.forward, new Vector3(1, 0, 1)).normalized;
            move = (v * camForward + h * ca.right).normalized;
        if(airMovement) {
            GetComponent<Rigidbody>().AddForce(move * movePower);
        }
        if (Input.GetButtonDown("Fire1")) {
            airMovement = true;
        }else if (Input.GetButtonUp("Fire1")){
            airMovement = false;
        }
    }
}
