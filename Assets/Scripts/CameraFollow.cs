using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] float smoothTime = 10f;   // Wie weich die Kamera folgt
    [SerializeField] float leaderSwitchThreshold = 5f; // Abstand, ab wann Führungswechsel passiert

    private Vector3 velocity = Vector3.zero;
    private PlayerControl currentLeader;
    private List<PlayerControl> players;

    public static CameraFollow instance;

    float camOffset = 5f;

    float camZ = -10;


    Vector3 startPos;

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {

        startPos = transform.position;
        players = FindObjectsOfType<PlayerControl>().ToList();
    }

    public void CamStart()
    {
        startPos = transform.position;
        players = FindObjectsOfType<PlayerControl>().ToList();
        currentLeader = players[0]; // irgendeinen als Startleader nehmens
    }

    void LateUpdate()
    {
        if (players.Count == 0 || currentLeader == null) return;

        // Neuen Leader bestimmen, wenn jemand deutlich vorne ist
        PlayerControl furthest = players.Where(p=> p != null).OrderByDescending(p => p.transform.position.x).First();
        if (furthest != currentLeader &&
            furthest.transform.position.x - currentLeader.transform.position.x > leaderSwitchThreshold)
        {
            currentLeader = furthest;
        }

        // Smooth Follow zur X-Position des Leaders
        Vector3 targetPos = new Vector3(currentLeader.transform.position.x + camOffset, transform.position.y, camZ);

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, smoothTime);
        

    }

    public void ResetCamera()
    {
        transform.position = startPos;
    }

    public void UpdatePlayers(List<PlayerControl> newPlayers)
    {
        players = newPlayers;
        if (players.Count > 0)
            currentLeader = players[0];
        else
            currentLeader = null;
    }
}