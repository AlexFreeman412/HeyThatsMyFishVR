using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;

public class Penguin : MonoBehaviour
{
    public Vector3 originalPosition;
    public int originalSiblingIndex;

    public Player player;

    public bool selected;

    public Board board;

    public List<Island> islands;
    public int sittingOn;
    public int pointsAvailable;
    public bool onBridge;
    public bool singleOwner;

    public float speed = 3f;

    public bool isStuck;
    public bool isMoving;
    public bool isColliding;

    /// Tile that the penguin is currently occupying.
    public Tile Tile { get; set; }
    private bool leavingTile = false;

    private const float HEIGHT = 1.1f;

    public List<Tile> ValidMoves;

    /// Tile that the penguin is currently hovering over.
    public Tile OverTile { get; private set; }
    public List<GameObject> OverTiles { get; private set; }

    /// PenguinSelected event is invoked when user clicks on unit that belongs to him. It requires a collider on the unit game object to work.
    public event EventHandler PenguinSelected;
    public event EventHandler PenguinDeselected;

    private Rigidbody rb;

    private Transform PenguinParent;

    void Awake()
    {

        PenguinParent = transform.parent;

        board = GameObject.Find("Board").GetComponent<Board>();

        rb = GetComponent<Rigidbody>();

        selected = false;

        originalPosition = transform.position;
        originalSiblingIndex = transform.GetSiblingIndex();

        OverTiles = new List<GameObject>();
    }

