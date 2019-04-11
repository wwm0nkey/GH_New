using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collection : MonoBehaviour {
    // Use this for initialization
    public GameManager _gm;
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other) {
            _gm.UpdateScore();
            Destroy(this.gameObject);
    }

    //private void OnCollisionEnter(Collision collision) {
    //    if (tag == "Player") {
    //        _gm.UpdateScore();
    //        Destroy(this.gameObject);
    //    }
    //}
}
