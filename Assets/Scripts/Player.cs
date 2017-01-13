using UnityEngine;
using System.Collections;

public abstract class Player : MonoBehaviour {

    public string playerName;
    public int playerNumber;
    public bool isHuman;
    public Color color;

    public int score;

	// Use this for initialization
	void Awake () { }
	

    void updateScore(int fish)
    {
        score += fish;
    }

}
