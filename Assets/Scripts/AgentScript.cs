using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class NavigationAgent : Agent
{
    public Transform target;  // Target the agent should reach
    private InputManager inputManager;  // Reference to your custom InputManager
    private Rigidbody rb;

    private Vector3 initialPosition;
    private Vector3 initialTargetPosition;

    // Initialize the agent
    public override void Initialize()
    {
        inputManager = GetComponent<InputManager>();
        rb = GetComponent<Rigidbody>();

        // Enable agent-controlled input
        inputManager.isAgentControlled = true;

        // Store initial positions for resetting each episode
        initialPosition = transform.localPosition;
        initialTargetPosition = target.localPosition;
    }

    // Reset the agent and environment at the start of each episode
    public override void OnEpisodeBegin()
    {
        // Reset agent's position and velocity
        transform.localPosition = initialPosition;
        rb.velocity = Vector3.zero;

        // Randomize target position within a specific range
        target.localPosition = initialTargetPosition + new Vector3(
            UnityEngine.Random.Range(-10f, 10f),
            0,
            UnityEngine.Random.Range(-10f, 10f)
        );
    }

    // Collect observations for decision-making
    public override void CollectObservations(VectorSensor sensor)
    {
        // Add agent position and velocity observations
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rb.velocity);

        // Direction vector to the target
        Vector3 directionToTarget = (target.localPosition - transform.localPosition).normalized;
        sensor.AddObservation(directionToTarget);
    }

    // Actions received from the neural network during training
    public override void OnActionReceived(ActionBuffers actions)
    {
        // Get actions from agent's neural network output
        float horizontal = actions.ContinuousActions[0];
        float vertical = actions.ContinuousActions[1];
        float cameraX = actions.ContinuousActions[2];
        float cameraY = actions.ContinuousActions[3];

        // Pass the agent's actions to the InputManager to control movement
        inputManager.SetAgentInputs(horizontal, vertical, cameraX, cameraY);

        float objective = Vector3.Distance(this.gameObject.transform.localPosition, target.transform.localPosition);

        // Small penalty for each step to encourage efficient movement
        SetReward(-0.001f);

        // Penalty for falling off the platform (simulating no ground below agent)
        if (transform.localPosition.y < 0)
        {
            SetReward(-3.0f);  // Heavy penalty for falling
            EndEpisode();
        }
        if(objective < 1.42f)
        {
            SetReward(1.0f);  // Large reward for reaching the target
            EndEpisode();
        }
    }

    // Trigger detection for reaching the target
/*    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object is the target
        if (other.CompareTag("Target"))
        {
            SetReward(1.0f);  // Large reward for reaching the target
            EndEpisode();
        }
    }*/

    // Manual control for testing purposes
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal"); // Horizontal input
        continuousActions[1] = Input.GetAxis("Vertical");   // Vertical input
        continuousActions[2] = Input.GetAxis("Mouse X");    // Camera X
        continuousActions[3] = Input.GetAxis("Mouse Y");    // Camera Y
    }
}
