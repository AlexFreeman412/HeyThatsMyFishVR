using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Diagnostics;

public class Board : MonoBehaviour
{

    public AudioManager AudioManager;

    public GUIStyle buttonStyle;

    public Text scoreText;

    public const int ONE_FISH_TILES = 30;
    public const int TWO_FISH_TILES = 20;
    public const int THREE_FISH_TILES = 10;
    public const int TOTAL_TILES = 60;

    public int currentPlayer;
    public int numPlayers;

    public bool menuOpen;
    public bool setupPhase;
    public bool allIslandsOwned;
    public bool gameOver;

    public List<Player> Players { get; private set; }

    public Transform PlayersParent;

    public List<Tile> Tiles { get; private set; }

    public List<Penguin> Penguins { get; private set; }

    public List<Island> Islands { get; private set; }

    public List<Pointer> Pointers { get; private set; }

    public Transform PenguinsParent;

    public const int BOARD_SIZE = 8;

    public int penguinsToPlace = 8;

    public bool finishGameHappened = false;

    public void StartGame(List<Player> players, List<Penguin> penguins)
    {
        setupPhase = true;
        Players = players;
        Penguins = penguins;
        penguinsToPlace = Penguins.Count;
        numPlayers = Players.Count;

        Physics.gravity = new Vector3(0, -20.0F, 0);


        Tiles = GenerateBoard();

        currentPlayer = 1;

        updateScoreboard();

        //menuOpen = true;

        Pointers = new List<Pointer>();

        if (!getCurrentPlayer().isHuman)
            (getCurrentPlayer() as AIPlayer).StartPenguin();

        //StartCoroutine(delayStart());
    }

    public void OnConnectController(GameObject controller)
    {
        Pointer newPointer = controller.GetComponentInChildren<Pointer>(true);

        if (newPointer == null)
            return;
        if (Pointers == null)
            Pointers = new List<Pointer>();

        Pointers.Add(newPointer);
        newPointer.PenguinSelected += OnPenguinSelected;
        newPointer.PenguinReleased += OnPenguinReleased;
    }

    public void OnDisconnectController(GameObject controller)
    {
        Pointer removedPointer = controller.GetComponentInChildren<Pointer>(true);

        if (removedPointer == null)
            return;

        Pointers.Remove(removedPointer);
        removedPointer.PenguinSelected -= OnPenguinSelected;
        removedPointer.PenguinReleased -= OnPenguinReleased;
    }

    private void OnPenguinSelected(object sender, Pointer.PenguinSelectArgs args)
    {
        AudioManager.PlayPenguinSound();
        Penguin penguin = args.selectedPenguin.GetComponent<Penguin>();

        if (penguin.Tile != null)
            penguin.Tile.MarkAsCurrent();

            if (penguin.playerNumber() == currentPlayer)
        {
            
            if (setupPhase)
            {
                if (penguin.Tile != null)
                    return;
                penguin.selected = true;
                
                foreach (Tile tile in Tiles)
                    if (tile.numFish == 1)
                        tile.MarkAsValidMove();
            }
            else
            {
                penguin.selected = true;

                foreach (Tile tile in Tiles)
                if (penguin.ValidMoves.Contains(tile))
                    tile.MarkAsValidMove();
            }
        }
    }

    private void OnPenguinReleased(object sender, Pointer.PenguinSelectArgs args)
    {
        Penguin movedPenguin = args.selectedPenguin.GetComponent<Penguin>();

        foreach (Tile tile in Tiles)
            tile.Unmark();

        if (!movedPenguin.selected)
            return;

        movedPenguin.selected = false;

        Tile destinationTile = movedPenguin.OverTile;

        if (destinationTile == null || destinationTile.IsRemoved || destinationTile.penguin != null)
            return;//TODO: MOVE PENGUIN BACK AND DISPLAY INVALID MOVE ERROR

        if (setupPhase)
        {
            if (destinationTile.numFish != 1)
            {              
                return; //TODO: MOVE PENGUIN BACK AND DISPLAY INVALID MOVE ERROR
            }
            startPenguin(movedPenguin, destinationTile);
        }
        else
        {
            if (MovePenguin(movedPenguin, destinationTile))
                PrepareNextTurn();
            else
            {
                print("Bad move");//TODO: MOVE PENGUIN BACK AND DISPLAY INVALID MOVE ERROR
            }
        }
    }

