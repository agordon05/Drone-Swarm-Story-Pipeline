using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ParticleAI
{
    public float fitness = 0f;
    private float[] weights;

    public void Initialize()
    {
        // 4 weights: two for X, two for Y
        weights = new float[4];
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] = Random.Range(-1f, 1f);
        }
    }

    public bool SetWeights(float[] w)
    {
        if (w.Length == 4)
        {
            this.weights = w;
            return true;
        }

        Debug.Log("Unable to set weights, weight array must be of length 4");
        return false;
    }

    public float[] GetWeights()
    {
        return this.weights;
    }

    // Decide which way to move
    public Vector2 Decide(Vector2 position, Vector2 goal, float[] sensors = null)
    {

        // (dx, dy) is a vector showing where the goal is relative to the particle

        // Distance X - shows where the goal is horizontally, negative value means goal is to the left of the particle
        float dx = goal.x - position.x;
        // Distance Y - shows where the goal is vertically, negaive value means goal is below the particle
        float dy = goal.y - position.y;

        // Weighted decision
        // Simple AI calculation, will advance with project
        float moveX = dx * weights[0] + dy * weights[1];
        float moveY = dx * weights[2] + dy * weights[3];

        // Normalize so it’s always a unit vector
        Vector2 dir = new Vector2(moveX, moveY).normalized;

        return dir;
    }

    // Reward system
    public void AddFitness(float amount)
    {
        fitness += amount;
    }
    public void ResetFitness()
    {
        fitness = 0;
    }

    public void Mutate(float mutationRate)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] += Random.Range(-mutationRate, mutationRate);
            weights[i] = Mathf.Clamp(weights[i], -1f, 1f);
        }
    }

    public ParticleAI Clone()
    {
        ParticleAI clone = new ParticleAI();
        clone.weights = (float[])this.weights.Clone();
        clone.fitness = 0f;
        return clone;
    }

}
