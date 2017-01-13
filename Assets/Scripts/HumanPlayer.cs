using UnityEngine;
using System.Collections;

public class HumanPlayer : Player {

    void Awake()
    {
        score = 0;
        isHuman = true;
    }

}
