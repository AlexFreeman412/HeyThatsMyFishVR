using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/*
* There are four states an island can be in. 
* Uninhabited (no penguins on island) - Island is irrelevant to the game
* Finished (penguins on island but no moves available) - Island is irrelevant to the game
* Owned (only one player has penguins on island) - Relevant and given a return value of true
* Unowned (multiple players have penguins on island - Relevant and given a return value of false
*/

public class Island
{

    public List<Tile> Tiles;
    public List<Penguin> Penguins;
    public List<Tuple<Tile, Island>> Bridges;

    public bool isOwned;
    public bool hasSinglePenguin;
    public bool isInhabited;
    public bool isFinished;

    public int totalPoints = 0;

    public Island(List<Tile> tiles)
    {
        Tiles = tiles;

        addPenguinsAndPoints();

        hasSinglePenguin = (Penguins.Count == 1);

        isInhabited = (Penguins.Count > 0);

        isOwned = isInhabited ? isIslandOwned() : false;

        isFinished = (Tiles.Count == Penguins.Count) || !isInhabited;

        Bridges = new List<Tuple<Tile, Island>>();
    }

    private void addPenguinsAndPoints()
    {
        Penguins = new List<Penguin>();
        foreach (Tile tile in Tiles)
        {
            if(tile.penguin == null)
                totalPoints += tile.numFish;
            if (tile.penguin != null)
                Penguins.Add(tile.penguin);
        }
    }

    private bool isIslandOwned()
    {
        bool isOwned = true;
        Player owner = null;
        foreach (Penguin penguin in Penguins)
        {
            if (owner == null)
                owner = penguin.player;
            isOwned = isOwned && (owner == penguin.player);
        }
        return isOwned;
    }

    public void AddBridge(Tile bridgeTile, List<Island> otherIslands)
    {
        foreach(Island island in otherIslands)
        {
            Tuple<Tile, Island> bridge = new Tuple<Tile, Island>(bridgeTile, island);
            Bridges.Add(bridge);
        }        
    }

    public bool Equals(Island island)
    {
        bool equal = true;
        foreach (Tile tile in Tiles)
            equal = equal && island.Tiles.Contains(tile);
        return equal;
    }

}
