using UnityEngine;
using System.Collections;

public class Connect : MonoBehaviour {

    public Board board;

    void OnEnable()
    {
        board.OnConnectController(gameObject);
    }

    void OnDisable()
    {
        board.OnDisconnectController(gameObject);
    }

}
