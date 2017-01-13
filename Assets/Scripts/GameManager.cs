using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public GameObject[] PlayerPanels;

    public Board Board;

    public Transform PlayersParent;
    public List<Player> Players { get; private set; }

    public Transform PenguinsParent;
    public List<Penguin> Penguins { get; private set; }

    public int numPlayers { get; private set; }

    private int penguinsToPlace = 0;

    private static GameManager _instance = null;
    public static GameManager Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
            _instance = this;

        DontDestroyOnLoad(this.gameObject);
        gameObject.name = "$GameManager";

    }

    public void StartGame()
    {
        Debug.Log("Made it to GameManager.StartGame()");

        if (Board.gameOver)
            resetGame();

        Players = getPlayerInput();

        setUpPenguins();

        GazeInputModuleCrosshair.DisplayCrosshair = false;
        Board.StartGame(Players, Penguins);
    }

    public void SetNumPlayers(int numPlayers)
    {
        this.numPlayers = numPlayers;
    }

    private void resetGame()
    {
        foreach (Penguin p in Penguins)
            p.Reset();  

        foreach(Tile t in Board.Tiles)
            t.Reset();

        Board.allIslandsOwned = false;
        Board.gameOver = false;
    }

    private void setUpPenguins()
    {

        if (numPlayers == 2 || numPlayers == 4)
            penguinsToPlace = 8;
        else if (numPlayers == 3)
            penguinsToPlace = 9;

        Penguins = new List<Penguin>();
        for (int i = 0; i < penguinsToPlace; i++)
        {
            var penguin = PenguinsParent.GetChild(i).GetComponent<Penguin>();
            if (penguin != null)
                Penguins.Add(penguin);
            else
                Debug.LogError("Invalid object in Penguins Parent game object");
        }

        assignPlayerAndActivate(numPlayers, penguinsToPlace);

    }

    private void assignPlayerAndActivate(int numPlayers, int penguinsToPlace)
    {
        int playerIndex = 0;
        for(int i = 0; i < penguinsToPlace; i++)
        {
            Penguins[i].player = Players[playerIndex];
            Penguins[i].SetColor(Players[playerIndex].color);
            Penguins[i].gameObject.SetActive(true);
            if ((i + 1) % (penguinsToPlace / numPlayers) == 0)
                playerIndex++;
        }
    }

    private List<Player> getPlayerInput()
    {
        List<Player> players = new List<Player>();
        Debug.Log("numPlayers: " + numPlayers);
        for (int i = 0; i < numPlayers; i++)
        {
            var playerObject = new GameObject();
            playerObject.transform.SetParent(PlayersParent);

            Player newPlayer;
            if (PlayerPanels[i].GetComponentInChildren<Toggle>().isOn)
                newPlayer = playerObject.AddComponent<HumanPlayer>();
            else
                newPlayer = playerObject.AddComponent<AIPlayer>();


            playerObject.name = newPlayer.playerName = PlayerPanels[i].transform.GetComponentInChildren<Text>().text;

            newPlayer.playerNumber = i + 1;

            newPlayer.color = PlayerPanels[i].transform.FindChild("Color").GetComponentInChildren<Image>().color;

            players.Add(newPlayer);
        }
        Debug.Log("players.count: " + players.Count);
        return players;
    }

    public void ResetPenguinPositions()
    {
        foreach(Penguin p in Penguins)
            p.ResetPosition();
    }
}
