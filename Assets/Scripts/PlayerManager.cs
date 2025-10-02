using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    public static PlayerManager instance;

    private List<PlayerControl> players = new List<PlayerControl>();

    [SerializeField] GameObject playerPrefab;
    [SerializeField] int playerCount = 20;
    [SerializeField] Color[] playerColors;
    [SerializeField] Vector3 spawnStartPos = new Vector3(1,0,0);    


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

    public void SpawnPlayers() //spawnt die Spieler, wird von GameManager aufgerufen
    {
        for (int i = 0; i < playerCount; i++)
        {
            GameObject instance = Instantiate(playerPrefab, spawnStartPos, Quaternion.identity);
            instance.GetComponent<SpriteRenderer>().color = playerColors[i];
            PlayerControl pc = instance.GetComponent<PlayerControl>();
            players.Add(pc);
        }
    }

    public List<PlayerControl> GetPlayers()
    {
        return players;
    }

    public PlayerControl GetLetzterPlayer()
    {
        if (players.Count == 0) return null;

        PlayerControl leader = null;
        float minX = float.MaxValue;

        foreach (var p in players)
        {
            if (p != null && p.transform.position.x < minX)
            {
                minX = p.transform.position.x;
                leader = p;
            }
        }

        return leader;
    }

    public PlayerControl GetErsterPlayer()
    {
        if (players.Count == 0) return null;

        PlayerControl leader = null;
        float maxX = float.MinValue;
        foreach (var p in players)
        {
            if (p != null && p.transform.position.x > maxX)
            {
                maxX = p.transform.position.x;
                leader = p;
            }
        }
        return leader;
    }

    public float GetLetzterPlayerX()
    {
        if(GetLetzterPlayer() == null) { return -1; }
        return GetLetzterPlayer().transform.position.x;
    }

    public float GetErsterPlayerX()
    {
        if (GetLetzterPlayer() == null) { return -1; }
        return GetErsterPlayer().transform.position.x;
    }

    public void RemovePlayer(PlayerControl pc)
    {
        if (players.Contains(pc))
        {
            players.Remove(pc);
        }

        // Kamera informieren, dass sich die Liste geändert hat
        CameraFollow.instance.UpdatePlayers(players);

        //wenn alle Spieler Tot sind wird neue Generation gestartet
        StartCoroutine("StartNewGen");
    }

    public IEnumerator StartNewGen()
    {
        if(players.Count == 0)
        {
            yield return new WaitForSeconds(3f);
            GameManager.Instance.GameOver();
        }
    }
}
