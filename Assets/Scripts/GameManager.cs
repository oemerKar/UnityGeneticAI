using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Start, Running, GameOver}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    PlayerManager playerManager;
    GroundSpawner groundSpawner;
    CameraFollow cameraFollow;
    DistanceCounter distanceCounter;

    bool brainInit = false;

    public GameState state = GameState.Start;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        playerManager = PlayerManager.instance;
        groundSpawner = GroundSpawner.instance;
        cameraFollow = CameraFollow.instance;
        distanceCounter = DistanceCounter.instance;

        StartGame();
    }

    public void StartGame()
    {
        state = GameState.Running;
        playerManager.gameObject.SetActive(true);
        groundSpawner.gameObject.SetActive(true);
        cameraFollow.gameObject.SetActive(true);

        // Reset GroundSpawner & PlayerManager
        groundSpawner.ResetSpawner();
        playerManager.SpawnPlayers();
        groundSpawner.SpawnFirst();
        cameraFollow.ResetCamera();
        cameraFollow.CamStart();
        if (!brainInit)
        {
            brainInit = true;
            GeneticAlgorithm.instance.Initialization();
        }
        else
        {
            GeneticAlgorithm.instance.Selection();
            Debug.Log("Selection Time");
        }
    }

    public void GameOver()
    {
        state = GameState.GameOver;
        Debug.Log("Alle Spieler tot. Spielende!");
        // Optional: Hier UI anzeigen

        ResetGame();
    }

    public void ResetGame()
    {

        playerManager.GetPlayers().Clear();

        // Reset GroundSpawner
        groundSpawner.ResetSpawner();

        // Reset Kamera
        cameraFollow.ResetCamera();

        distanceCounter.distance = 0;

        GenCounter.instance.gen++;
        StartGame();
    }
}
