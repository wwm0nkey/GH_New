using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour {
    public double health;
	// Use this for initialization
	void Start () {
        health = 50;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void TakeDamage() {
        health -= 5;
        if(health <= 0) {
            Destroy(this.gameObject);
        }
    }
}
