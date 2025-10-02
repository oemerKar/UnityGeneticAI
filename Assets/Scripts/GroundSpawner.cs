using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GroundSpawner : MonoBehaviour
{
    [SerializeField]List <GameObject> groundSegments; //bestehend aus visuellem Boden und dünner Colliderschicht oben
    float lochProb = 0.5f;
    [SerializeField]Queue<GameObject> floors = new Queue<GameObject>();

    float lastX = 0;

    float floorSize  = 20; //wie lang ist ein Floor
    float floorY = -4.74f; //y Pos vom Boden

    public PlayerControl player; //Der Player der am weitesten vorne ist (wird in PlayerManager gesetzt)

    float offset = 40f; //Offset vor dem neuer Boden gespawnt wird

    public static GroundSpawner instance; //Singleton


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


    void Start()
    {
        SpawnFirst();
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateFloor();
    }

    //Boden aussuchen 
    GameObject chooseGround()
    {
        float prob = Random.Range(0f, 1f);

        //Falls Prob < lochProb normaler Boden
        if(prob < lochProb)
        {
            lochProb = Mathf.Max(0f, lochProb - 0.05f); //Wahrscheinlichkeit für Löcher um 5 % erhöhen
            
            return groundSegments[0]; //wenn Wahrscheinlichkeit niedriger als Lochwahrscheinlichkeit, Boden ohne Loch
        }
        //Sonst Boden mit Loch
        else
        {
            float borders = (1- lochProb)/(groundSegments.Count-1); //aufteilen der Wahrscheinlichkeit auf Menge von Böden mit Löchern
            for(int i = 1; i < groundSegments.Count; i++)
            {
                if(prob < lochProb + i * borders)
                {
                    lochProb = Mathf.Min(1f, lochProb + 0.05f); //Wahrscheinlichkeit für Löcher um 5 % verringern
                    return groundSegments[i];
                }
            }
            return groundSegments[groundSegments.Count -1]; 
        }
    }

    //Instanziiert die ersten Boeden in Start
    public void SpawnFirst()
    {
        for(int i = -1; i < 6; i++)
        {
            GameObject floor = chooseGround();
            Spawn(floor, i);
            lastX = i * floorSize;
        }
        player = PlayerManager.instance.GetErsterPlayer();
    }

    //Spawnt neuen Floor vorne und entfernt alten hinten
    void SpawnGround()
    {
        GameObject floor = chooseGround();
        Vector3 newPos = new Vector3(lastX + floorSize, floorY, 0);
        lastX = newPos.x;
        Spawn(floor, newPos);
    }


    //Ruft wenn neuer Boden betreten wird SpawnGround auf und löscht wenn mind. 7 Böden da sind den hintersten
    void UpdateFloor()
    {
        float camX = Camera.main.transform.position.x;
        float spawnUntil = camX + offset; //Bis wohin gespawnt werden soll
        if(floors.Count == 0) { return; }
        GameObject lastFloor = floors.Last();
        float edge = lastFloor.transform.position.x + floorSize;
        while (spawnUntil >= edge)
        {
            SpawnGround();
            lastFloor = floors.Last();
            edge = lastFloor.transform.position.x + floorSize;
        }
        if (floors.Count >= 7)
        {
            DeleteFloor();
        }

    }


    //Spawnt die Floors
    void Spawn(GameObject floor, int pos)
    {
        Debug.Log("Spawn Floor at position: " + pos);
        GameObject instance = Instantiate(floor, new Vector3(pos * floorSize, floorY, 0), Quaternion.identity);
        floors.Enqueue(instance);
    }

    void Spawn(GameObject floor, Vector3 pos)
    {
        GameObject instance = Instantiate(floor,pos, Quaternion.identity);
        floors.Enqueue((instance));
    }

    public void DeleteFloor()
    {
        Destroy(floors.Dequeue());
    }

    public void ResetSpawner()
    {
        foreach (GameObject floor in floors)
        {
            Destroy(floor);
        }
        floors.Clear();
    }
}