    private void OnPenguinClicked(object sender, EventArgs e)
    {
        if (setupPhase)
            return;

        Penguin penguin = sender as Penguin;

        foreach (Penguin p in Penguins)
            if (!(p == penguin))
                p.selected = false;

        if (penguin.playerNumber() == currentPlayer)
            penguin.selected = !penguin.selected;
    }

    public void startPenguin(Penguin penguin, Tile start)
    {
        penguin.player.score += start.numFish;
        penguin.StartAt(start);
        nextPlayersTurn();
        updateScoreboard();

        if (--penguinsToPlace == 0)
        {
            setupPhase = false;
            PrepareNextTurn();
            return;
        }

        if (!getCurrentPlayer().isHuman)
            (getCurrentPlayer() as AIPlayer).StartPenguin();
    }

    public Player getCurrentPlayer()
    {
        return Players.Find(p => p.playerNumber == currentPlayer);
    }

    public bool MovePenguin(Penguin penguin, Tile target)
    {
        if ( penguin == null || !penguin.ValidMoves.Contains(target) )
            return false;

        penguin.player.score += target.numFish;
        penguin.Move(target);
        return true;
    }

    public IEnumerator MoveWhenReady(Move moveToMake)
    {
        while (IsPenguinMoving())
            yield return 0;
        /*
        foreach (Penguin p in Penguins)
            p.selected = false;

        moveToMake.penguin.selected = true;
        */
        MovePenguin(moveToMake.penguin, moveToMake.to);
        moveToMake.penguin.UpdateValidMoves();
    }

    public IEnumerator NextTurnWhenReady()
    {
        while (IsPenguinMoving())
            yield return 0;

        PrepareNextTurn();

    }

    public void PrepareNextTurn()
    {
        foreach (Penguin p in Penguins)
        {
            p.UpdateValidMoves();
            if (p.ValidMoves.Count == 0)
                p.isStuck = true;
        }

        

        if (isGameOver() || finishGameHappened)
        {
            finishGameHappened = false;
            GazeInputModuleCrosshair.DisplayCrosshair = true;
            updateScoreboard();
            print("GAME OVER!");
            return;
        }
        else
            nextPlayersTurn();

        updateScoreboard();

        if (Islands != null)
            Islands.Clear();

        Islands = GetAllIslands(Tiles);

        //AddPenguinIslandInfo(Islands);

        /*
        foreach( Island island in Islands)
        {
            String msg = "";
            msg += "There are " + island.Tiles.Count + " tiles in this island." + Environment.NewLine;
            foreach (Tile tile in island.Tiles)
                msg += tile + " ";
            msg += Environment.NewLine + "Inhabited: " + island.isInhabited + " - Owned: " + island.isOwned + " - Single: " + island.hasSinglePenguin + " - Finished: " + island.isFinished + Environment.NewLine;
            foreach (Tuple<Tile, List<Island>> bridge in island.Bridges)
            {
                String fuck = "";
                foreach (Island i in bridge.Second)
                    fuck += " " + i.Tiles.Count;
                msg += "Bridge: " + bridge.First + " " + fuck;
            }
            print(msg);
            
        }
        

        foreach (Penguin penguin in Penguins)
        {
            print("The penguin at " + penguin.Tile + " has " + penguin.pointsAvailable + " points available.");
            if (penguin.sittingOn > 0)
                print("The penguin at " + penguin.Tile + " is sitting on: " + penguin.sittingOn + " points.");
        }
        
         */
        if (AreAllIslandsOwned())
        {
            print("ALL ISLANDS OWNED");
            FinishGame();
            return;
        }  
       
        if (!getCurrentPlayer().isHuman)
            (getCurrentPlayer() as AIPlayer).play();

    }

