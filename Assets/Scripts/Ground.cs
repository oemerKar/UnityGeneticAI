using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground : MonoBehaviour
{
    int playersOnThisFloor = 0;
    PlayerManager playerManager;

    void Start()
    {
        playerManager = PlayerManager.instance;
    }

    // Update is called once per frame
    void Update()
    {
        TryRemove();
    }


    public bool hasPlayers()
    {
        return playersOnThisFloor > 0;
    }

    public void TryRemove()
    {
        if(!hasPlayers() && transform.position.x + 25< playerManager.GetLetzterPlayerX()) //+20 da alle Grounds 20 breit sind und er möglichst spät löschen soll
        {
            GroundSpawner.instance.DeleteFloor();
        }
    }

    public void PlayerEntered()
    {
        playersOnThisFloor++;
    }

    public void PlayerExited()
    {
        playersOnThisFloor = Mathf.Max(0, playersOnThisFloor - 1);
        TryRemove();
    }
}
