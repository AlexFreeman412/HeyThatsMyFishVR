using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class Tile : MonoBehaviour {

    public Material oneTile;
    public Material twoTile;
    public Material threeTile;
    public Material water;

    public EventHandler TileClicked;

    private MeshRenderer imageRenderer;

    public Point coord;

    public bool IsRemoved = false;

    public Penguin penguin = null;

    public int numFish; //numfish = 0 means tile is gone

    public Board board;

    public List<Tile> Neighbours;

    void Start () {
        board = GetComponentInParent<Board>();
        imageRenderer = transform.FindChild("TileImage").gameObject.GetComponent<MeshRenderer>();
        transform.Rotate(Vector3.forward, 60 * UnityEngine.Random.Range(1, 6)); //Gives each tile a random rotation. Starting at 30 (this aligns them properly) and incrementing by 60 (hexagons angles are 60 degrees each).
    }

    void OnMouseDown()
    {
        if (TileClicked != null)
            TileClicked.Invoke(this, new EventArgs());
    }

    public void OnPenguinLeave()
    {
        gameObject.SetActive(false);
        IsRemoved = true;
        penguin = null;
    }

    public void OnPenguinEnter(Penguin penguin)
    {
        this.penguin = penguin;
    }

    public List<Tile> getFreeNeighbours()
    {
        List<Tile> neighbours = getNeighbours().Where(e => e.penguin == null).ToList();
        return neighbours;
    }

    public List<Tile> getNeighbours()
    {
        List<Tile> neighbours = new List<Tile>();
        foreach (direction direction in Enum.GetValues(typeof(direction)))
        {
            Tile tile = getTileInDirection(direction);
            if (tile != null && !tile.IsRemoved)
                neighbours.Add(tile);
        }
        return neighbours;
    }

    public Tile getTileInDirection(direction direction)
    {
        switch (direction)
        {
            case direction.RIGHT:
                return board.getTileAtCoord(coord.x + 1, coord.y);
            case direction.LEFT:
                return board.getTileAtCoord(coord.x - 1, coord.y);
            case direction.UPLEFT:
                if(coord.y % 2 == 1)
                    return board.getTileAtCoord(coord.x - 1, coord.y + 1);
                else
                    return board.getTileAtCoord(coord.x, coord.y + 1);
            case direction.UPRIGHT:
                if (coord.y % 2 == 1)
                    return board.getTileAtCoord(coord.x, coord.y + 1);
                else
                    return board.getTileAtCoord(coord.x + 1, coord.y + 1);
            case direction.DOWNLEFT:
                if (coord.y % 2 == 1)
                    return board.getTileAtCoord(coord.x - 1, coord.y - 1);
                else
                    return board.getTileAtCoord(coord.x, coord.y - 1);
            case direction.DOWNRIGHT:
                if (coord.y % 2 == 1)
                    return board.getTileAtCoord(coord.x, coord.y - 1);
                else
                    return board.getTileAtCoord(coord.x + 1, coord.y - 1);
            default: //Can't happen
                return null;
        }
    }

    public void updateImage()
    {
        if (imageRenderer == null)
            imageRenderer = transform.FindChild("TileImage").gameObject.GetComponent<MeshRenderer>();

        switch (numFish)
        {
            case 1:
                imageRenderer.material = oneTile;
                return;
            case 2:
                imageRenderer.material = twoTile;
                return;
            case 3:
                imageRenderer.material = threeTile;
                return;
            default:
                imageRenderer.material = water;
                return;
        }
    }

    public void MarkAsValidMove()
    {
        setHighlightColor(Color.green);
    }

    public void MarkAsInvalidMove()
    {
        setHighlightColor(Color.red);
    }

    public void MarkAsSelected()
    {
        setHighlightColor(Color.yellow);
    }

    public void MarkAsCurrent()
    {
        setHighlightColor(Color.magenta);
    }

    public void Unmark()
    {
        setHighlightColor(Color.black);
    }

    private void setHighlightColor(Color color)
    {
        var outline = transform.FindChild("Outline");
        for(int i = 0; i < outline.childCount; i++)
        {
            var renderer = outline.GetChild(i).GetComponent<MeshRenderer>();
            renderer.material.SetColor("_Color", color);
        }
    }

    public void Reset()
    {
        IsRemoved = false;
        penguin = null;
        gameObject.SetActive(true);
    }

    public override string ToString()
    {
        return coord.ToString();
    }

    public enum direction
    {
        RIGHT,
        LEFT,
        UPRIGHT,
        UPLEFT,
        DOWNRIGHT,
        DOWNLEFT
    }

}



/*public readonly Vector3 RIGHT = new Vector3 ( 1.892f, 0f, 0f );
    public readonly Vector3 LEFT = new Vector3(-1.892f, 0f, 0f);
    public readonly Vector3 UPRIGHT = new Vector3(0.946f, -1.65f, 0f);
    public readonly Vector3 UPLEFT = new Vector3(-0.946f, -1.65f, 0f);
    public readonly Vector3 DOWNRIGHT = new Vector3(0.946f, 1.65f, 0f);
    public readonly Vector3 DOWNLEFT = new Vector3(-0.946f, 1.65f, 0f);
    */
