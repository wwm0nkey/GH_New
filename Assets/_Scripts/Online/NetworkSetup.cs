using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class NetworkSetup : NetworkBehaviour {

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
        if (isLocalPlayer) {
            if(scene.name != "LobbyTest"){
            this.GetComponentInChildren<Camera>().enabled = true;
            this.GetComponentInChildren<AudioListener>().enabled = true;
            //this.GetComponentInChildren<MeshRenderer>().enabled = true;
            this.GetComponent<OnlineGravityBody>().enabled = true;
            this.GetComponent<OnlineHook>().enabled = true;
            this.GetComponent<CapsuleCollider>().enabled = true;
            this.GetComponent<OnlinePlayerController>().enabled = true;
            StartCoroutine(SpawnSetup());
            }
        }
        }    

        void OnEnable()
        {
            SceneManager.sceneLoaded += OnLevelFinisedLoading;
        }

        IEnumerator SpawnSetup(){
            var spawnList = new List<GameObject>();
            spawnList.AddRange(GameObject.FindGameObjectsWithTag("Spawner"));
            int randomNum = Random.Range(0, spawnList.Count);
            yield return new WaitForSeconds(2f);
            this.gameObject.transform.position = spawnList[randomNum].transform.position;
            this.GetComponent<Rigidbody>().useGravity = true;
        }
}
