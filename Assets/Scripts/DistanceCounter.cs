using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DistanceCounter : MonoBehaviour
{
    //Singleton
    public static DistanceCounter instance;

    public float distance;

    PlayerControl player;

    [SerializeField] TMP_Text distanceText;

    PlayerManager playerManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerManager = PlayerManager.instance;
    }

    void Update()
    {
        getDistance();
        UIDarstellung();
    }

    void getDistance()
    {
        player = playerManager.GetErsterPlayer();
        if(player == null)
        {
            return;
        }
        distance = Mathf.Max(player.gameObject.transform.position.x, distance);
    }

    void UIDarstellung()
    {
        distanceText.text = ((int)distance).ToString() + "m";
    }
}
