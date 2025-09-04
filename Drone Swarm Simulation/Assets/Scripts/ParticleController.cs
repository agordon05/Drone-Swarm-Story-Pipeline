using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{

    public float fitness = 0f;
    public float rewardRate = 5f;
    public float Penalty = 100f;
    public bool insideGoal = false;
    public bool isActive = true;

    public float acceleration = 2.5f;   // how strong the thrust is
    public float maxSpeed = 3f;        // top speed
    private Vector2 savedVelocity = Vector2.zero;

    private Rigidbody2D rb;
    private SimulationManager SimManager;
    private ParticleAI ai;
    public GameObject goal;

    private void Awake()
    {
        if (ai == null)
        {
            ai = new ParticleAI();
            ai.Initialize();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // keep particle floating in 2D

        // Grab the first SimulationManager in the scene
        SimManager = FindObjectOfType<SimulationManager>();

        if (SimManager == null)
            Debug.LogError("SimulationManager not found in scene!");

    }

    // Update is called once per frame
    void Update()
    {
        // Simulation is running
        if (isActive)
        {
            if (SimManager.IsRunning())
            {
                // If resuming and we are at rest, reapply saved velocity
                if (rb.velocity == Vector2.zero && savedVelocity != Vector2.zero)
                {
                    rb.velocity = savedVelocity;
                    savedVelocity = Vector2.zero; // clear it so it doesn’t reapply every frame
                }

                InBounds();
                Move();
                Reward();
            }
            // Simulation is not runnint
            else
            {
                // Save velocity and stop motion
                if (rb.velocity != Vector2.zero)
                {
                    savedVelocity = rb.velocity;
                    rb.velocity = Vector2.zero;
                }
            }
        }
        else
        {
            // Save velocity and stop motion
            if (rb.velocity != Vector2.zero)
            {
                savedVelocity = rb.velocity;
                rb.velocity = Vector2.zero;
            }
        }


    }

    void InBounds()
    {
        if (transform.position.x < SimManager.minX || transform.position.x > SimManager.maxX ||
    transform.position.y < SimManager.minY || transform.position.y > SimManager.maxY)
        {
            ai.fitness -= Penalty;
            isActive = false;
            gameObject.SetActive(false);
            //SimManager.EndSimulation();
        }
    }

    // sends variables to ai to decide how to move
    void Move()
    {
        //// Temporary: link to keyboard input
        //float moveX = Input.GetAxis("Horizontal"); // A/D or Left/Right
        //float moveY = Input.GetAxis("Vertical");   // W/S or Up/Down

        // Apply acceleration force
        // Vector2 moveDir = new Vector2(moveX, moveY).normalized;
        Vector2 position = transform.position;
        
        Vector2 goalPosition = goal.transform.position;


        Vector2 moveDir = ai.Decide(position, goalPosition);
        rb.AddForce(moveDir * acceleration);

        // Clamp maximum speed
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    
    }

    void Reward()
    {
        if (insideGoal && isActive)
        {
            ai.fitness += rewardRate * Time.deltaTime;
        }
    }

    public void ResetFitness()
    {
        ai.ResetFitness();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Goal"))
        {
            insideGoal = true;
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Goal"))
        {
            insideGoal = false;
        }
    }

    public bool SetAIWeights(float[] w)
    {
        if(w != null && w.Length == 4)
        {
            return ai.SetWeights(w);
        }

        return false;
    }

    public float[] GetAIWeights()
    {
        return ai.GetWeights();
    }

    public float GetFitness()
    {
        return ai.fitness;
    }

    public void SetGoal(GameObject goal)
    {
        this.goal = goal;
    }
    public ParticleAI GetAI()
    {
        return ai;
    }
    public void SetAI(ParticleAI ai)
    {
        if(ai != null)
            this.ai = ai;
    }

}