    void FixedUpdate()
    {
        
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0.0f, transform.eulerAngles.y, 0.0f));

        if (inBoardSpace() && !penguinIsUpright())
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Mathf.Min(100f * Time.deltaTime, 1));
        }

        Tile newOverTile;
        if (OverTiles.Count > 0)
            newOverTile = FindClosestTile();
        else
            newOverTile = null;

        if (newOverTile != OverTile)
                OnOverTileChanged(OverTile, newOverTile);
 
    }

    private bool inBoardSpace()
    {
        return transform.position.x > -2f && transform.position.x < 17f
            && transform.position.y > 1f && transform.position.y < 3.5
            && transform.position.z > -2f && transform.position.z < 15f;
    }

    private bool penguinIsUpright()
    {
        return transform.eulerAngles.x < 10
            && transform.eulerAngles.z < 10;
    }

    void OnOverTileChanged(Tile oldTile, Tile newTile)
    {
        if (leavingTile)
        {
            leavingTile = false;
            OverTile.OnPenguinLeave();
        }
            
        OverTile = newTile;

        if (player.isHuman && selected)
        {
            if (oldTile != null && oldTile != Tile)
                updateOldTile(oldTile);

            if (newTile != null && newTile != Tile)
                updateNewTile(newTile);
        }
    }

    private void updateOldTile(Tile oldTile)
    {
        if (board.setupPhase && oldTile.numFish == 1
            || !board.setupPhase && ValidMoves.Contains(oldTile))
        {
            oldTile.MarkAsValidMove();
            return;
        }

        oldTile.Unmark();       
    }

    private void updateNewTile(Tile newTile)
    {
        if (board.setupPhase && newTile.numFish == 1
            || !board.setupPhase && ValidMoves.Contains(newTile))
        {
            newTile.MarkAsSelected();
            return;
        }

        newTile.MarkAsInvalidMove();
    }

    public void StartAt(Tile startTile)
    {
        Tile = startTile;
        Tile.OnPenguinEnter(this);
        transform.position = new Vector3(Tile.transform.position.x, 1.1f, Tile.transform.position.z);
    }

    public virtual void Move(Tile destinationTile)
    {
    
        if (Tile == destinationTile || destinationTile == null)
            return;

        if (Tile != null)
            if (player.isHuman)
                Tile.OnPenguinLeave();
            else
                leavingTile = true;

        Tile = destinationTile;
        Tile.OnPenguinEnter(this);

        StartCoroutine(MovementAnimation());
    }

    private IEnumerator MovementAnimation()
    {
        
        isMoving = true;
        rb.isKinematic = true;

        Vector3 targetPosition = new Vector3(Tile.transform.position.x, 1.1f, Tile.transform.position.z);

        if (!player.isHuman)
        {
            Quaternion targetRotation = Quaternion.Euler(0.0f, (Quaternion.LookRotation(targetPosition - transform.position).eulerAngles.y), 0.0f);
            transform.rotation = targetRotation;
            /*
            //Vector3.Angle(transform.forward, targetRotation.eulerAngles) > 1
            while (transform.rotation != targetRotation)
            {
                float str = Mathf.Min(5f * Time.deltaTime, 1f);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 75f * Time.deltaTime);
                yield return 0;
            }
            */
        }


        while (transform.position.x != targetPosition.x ||
            transform.position.z != targetPosition.z)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * speed);

            if (Mathf.Abs(Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(targetPosition.x, targetPosition.z))) < 0.1f && transform.position.y < 2)
            {
                transform.position = targetPosition;                
            }
            yield return 0;
        }

        rb.isKinematic = false;
        isMoving = false;
    }

    ///<summary>
    /// Method indicates if penguin is capable of moving to tile given as parameter.
    /// </summary>
    public virtual bool IsTileMovableTo(Tile tile)
    {
        return !tile.IsRemoved && tile.penguin == null;
    }

    public int playerNumber()
    {
        return player.playerNumber;
    }
    
    void OnTriggerEnter(Collider col)
    {

        if (col.tag == "Tile")
        {
            //print(GetComponent<Collider>().name + " enter: " + col.name);

            OverTiles.Add(col.gameObject);           
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (col.tag == "Tile")
        {
            //print(GetComponent<Collider>().name + " exits: " + col.name);
            OverTiles.Remove(col.gameObject);                
        }
    }
    
    public Tile FindClosestTile()
    {
        Tile closestTile = null;
        float shortestDist = 100.0f;
        foreach(GameObject tile in OverTiles)
        {
            float dist = Mathf.Abs(Vector3.Distance(tile.transform.position, transform.position));
            if (dist < shortestDist)
            {
                closestTile = tile.GetComponent<Tile>();
                shortestDist = dist;
            }
        }
        //print(closestTile.ToString());
        return closestTile;
    }

    public void UpdateValidMoves()
    {
        ValidMoves = GetAvailableDestinations();
    }

    public List<Tile> GetAvailableDestinations()
    {
        if(Tile == null)
        {
            print(this.name);
        }
        Tile nextTile = Tile;

        var ret = new List<Tile>();

        foreach(Tile.direction direction in Enum.GetValues(typeof(Tile.direction)))
        {
            nextTile = Tile;
            while (true)
            {
                nextTile = nextTile.getTileInDirection(direction);
                if (nextTile != null
                    && !nextTile.IsRemoved
                    && nextTile.penguin == null)
                    ret.Add(nextTile);
                else
                    break;
            }
        }

        return ret;
    }

    public void SetColor(Color color)
    {
        if (gameObject.GetComponent<MeshRenderer>() != null)
            gameObject.GetComponent<MeshRenderer>().material.SetColor("_Color", color);
    }

    public void ResetPosition()
    {
        transform.SetParent(PenguinParent);
        if (Tile != null)
            transform.position = new Vector3(Tile.transform.position.x, HEIGHT, Tile.transform.position.z);
        else
        {
            rb.isKinematic = true;
            transform.position = originalPosition;
        }

        transform.rotation = Quaternion.Euler(new Vector3(0.0f, transform.eulerAngles.y, 0.0f));

    }

    public void Reset()
    {
        transform.position = originalPosition;
        rb.isKinematic = true;

        transform.SetParent(PenguinParent);
        transform.SetSiblingIndex(originalSiblingIndex);
        gameObject.SetActive(false);

        Tile = null;

        selected = false;

        isStuck = false;
        isMoving = false;

        islands = null;
        sittingOn = 0;
        pointsAvailable = 0;
        onBridge = false;
        singleOwner = false;
    }

}
