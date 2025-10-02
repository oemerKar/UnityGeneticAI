using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNetwork 
{

    public int inputsize, hiddensize, outputsize;

    public float[,] weightsInputHidden, weightsHiddenOutput;

    public float fitness = 0f;


    public NeuralNetwork(int input, int hidden, int output)
    {
        inputsize = input; hiddensize = hidden; outputsize = output;

        weightsInputHidden = new float[input, hidden];
        weightsHiddenOutput = new float[hidden, output];

        RandomizeWeights();
    }

    public NeuralNetwork(NeuralNetwork copy)
    {
        inputsize = copy.inputsize;
        hiddensize = copy.hiddensize;
        outputsize = copy.outputsize;

        weightsInputHidden = (float[,])copy.weightsInputHidden.Clone();
        weightsHiddenOutput = (float[,])copy.weightsHiddenOutput.Clone();
        fitness = copy.fitness;
    }

    void RandomizeWeights()
    {
        for(int i = 0; i< inputsize; i++)
        {
            for (int j = 0; j < hiddensize; j++)
            {
                weightsInputHidden[i, j] = Random.Range(-1f, 1f);
            }
        }
        for (int i = 0; i < hiddensize; i++)
        {
            for (int j = 0; j < outputsize; j++)
            {
                weightsHiddenOutput[i, j] = Random.Range(-1f, 1f);
            }
        }
    }

    public float[] FeedForward(float[] input)
    {
        float[] hidden = new float[hiddensize];
        float[] output = new float[outputsize];


        //From Input to Hidden
        for (int i = 0; i < hiddensize; i++)
        {
            float sum = 0;

            for (int j = 0; j < inputsize; j++)
            {
                sum += input[j] * weightsInputHidden[j, i];
            }
            hidden[i] = Sigmoid(sum);
        }

        //From Hidden to Output
        for (int i = 0; i < outputsize; i++)
        {
            float sum = 0;

            for (int j = 0; j < hiddensize; j++)
            {
                sum += hidden[j] * weightsHiddenOutput[j, i];
            }
            output[i] = Sigmoid(sum);
        }

        return output;
    }

    float Sigmoid(float x)
    {
        return 1f / (1f + Mathf.Exp(-x));
    }

    public void SetFitness(float fit)
    {
        fitness = fit;
    }

    
}
