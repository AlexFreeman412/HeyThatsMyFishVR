using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Diagnostics;

public class AIPlayer : Player {

    public Board board;
    private Move moveToMake;
    private bool thinking = false;

    void Awake()
    {
        score = 0;
        isHuman = false;
        board = GameObject.Find("Board").GetComponent<Board>();
    }

    public void play()
    {
        moveToMake = null;

        List<Penguin> penguins = board.Penguins.FindAll(p => p.playerNumber() == playerNumber);
        List<Move> moves = new List<Move>(); 

        foreach (Penguin p in penguins)
        {
            //if (!(p.singleOwner && !p.onBridge) || board.allIslandsOwned)
            //{
            List<Tile> tiles = p.ValidMoves;
            //int pointModifier = penguinInDanger(p);
            foreach (Tile tile in tiles)
                moves.Add(new Move(p, p.Tile, tile, tile.numFish));
            //}
        }


        foreach (Move move in moves)
        {
            move.points += whoIsStuck(move, board);
            //move.points += pointsForIslands(move, board);
        }

        moveToMake = getBestMove(moves);
        thinking = false;

        /*
        foreach (Penguin p in penguins)
        {
            print ( getBestMove(moves, p) );
        }
        */

        StartCoroutine(board.MoveWhenReady(moveToMake));

        StartCoroutine(board.NextTurnWhenReady());

    }

    public void StartPenguin()
    {
        Penguin penguin = board.Penguins.Where(p => p.Tile == null && p.playerNumber() == playerNumber).First();

        System.Random rnd = new System.Random();
        Tile tile = board.getTileAtCoord(rnd.Next(2, 8), rnd.Next(2, 8));
        while (tile.numFish != 1 || tile.penguin != null)
            tile = board.getTileAtCoord(rnd.Next(2, 8), rnd.Next(2, 8));

        board.startPenguin(penguin, tile);
    }

    public int penguinInDanger(Penguin p)
    {
        int pointModifier = 0;
        List<Tile> neighbours = p.Tile.getFreeNeighbours();
        if (neighbours.Count == 1)
            pointModifier += 5;

        return pointModifier;
    }

    public int whoIsStuck(Move move, Board board)
    {
        int pointModifier = 0;        
        //GameObject empty = new GameObject("empty");
        //Penguin fakePenguin = empty.AddComponent<Penguin>();
        move.to.penguin = move.penguin;
        move.penguin.Tile = move.to;
        move.from.IsRemoved = true;
        move.from.penguin = null;
        //fakePenguin.Tile = move.to;
        //if (fakePenguin.ValidMoves.Count == 0)
        //    pointModifier -= 5;
        foreach (Penguin p in board.Penguins)
        {
            float temper = Mathf.Min((float)(p.pointsAvailable * 3 / 60), 1);
            if (p != move.penguin)
                if (p.ValidMoves.Count == 0)
                    if (p.playerNumber() != playerNumber)
                        pointModifier += (int)(5 * temper);
                    else
                        pointModifier -= (int)(5 * temper);
        }
        move.to.penguin = null;
        move.from.IsRemoved = false;
        move.from.penguin = move.penguin;
        move.penguin.Tile = move.from;
        //Destroy(empty);
        return pointModifier;
    }

    public int pointsForIslands(Move move, Board board)
    {
        int wasSittingOn = 0;
        int otherWasSittingOn = 0;
        //int[] oldSittingOnPoints = board.Penguins.Select(i => i.sittingOn).ToArray();
        float temper;
        if (move.penguin.pointsAvailable != 0)
            temper = Mathf.Min((float)(1 / move.penguin.pointsAvailable), 1);
        else
            temper = 1;

        foreach (Penguin p in board.Penguins)
            if (p.player == this)
                wasSittingOn += p.sittingOn;
            else
                otherWasSittingOn += p.sittingOn;

        bool[] onBridgeState = board.Penguins.Select(i => i.onBridge).ToArray();

        move.from.IsRemoved = true;
        move.from.penguin = null; 

        move.to.penguin = move.penguin;
        move.penguin.Tile = move.to;


        List<Island> newIslands = board.GetAllIslands(board.Tiles);
        board.AddPenguinIslandInfo(newIslands);

        int isSittingOn = 0;
        int otherIsSittingOn = 0;

        foreach (Penguin p in board.Penguins)
            if (p.player == this)
                isSittingOn += p.sittingOn;
            else
                otherIsSittingOn += p.sittingOn;

        int pointModifier = (int)( ( (isSittingOn - wasSittingOn) - (otherIsSittingOn - otherWasSittingOn) )
            * temper);

        /*
        for(int i = 0; i < board.Penguins.Count; i++)
        {
            board.Penguins[i].sittingOn = oldSittingOnPoints[i];
            board.Penguins[i].onBridge = onBridgeState[i];
        }
        */

        move.to.penguin = null;
        move.penguin.Tile = move.from;

        move.from.IsRemoved = false;
        move.from.penguin = move.penguin;

        board.AddPenguinIslandInfo(board.Islands);

        return pointModifier;
    }

    public Move getBestMove(List<Move> moves)
    {
        Move bestMove = moves[0];
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].points > bestMove.points)
                bestMove = moves[i];
        return bestMove;
    }

    public Move getBestMove(List<Move> moves, Penguin p)
    {
        if (moves == null || moves.Where(move => move.penguin == p).Count() == 0 || p == null)
            return new Move();

        Move bestMove = moves.Where(e => e.penguin == p).First();
        for (int i = 0; i < moves.Count; i++)
            if (moves[i].points > bestMove.points && moves[i].penguin == p)
                bestMove = moves[i];
        return bestMove;
    }
}
