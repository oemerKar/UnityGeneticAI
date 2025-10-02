using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneticAlgorithm : MonoBehaviour
{
    public static GeneticAlgorithm instance;
    List<NeuralNetwork> brains = new List<NeuralNetwork>();
    float mutationsProb = 0.05f;
    float mutationsRate = 0.05f;

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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Initialization() //einmal am anfang Random Gehirne zuweisen
    {
        foreach (PlayerControl player in PlayerManager.instance.GetPlayers()){
            NeuralNetwork brain = new NeuralNetwork(4, 7, 3);
            brains.Add(brain);
            player.SetBrain(brain);
        }
    }

    public void Selection()
    {
        List<NeuralNetwork> nextGen = new List<NeuralNetwork>(); 

        brains.Sort((a, b) => b.fitness.CompareTo(a.fitness)); //brains nach fitness score absteigend sortieren

        //Elitismus: die beiden mit der besten fitness genau übernehmen, der Rest wird später in Crossover überschrieben
        nextGen.Add(brains[0]);
        nextGen.Add(brains[1]);

        //CrossOver und Mutation umsetzen
        for(int i = 2; i < brains.Count; i++) //Die Brains mit Top 10 fitness werden gewählt um die neuen brains zu bestimmen
        {
            while(nextGen.Count < brains.Count)
            {
                //Eltern bestimmen
                int parentIn1 = Random.Range(0, 10);
                int parentIn2 = Random.Range(0, 10);
                while (parentIn1 == parentIn2) //Sicherstellen das unterschiedliche parents hat
                {
                    parentIn2 = Random.Range(0, 10);
                }

                //Kind als Crossover der Eltern
                NeuralNetwork child = CrossOver(brains[parentIn1], brains[parentIn2]);

                //Kind mutieren
                Mutate(child);

                nextGen.Add(child);
            }

        }

        //Brains ersetzen
        brains = nextGen;
        BrainToPlayer();

    }

    //CrossOverFunktion
    NeuralNetwork CrossOver(NeuralNetwork p1, NeuralNetwork p2)
    {
        NeuralNetwork child = new NeuralNetwork(p1); // Kopie von p1

        // Input zu Hidden
        for (int i = 0; i < child.weightsInputHidden.GetLength(0); i++)
        {
            for (int j = 0; j < child.weightsInputHidden.GetLength(1); j++)
            {
                child.weightsInputHidden[i, j] = (Random.value < 0.5f) ? p1.weightsInputHidden[i, j] : p2.weightsInputHidden[i, j];
            }
        }

        // Hidden zu Output
        for (int i = 0; i < child.weightsHiddenOutput.GetLength(0); i++)
        {
            for (int j = 0; j < child.weightsHiddenOutput.GetLength(1); j++)
            {
                child.weightsHiddenOutput[i, j] =(Random.value < 0.5f) ? p1.weightsHiddenOutput[i, j] : p2.weightsHiddenOutput[i, j];
            }
        }

        return child;
    }

    void Mutate(NeuralNetwork net)
    {
        for (int i = 0; i < net.weightsInputHidden.GetLength(0); i++)
        {
            for (int j = 0; j < net.weightsInputHidden.GetLength(1); j++)
            {
                if (Random.value < mutationsProb)
                {
                    net.weightsInputHidden[i, j] += Random.Range(-mutationsRate, mutationsRate);
                }
            }
        }

        for (int i = 0; i < net.weightsHiddenOutput.GetLength(0); i++)
        {
            for (int j = 0; j < net.weightsHiddenOutput.GetLength(1); j++)
            {
                if (Random.value < mutationsProb)
                {
                    net.weightsHiddenOutput[i, j] += Random.Range(-mutationsRate, mutationsRate);
                }
            }
        }
    }

    //Weist den Spielern die Gehirne zu
    void BrainToPlayer()
    {
        for (int i = 0; i < PlayerManager.instance.GetPlayers().Count; i++)
        {
            if (brains.Count > i)
            {
                PlayerManager.instance.GetPlayers()[i].SetBrain(brains[i]);
            }
            else
            {
                Debug.Log("Not enough Brains");
            }
        }
    }
}
