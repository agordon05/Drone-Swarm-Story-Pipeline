using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SimulationManager : MonoBehaviour
{

    // Wall Boundaries
    public float minX = -9; // Left
    public float maxX = 9; // Right
    public float minY = -5; // Down
    public float maxY = 5; // Up

    // Simulation Parameters
    public float timer = 0f;
    public float stopTime = 10f;
    public int NumOfParticles = 10;
    public int NumOfGoals = 1;
    private int Generation = 0;

    //prefabs
    public GameObject ParticlePrefab;
    public GameObject GoalPrefab;

    private GameObject[] Particles = new GameObject[0];
    private GameObject[] Goals = new GameObject[0];
    private ParticleAI[] AIPopulation = new ParticleAI[0];


    private bool isRunning = false;

    public UnityEngine.UI.Slider speedSlider; // assigned in Inspector
    public TextMeshProUGUI speedText; // assign this in Inspector



    // Start is called before the first frame update
    void Start()
    {
        StartSimulation();
    }

    // Update is called once per frame
    void Update()
    {
        if (isRunning)
        {
            StopWatch();
            Time.timeScale = speedSlider.value;
            speedText.text = "Time Scale: " + speedSlider.value.ToString("F2"); // two decimal places
        }
        if (AllParticlesDead())
        {
            EndSimulation();
        }
    }

    bool AllParticlesDead()
    {
        for(int i = 0; i < Particles.Length; i++)
        {
            if (Particles[i].activeSelf) return false;
        }
        return true;
    }

    public int GetGeneration()
    {
        return Generation;
    }

    public void StartSimulation()
    {
        CreateGoals();
        CreateParticles();
        SetSimulation();

        timer = 0f;
        isRunning = true;
        Debug.Log("Simulation started");

    }

    public void EndSimulation()
    {


        isRunning = false;
        PrintFitness();

        Debug.Log("Simulation ended. Duration: " + timer);
        // TODO: Evaluate fitness, reset particles, prepare next round


        // Score particles
        List<(ParticleAI, float)> scores = GetScores();
        scores.Sort((a, b) => b.Item2.CompareTo(a.Item2));
        Debug.Log("Best fitness: " + scores[0].Item2);

        Generation++;
        Evolve(scores);
        StartSimulation();

    }

    private List<(ParticleAI, float)> GetScores()
    {
        // Collect fitness scores
        List<(ParticleAI, float)> scored = new List<(ParticleAI, float)>();
        foreach (var pc in Particles)
        {
            ParticleController PCon = pc.GetComponent<ParticleController>();
            scored.Add((PCon.GetAI(), PCon.GetFitness()));
        }
        return scored;
    }

    public void PauseSimulation()
    {
        isRunning = false;
        Time.timeScale = 0;
    }

    public void ResumeSimulation()
    {
        isRunning = true;
        Time.timeScale = speedSlider.value;
    }

    void StopWatch()
    {
        if(timer < stopTime)
        {
            timer += Time.deltaTime;
        }
        else
        {
            EndSimulation();
        }
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public float GetTime()
    {
        return timer;
    }

    private void CreateParticles()
    {
        // Destroy existing particles if any
        if (Particles != null)
        {
            foreach (var p in Particles)
            {
                if (p != null) Destroy(p);
            }
        }

        Particles = new GameObject[NumOfParticles];

        for (int i = 0; i < NumOfParticles; i++)
        {
            // Get a valid spawn position
            Vector2 spawnPos = GetValidSpawnPosition();

            // Instantiate particle
            Particles[i] = Instantiate(ParticlePrefab, spawnPos, Quaternion.identity);

            // Assign AI
            if (AIPopulation.Length == 0)
            {
                // First generation: initialize AIPopulation
                AIPopulation = new ParticleAI[NumOfParticles];
                AIPopulation[i] = Particles[i].GetComponent<ParticleController>().GetAI();
            }
            else
            {
                // Subsequent generations: assign evolved AI
                Particles[i].GetComponent<ParticleController>().SetAI(AIPopulation[i]);
            }
        }
    }

    // Helper to get a spawn position not too close to the goal
    private Vector2 GetValidSpawnPosition()
    {
        Vector2 spawnPos;
        Vector2 goalPos = Goals[0].transform.position; // assumes one goal
        const float minDistance = 1.0f;

        do
        {
            spawnPos = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        } while (Vector2.Distance(spawnPos, goalPos) <= minDistance);

        return spawnPos;
    }



    private void CreateGoals()
    {
        if (Goals != null)
        {
            foreach (var g in Goals)
                if (g != null) Destroy(g);
        }
        Goals = new GameObject[NumOfGoals];
        for (int i = 0; i < NumOfGoals; i++)
        {
            // Spawn at random position inside your simulation area
            Vector2 spawnPos = new Vector2(Random.Range(minX - 1, maxX - 1), Random.Range(minY - 1, maxY - 1));

            Goals[i] = Instantiate(GoalPrefab, spawnPos, Quaternion.identity);
        }
    }

    // this method assigns positions to both particles and goals, it also assigns each particle with a specific goal
    private void SetSimulation()
    {
        // Current setup is only one goal -- will advance later
        for(int i = 0; i < Particles.Length; i++)
        {
            Particles[i].GetComponent<ParticleController>().SetGoal(Goals[0]);
        }

    }

    private void PrintFitness()
    {
        for(int i = 0; i < Particles.Length; i++)
        {

            // get weights from particle
            string weightString = "";
            ParticleController Pcontroller = Particles[i].GetComponent<ParticleController>();
            float[] weights = Pcontroller.GetAIWeights();
            for(int i2 = 0; i2 < weights.Length; i2++)
            {
                if(i2 == weights.Length - 1)
                {
                    weightString += weights[i2];
                }
                else
                {
                    weightString += weights[i2] + ", ";
                }
            }


            Debug.Log("Particle " + i + ": Weights - " + weightString + " || Fitness Value: " + Pcontroller.GetFitness());
        }
    }


    private void Evolve(List<(ParticleAI, float)> scored)
    {
        // Reset AIPopulation List
        AIPopulation = new ParticleAI[NumOfParticles];
        int eliteCount = Mathf.CeilToInt(NumOfParticles * 0.2f);


        for (int i = 0; i < NumOfParticles; i++)
        {
            // Retrieves a new Particle AI with the weights from the top 20%
            ParticleAI pAI = scored[i].Item1.Clone();

            if (i >= eliteCount)
            {
                // Child: pick random elite, clone, and mutate
                ParticleAI parent = scored[Random.Range(0, eliteCount)].Item1;
                pAI = parent.Clone();

                //// Clone parent's weights
                pAI.Mutate(0.1f);  
            }

            // Add ParticleAI to AIPopulation
            AIPopulation[i] = pAI;
        }
    }


}