    private void updateScoreboard()
    {
        scoreText.text = "";
        if (isGameOver())
        {
            List<string> winners = Players.Where(a => a.score == Players.Select(p => p.score).Max()).Select( b=> b.playerName).ToList();

            if(winners.Count == 1)
                scoreText.text += "Game Over!\n" + winners[0] + " wins!\n";
            else
            {
                string winnersNames = "";
                for(int i = 0; i < winners.Count; i++)
                {
                    if (i == winners.Count - 1)
                        winnersNames += winners[i];
                    else if (i == winners.Count)
                        winnersNames += winners[i] + " and ";
                    else
                        winnersNames += winners[i] + ", ";
                }
                scoreText.text += "Game Over!\n" + winnersNames + " drew!\n\n";
            }
        }
        for(int i = 0; i < numPlayers; i++)
        {
            scoreText.text += Players[i].playerName + "'s score: " + Players[i].score + "\n";
            //scoreText.color = Players[i].color;
        }

        if (!isGameOver())
            scoreText.text += "It is " + getCurrentPlayer().playerName + "'s turn.\n";

    }

    public bool IsPenguinMoving()
    {
        foreach (Penguin penguin in Penguins)
            if (penguin.isMoving)
                return true;
        return false;
    }

    public Penguin getSelectedPenguin()
    {
        foreach (Penguin penguin in Penguins)
            if (penguin.selected == true)
                return penguin;
        return null;
    }

    public bool isGameOver()
    {
        if (setupPhase)
            return false;

        gameOver = true;
        foreach (Penguin penguin in Penguins)
            gameOver = gameOver && penguin.isStuck;
        return gameOver;
    }

    public void nextPlayersTurn()
    {
        if (currentPlayer != numPlayers)
            currentPlayer++;
        else
            currentPlayer = 1;

        if (setupPhase)
            return;
        if (areAllPenguinsStuck())
            nextPlayersTurn();
    }

    private bool areAllPenguinsStuck()//Of current player
    {
        foreach (Penguin p in Penguins)
        {
            if (p.Tile != null)
                if (p.playerNumber() == currentPlayer &&
                    !p.isStuck)
                    return false;
        }
        return true;
    }

    public Tile getTileAtCoord(int x, int y)
    {
        foreach (Tile tile in Tiles)
        {
            if (tile == null)
                return null;
            if (tile.coord.Equals(new Point(x, y)))
                return tile;
        }
        return null;
    }

    public List<Tile> GenerateBoard()
    {

        int x = 1;
        int y = 1;
        int sevenRow = 0;

        System.Random rnd = new System.Random();

        int oneTiles = 0;
        int twoTiles = 0;
        int threeTiles = 0;

        int tileType = 0;

        List<Tile> ret = new List<Tile>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var tile = transform.GetChild(i).gameObject.GetComponent<Tile>();
            if (tile != null)
            {
                tile.coord = new Point(x, y);
                if (x < BOARD_SIZE - sevenRow)
                    x++;
                else
                {
                    sevenRow = (sevenRow == 0) ? 1 : 0;
                    x = 1;
                    y++;
                }

                //tile = generateTile(oneTiles, twoTiles, threeTiles);

                /* TODO: Make it so the next tile is dependent on tiles left */
                /* TODO: Find a way to get this code working in methods */

                int chance = rnd.Next(1, 7);

                switch (chance)
                {
                    case 1:
                    case 2:
                    case 3:
                        tileType = 1;
                        break;
                    case 4:
                    case 5:
                        tileType = 2;
                        break;
                    case 6:
                        tileType = 3;
                        break;
                    default:
                        break;
                }

                /* TODO  Clean up code for avoiding used up tiles*/
                if (oneTiles == ONE_FISH_TILES && twoTiles == TWO_FISH_TILES)
                    tileType = 3;
                else if (oneTiles == ONE_FISH_TILES && threeTiles == THREE_FISH_TILES)
                    tileType = 2;
                else if (threeTiles == THREE_FISH_TILES && twoTiles == TWO_FISH_TILES)
                    tileType = 1;
                else if (oneTiles == ONE_FISH_TILES)
                    while (tileType == 1)
                        tileType = rnd.Next(1, 4);
                else if (twoTiles == TWO_FISH_TILES)
                    while (tileType == 2)
                        tileType = rnd.Next(1, 4);
                else if (threeTiles == THREE_FISH_TILES)
                    while (tileType == 3)
                        tileType = rnd.Next(1, 4);

                oneTiles = (tileType == 1) ? ++oneTiles : oneTiles;
                twoTiles = (tileType == 2) ? ++twoTiles : twoTiles;
                threeTiles = (tileType == 3) ? ++threeTiles : threeTiles;

                tile.numFish = tileType;
                tile.updateImage();

                ret.Add(tile);
            }

            else
                UnityEngine.Debug.LogError("Invalid object in board game object");
        }

