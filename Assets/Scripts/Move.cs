using UnityEngine;
using System.Collections;
using System;

public class Move
{
    public Penguin penguin;
    public Tile to;
    public Tile from;
    public int points;

    public Move()
    {
        points = 0;
    }

    public Move(Penguin penguin, Tile from, Tile to, int points)
    {
        this.penguin = penguin;
        this.to = to;
        this.from = from;
        this.points = points;
    }

    public bool Equals(Move move)
    {
        return (penguin == move.penguin &&
            to == move.to &&
            from == move.from);
    }

    public override String ToString()
    {
        if (penguin == null)
            return "This is not a move";

        return "Penguin: " + penguin.ToString()
            + " From: " + from.ToString()
            + " To: " + to.ToString()
            + " For " + points + " points";
    }


}
