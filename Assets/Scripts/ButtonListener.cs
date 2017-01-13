using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ButtonListener : MonoBehaviour 
{
    public GameObject[] PlayerPanels;

    public Transform StartButton;
    private Vector3 startButtonPos;
    private float startButtonOffset = 70.0f;

    private int numPlayers;

    void Start()
    {
        startButtonPos = StartButton.localPosition;
    }

    public void MyClick (GameObject obj) 
	{
        if (obj.CompareTag("NumPlayersButton"))
        {
            numPlayers = Int32.Parse(obj.GetComponentInChildren<Text>().text);
            PlayerPanels[0].SetActive(true);
            switch (numPlayers)
            {
                case 4:
                    PlayerPanels[1].SetActive(true);
                    PlayerPanels[2].SetActive(true);
                    PlayerPanels[3].SetActive(true);
                    StartButton.localPosition = new Vector3(startButtonPos.x, PlayerPanels[3].transform.localPosition.y - startButtonOffset, startButtonPos.z);
                    break;
                case 3:
                    PlayerPanels[1].SetActive(true);
                    PlayerPanels[2].SetActive(true);
                    PlayerPanels[3].SetActive(false);
                    StartButton.localPosition = new Vector3(startButtonPos.x, PlayerPanels[2].transform.localPosition.y - startButtonOffset, startButtonPos.z);
                    break;
                case 2:
                    PlayerPanels[1].SetActive(true);
                    PlayerPanels[2].SetActive(false);
                    PlayerPanels[3].SetActive(false);
                    StartButton.localPosition = new Vector3(startButtonPos.x, PlayerPanels[1].transform.localPosition.y - startButtonOffset, startButtonPos.z);
                    break;
            }
            StartButton.gameObject.SetActive(true);
        }

        if (obj.CompareTag("StartGameButton"))
        {
            GameManager.Instance.SetNumPlayers(numPlayers);
            GameManager.Instance.StartGame();
        }

        // Find input field
        InputField[] ins = GameObject.FindObjectsOfType<InputField>();
		foreach (InputField i in ins)
		{
			Debug.Log ("in: " + i.name);
			if (i.isFocused)
			{//i.Select();   // I also tried to use this EventSystem.current.SetSelectedGameObject(go);
				i.ActivateInputField();
				i.Select();
				i.MoveTextEnd(false);
				i.ProcessEvent(Event.KeyboardEvent("b"));
//				inputField.ProcessEvent(Event.KeyboardEvent("a"));
//				i.text += "a";
//				i.textComponent.text += "a";
			}
		}
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.A))
		{
			GameObject go = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
			if (go != null)
			{
				InputField i = go.GetComponent<InputField>();
				if (i != null)
				{
					i.ProcessEvent(Event.KeyboardEvent("l"));
				}
			}
		}
	}
}
