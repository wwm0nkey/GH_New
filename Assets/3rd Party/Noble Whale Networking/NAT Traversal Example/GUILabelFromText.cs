using UnityEngine;

public class GUILabelFromText : MonoBehaviour
{

    public TextAsset textFile;
    public Vector2 position;
    string text;
    
	void Start ()
    {
        text = textFile.text;
	}
	
	void OnGUI()
    {
        GUI.Label(new Rect(position.x, position.y, 900, 900), text);
	}
}
