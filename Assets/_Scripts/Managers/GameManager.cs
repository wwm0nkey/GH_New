using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    // Use this for initialization
    public int Score;
    public Text ScoreText;
    public bool fpsLimit;
    public int FPS;
	void Start () {
        ScoreText.text = "Score: " + Score.ToString();
        if (fpsLimit) {
            Application.targetFrameRate = FPS;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}

   public void UpdateScore() {
        Score = Score + 1;
        ScoreText.text = "Score: " + Score.ToString();
    }
}