        return ret;
    }

    public Island GetIsland(Tile tile)
    {
        List<Tile> islandTiles = new List<Tile>();
        List<Tile> edge = new List<Tile>();

        if (tile.IsRemoved)
            return null;

        islandTiles.Add(tile);

        if(tile.penguin == null)
            edge = tile.getNeighbours();
        else
            edge = tile.getFreeNeighbours();

        while (edge.Count > 0)
        {
            islandTiles.AddRange(edge);
            List<Tile> newEdge = new List<Tile>();
            foreach (Tile t in edge)
                if (t.penguin == null)
                    newEdge.AddRange(t.getNeighbours().Where(e => !islandTiles.Contains(e) && !newEdge.Contains(e)).ToList());
            edge = newEdge;
        }

        return new Island(islandTiles);

    }

    public List<Island> GetAllIslands(List<Tile> range)
    {
        List<Island> islands = new List<Island>();
        List<Tile> tilesLeft = (new List<Tile>(range)).Where(e => !e.IsRemoved).ToList();
        while (tilesLeft.Count > 0)
        {
            Island island = GetIsland(tilesLeft[0]);
            islands.Add(island);
            tilesLeft.RemoveAll(e => islands.SelectMany(a => a.Tiles).Contains(e));
        }

        

        List<Tile> bridgeTiles = islands.SelectMany(i => i.Tiles).GroupBy(t => t).Where(a => a.Count() > 1).Select(x => x.Key).ToList();

        foreach(Penguin p in Penguins)
            p.onBridge = false;

        foreach (Tile tile in bridgeTiles)
        {
            List<Island> bridgeIslands = islands.Where(i => i.Tiles.Contains(tile)).ToList();
            foreach (Island island in bridgeIslands)
                island.AddBridge(tile, bridgeIslands.Where(i => !i.Equals(island) ).ToList());
            tile.penguin.onBridge = true;            
        }               
        
        
        return islands;
    }

    public void AddPenguinIslandInfo(List<Island> islands)
    {
        if (islands == null)
            return;

        foreach (Penguin p in Penguins)
        {
            p.pointsAvailable = 0;
            p.sittingOn = 0;
            p.singleOwner = false;

            p.islands = islands.Where(i => i.Penguins.Contains(p)).ToList();

            if (p.islands.Count > 1)
                p.onBridge = true;
            else
                p.onBridge = false;
        }

        foreach (Island island in islands)
        {
            
            if(island.isInhabited && !island.isOwned && !island.isFinished)
                foreach (Penguin p in island.Penguins)
                    if (island.totalPoints > p.pointsAvailable)
                        p.pointsAvailable = island.totalPoints;
                        
            if (island.hasSinglePenguin)
            {
                Penguin penguin = island.Penguins.First();
                penguin.singleOwner = true;
                int points = GetBestPath(GetAllPaths(island, penguin)).Second;
                if (points > penguin.sittingOn)
                    penguin.sittingOn = points;
            }

            if (island.isOwned)
                pointsForOwnedIsland(island);
        }
    }

    private void pointsForOwnedIsland(Island island)
    {
        foreach (Penguin p in island.Penguins)
        {
            if (!p.onBridge)
            {
                int points = GetBestPath(GetAllPaths(island, p)).Second;
                if (points > p.sittingOn)
                    p.sittingOn = points;
                return;
            }
        }
        Penguin penguin = island.Penguins.Where(e => e.sittingOn == island.Penguins.Select(a => a.sittingOn).Min()).Count() == 1 ?
            island.Penguins.Where(e => e.sittingOn == island.Penguins.Select(a => a.sittingOn).Min()).First() :
            island.Penguins.Where(e => e.pointsAvailable == island.Penguins.Select(a => a.pointsAvailable).Min()).First();

    }

    public bool AreAllIslandsOwned()
    {
        allIslandsOwned = true;

        foreach (Island island in Islands)
        {
            allIslandsOwned = allIslandsOwned && (island.isOwned ||
                    !island.isInhabited ||
                    island.isFinished);
        }

        return allIslandsOwned;
    }

    public void FinishGame()
    {
        List<Island> combinedIslands = getCombinedIslands(Islands);
        foreach (Island island in combinedIslands)
        {/*
            if (!island.isFinished)
            {
                FinishIsland(island, island.Penguins);
            }
            //finishedgame = true;
             */
            if (island.hasSinglePenguin && !island.isFinished)
                FinishSinglePenguinIsland(island);
            else if (island.isOwned && !island.isFinished)
                FinishMultiplePenguinIsland(island);   
               
        }
        finishGameHappened = true;
        StartCoroutine(NextTurnWhenReady());

    }

    private List<Island> getCombinedIslands(List<Island> islands)
    {
        List<Island> islandsLeft = new List<Island>(islands);
        List<Island> combinedIslands = new List<Island>();

        foreach (Island island in islands)
        {
            if (!islandsLeft.Contains(island))
                continue;

            if (island.Bridges.Count == 0)
            {               
                combinedIslands.Add(island);
                islandsLeft.Remove(island);
            }
            else
            {
                List<Island> islandChain = getIslandsInChain(island);
                foreach (Island i in islandChain)
                    islandsLeft.Remove(i);
                combinedIslands.Add(combineIslands(islandChain));
            }
        }

        return combinedIslands;
    }

    private List<Island> getIslandsInChain(Island island)
    {
        List<Island> islandChain = new List<Island>() { island };
        List<Island> nextInChain = new List<Island>() { island };
        

        bool chainContinues = true;
        while (chainContinues)
        {
            List<Island> addToChain = new List<Island>();
            foreach (Island i in nextInChain)
            {
                addToChain.AddRange(island.Bridges.Where(a => !islandChain.Contains(a.Second)).Select(b => b.Second).ToList());               
            }

            nextInChain = addToChain;

            if (nextInChain.Count == 0)
                chainContinues = false;
            else
                islandChain.AddRange(nextInChain);
        }

        return islandChain;
    }

    private Island combineIslands(List<Island> islands)
    {
        List<Tile> tilesInIslands = islands.SelectMany(t => t.Tiles).Distinct().ToList();
        return new Island(tilesInIslands) ;
    }

    public void FinishIsland(Island island, List<Penguin> penguins)
    {
        List<List<Move>> paths = GetAllPathsM(island, penguins);
        List<Move> bestPath = GetBestPath(paths).First;
        foreach (Move move in bestPath)
            StartCoroutine(MoveWhenReady(move));
    }

    public void FinishMultiplePenguinIsland(Island island)
    {
        int perfectPath = island.Tiles.Where(t => t.penguin == null).Count();
        List<Penguin> penguins = island.Penguins;
        List<List<Move>>[] penguinsPaths = new List<List<Move>>[4];

        int bestPathSize = 0;
        List<Move> bestPath = new List<Move>();
        int thisPathSize;

        Stopwatch stop = new Stopwatch();
        stop.Start();
        
        for (int i = 0; i < penguins.Count; i++)
        {
            penguinsPaths[i] = GetAllPaths2(island, penguins[i]);
            if(penguinsPaths[i].Count == 1 && penguinsPaths[i][0].Count == perfectPath)
            {
                bestPath = penguinsPaths[i][0];
                foreach (Move move in bestPath)
                    StartCoroutine(MoveWhenReady(move));
                return;
            }
        }
        stop.Stop();

        print("For the island with " + island.Tiles.Count + " tiles and " + island.Penguins.Count + " penguins, getting paths took: " + stop.Elapsed);


        int penguinOnePathCount = penguinsPaths[0] != null ? penguinsPaths[0].Count : 0;
        int penguinTwoPathCount = penguinsPaths[1] != null ? penguinsPaths[1].Count : 0;
        int penguinThreePathCount = penguinsPaths[2] != null ? penguinsPaths[2].Count : 0;
        int penguinFourPathCount = penguinsPaths[3] != null ? penguinsPaths[3].Count : 0;

        for (int a = 0; a < penguinOnePathCount; a++)
        {
            List<Move> penguinOnePath = penguinsPaths[0][a];
            List<Tile> penguinOnePathTiles = penguinOnePath.Select(m => m.from).ToList();
            penguinOnePathTiles.Add(penguinOnePath.Last().to);
            for (int b = 0; b < penguinTwoPathCount; b++)
            {
                List<Move> penguinTwoPath = penguinsPaths[1][b];
                List<Tile> penguinTwoPathTiles = penguinTwoPath.Select(m => m.from).ToList();
                penguinTwoPathTiles.Add(penguinTwoPath.Last().to);
                if (penguinOnePathTiles.Intersect(penguinTwoPathTiles).Any())
                {
                    continue;
                }
                if (penguins.Count == 2)
                {
                    thisPathSize = penguinsPaths[0][a].Count + penguinsPaths[1][b].Count;
                    if(thisPathSize == perfectPath)
                    {
                        bestPath = penguinsPaths[0][a].Concat(penguinsPaths[1][b]).ToList();
                    }
                    if(thisPathSize > bestPathSize)
                    {
                        bestPathSize = thisPathSize;
                        bestPath = penguinsPaths[0][a].Concat(penguinsPaths[1][b]).ToList();
                    }
                }
                for (int c = 0; c < penguinThreePathCount; c++)
                {
                    List<Move> penguinThreePath = penguinsPaths[2][c];
                    List<Tile> penguinThreePathTiles = penguinThreePath.Select(m => m.from).ToList();
                    penguinThreePathTiles.Add(penguinThreePath.Last().to);
                    if (penguinOnePathTiles.Concat(penguinTwoPathTiles).Intersect(penguinThreePathTiles).Any())
                    { 
                        continue;
                    }
                    if (penguins.Count == 3)
                    {
                        thisPathSize = penguinsPaths[0][a].Count + penguinsPaths[1][b].Count + penguinsPaths[2][c].Count;
                        if (thisPathSize == perfectPath)
                        {
                            bestPath = penguinsPaths[0][a].Concat(penguinsPaths[1][b]).Concat(penguinsPaths[2][c]).ToList();
                        }
                        if (thisPathSize > bestPathSize)
                        {
                            bestPathSize = thisPathSize;
                            bestPath = penguinsPaths[0][a].Concat(penguinsPaths[1][b]).Concat(penguinsPaths[2][c]).ToList();
                        }
                    }
                    for (int d = 0; d < penguinFourPathCount; d++)
                    {
                        List<Move> penguinFourPath = penguinsPaths[3][d];
                        List<Tile> penguinFourPathTiles = penguinFourPath.Select(m => m.from).ToList();
                        penguinFourPathTiles.Add(penguinFourPath.Last().to);
                        if (penguinOnePathTiles.Concat(penguinTwoPathTiles).Concat(penguinThreePathTiles).Intersect(penguinFourPathTiles).Any())
                        {
                            continue;
                        }
                        if (penguins.Count == 4)
                        {
                            thisPathSize = penguinsPaths[0][a].Count + penguinsPaths[1][b].Count + penguinsPaths[2][c].Count + penguinsPaths[3][d].Count;
                            if (thisPathSize == perfectPath)
                            {
                                bestPath = penguinsPaths[0][a].Concat(penguinsPaths[1][b]).Concat(penguinsPaths[2][c]).Concat(penguinsPaths[3][d]).ToList();
                            }
                            if (thisPathSize > bestPathSize)
                            {
                                bestPathSize = thisPathSize;
                                bestPath = penguinsPaths[0][a].Concat(penguinsPaths[1][b]).Concat(penguinsPaths[2][c]).Concat(penguinsPaths[3][d]).ToList();
                            }
                        }
                    }
                }
            }
        }

        foreach (Move move in bestPath)
            StartCoroutine(MoveWhenReady(move));
    }

    public void FinishSinglePenguinIsland(Island island)
    {
        List<List<Move>> paths = GetAllPaths(island, island.Penguins.First());
        List<Move> bestPath = GetBestPath(paths).First;
        print("The island has " + island.Tiles.Count + " Tiles and the path has " + bestPath.Count + " moves in it.");
        foreach (Move move in bestPath)
            StartCoroutine(MoveWhenReady(move));
    }

    public List<List<Move>> GetAllPaths2(Island island, Penguin penguin)
    {
        List<List<Move>> paths = new List<List<Move>>();
        List<List<Move>> activePaths = new List<List<Move>>();
        Tile start = penguin.Tile;

        foreach (Tile t in start.getFreeNeighbours().Where(t => island.Tiles.Contains(t)))
        {
            Move move = new Move(penguin, start, t, t.numFish);
            activePaths.Add(new List<Move>() { move });
        }

        List<List<Move>> newPaths;

        while (activePaths.Count > 0)
        {
            newPaths = new List<List<Move>>();
            foreach (List<Move> path in activePaths)
            {
                Tile current = path.Last().to;
                List<Move> newPath;

                List<Tile> tilesInPath = path.Select(m => m.from).ToList();
                tilesInPath.Add(current);

                List<Tile> potentialTiles = current.getFreeNeighbours()
                    .Where(n => !tilesInPath.Contains(n)).ToList();
                
                if (potentialTiles.Count == 0)
                {
                    if (path.Count == island.Tiles.Where(t => t.penguin == null).Count())
                        return new List<List<Move>> { path };
                }
                paths.Add(path);

                if (potentialTiles.Count >= 1)
                {
                    foreach (Tile t in potentialTiles)
                    {
                        newPath = new List<Move>(path);
                        newPath.Add(new Move(penguin, current, t, t.numFish));
                        newPaths.Add(newPath);
                    }
                }
            }
            activePaths = newPaths;
        }

        return paths;

    }

    public List<List<Move>> GetAllPaths(Island island, Penguin penguin)
    {
        List<List<Move>> paths = new List<List<Move>>();
        List<List<Move>> activePaths = new List<List<Move>>();
        Tile start = penguin.Tile;

        foreach (Tile t in start.getFreeNeighbours().Where(t => island.Tiles.Contains(t)))
        {
            Move move = new Move(penguin, start, t, t.numFish);
            activePaths.Add(new List<Move>() { move });
        }

        List<List<Move>> newPaths;

        while (activePaths.Count > 0)
        {
            newPaths = new List<List<Move>>();
            foreach (List<Move> path in activePaths)
            {
                Tile current = path.Last().to;
                List<Move> newPath;

                List<Tile> tilesInPath = path.Select(m => m.from).ToList();
                tilesInPath.Add(current);

                List<Tile> potentialTiles = current.getFreeNeighbours()
                    .Where(n => !tilesInPath.Contains(n)).ToList();

                if (potentialTiles.Count == 0)
                {
                    if (path.Count == island.Tiles.Where(t => t.penguin == null).Count())
                        return new List<List<Move>> { path };
                    else
                        paths.Add(path);
                }
                //TODO: Remove this if statement make the other >= 1
                if (potentialTiles.Count == 1)
                {
                    newPath = new List<Move>(path);
                    newPath.Add(new Move(penguin, current, potentialTiles.First(), potentialTiles.First().numFish));
                    newPaths.Add(newPath);
                }

                if (potentialTiles.Count > 1)
                {
                    foreach (Tile t in potentialTiles)
                    {
                        newPath = new List<Move>(path);
                        newPath.Add(new Move(penguin, current, t, t.numFish));
                        newPaths.Add(newPath);
                    }
                }
            }
            activePaths = newPaths;
        }

        return paths;

    }

    private bool arePathsEqual(List<Move> path1, List<Move> path2)
    {
        if(path1.Count != path2.Count)
            return false;

        List<Penguin> penguinsInPath1 = path1.Select(m => m.penguin).Distinct().ToList();

        List<Penguin> penguinsInPath2 = path2.Select(m => m.penguin).Distinct().ToList();

        foreach (Penguin penguin in penguinsInPath1)
        {
            if (!penguinsInPath2.Contains(penguin))
                return false;

            Tile current1;
            if (path1.Where(a => a.penguin == penguin).Count() > 0)
                current1 = path1.Where(a => a.penguin == penguin).Last().to;
            else
                current1 = penguin.Tile;

            Tile current2;
            if (path2.Where(a => a.penguin == penguin).Count() > 0)
                current2 = path2.Where(a => a.penguin == penguin).Last().to;
            else
                current2 = penguin.Tile;

            if(current1 != current2)
                return false;
        }

        List<Tile> tilesInPath1 = path1.Select(m => m.from).ToList();
        tilesInPath1.Add(path2.Last().to);

        List<Tile> tilesInPath2 = path2.Select(m => m.from).ToList();
        tilesInPath2.Add(path2.Last().to);

        foreach(Tile tile in tilesInPath1)
            if (!tilesInPath2.Contains(tile))
                return false;

        return true;

    }

    public List<List<Move>> GetAllPathsM(Island island, List<Penguin> penguins)
    {
        List<List<Move>> paths = new List<List<Move>>();
        List<List<Move>> activePaths = new List<List<Move>>();

        foreach (Penguin p in penguins)
        {
            Tile start = p.Tile;
            foreach (Tile t in start.getFreeNeighbours().Where(t => island.Tiles.Contains(t)))
            {
                Move move = new Move(p, start, t, t.numFish);
                activePaths.Add(new List<Move>() { move });
            }
        }

        List<List<Move>> newPaths;

        while (activePaths.Count > 0)
        {
            newPaths = new List<List<Move>>();
            foreach (List<Move> path in activePaths)
            {
                List<Move> newPath;
                List<Move> nextMoves = new List<Move>();

                List<Tile> tilesInPath = path.Select(m => m.from).ToList();
                tilesInPath.Add(path.Last().to);

                foreach (Penguin p in penguins)
                {
                    Tile current;
                    if (path.Where(a => a.penguin == p).Count() > 0)
                        current = path.Where(a => a.penguin == p).Last().to;
                    else
                        current = p.Tile;

                    List<Tile> potentialTiles = current.getFreeNeighbours().Where(n => !tilesInPath.Contains(n)).ToList();
                    foreach(Tile t in potentialTiles)
                        nextMoves.Add(new Move(p, current, t, t.numFish));

                }

                if (nextMoves.Count == 0)
                {
                    if (path.Count == island.Tiles.Where(t => t.penguin == null).Count())
                    {
                        print("found perfect path. paths: " + (paths.Count + activePaths.Count()));
                        return new List<List<Move>> { path }; 
                    }
                    else
                        paths.Add(path);
                }

                if (nextMoves.Count >= 1)
                {
                    foreach (Move move in nextMoves)
                    {
                        newPath = new List<Move>(path);
                        newPath.Add(move);

                        //List<List<Move>> allPaths = paths.Concat(newPaths).ToList();
                       // bool unique = true;
                        //foreach (List<Move> apath in allPaths)
                            //if (!(unique = unique && !arePathsEqual(newPath, apath))) break;
                            
                        //if (allPaths.Count == 0 || unique)
                            newPaths.Add(newPath);

                        if (newPaths.Count > 10000)
                        {
                            print("too many paths");
                            return paths.Concat(activePaths).ToList();
                        }
                    }
                }
            }
            activePaths = newPaths;
            
        }

        print(paths.Count());
        return paths;

    }

    public Tuple<List<Move>,int> GetBestPath(List<List<Move>> paths)
    {
        List<Move> bestPath = new List<Move>();
        int bestPathPoints = 0;

        foreach(List<Move> path in paths)
        {
            int points = path.Select(p => p.to.numFish).Sum();
            if (points > bestPathPoints)
            {
                bestPath = path;
                bestPathPoints = points;
            }

        }
        return new Tuple<List<Move>, int>(bestPath, bestPathPoints); ;
    }


}


