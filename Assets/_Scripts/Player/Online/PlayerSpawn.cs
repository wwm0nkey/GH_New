using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawn : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	        void Awake()
            {
                DontDestroyOnLoad(this.gameObject);
            }

        void OnLevelFinisedLoading(Scene scene, LoadSceneMode mode){
            //this.gameObject.transform.position = GameObject.FindGameObjectWithTag("Spawner").transform.position; 
			//this.gameObject.GetComponent<PlayerSpawn>().enabled = false;
        }    

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnLevelFinisedLoading;
        }
}
