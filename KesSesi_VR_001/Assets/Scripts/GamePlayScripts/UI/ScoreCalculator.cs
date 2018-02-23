using UnityEngine;
using UnityEngine.UI;

public class ScoreCalculator : MonoBehaviour {

    public static int score = 0;
    public TextMesh displayText;
	
	void Update () {
        displayText.text = score.ToString("000");
	}
}
