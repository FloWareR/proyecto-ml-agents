using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class WalkingAgent : Agent
{
    public Transform[] targets; // Array of target GameObjects
    public float moveSpeed = 10f; // Force applied for movement
    public float maxStepTime = 200f; // Steps allowed to reach each target

    private int currentTargetIndex = 0; // Index of the current target
    private float stepsTaken = 0f; // Counter for steps taken
    private Rigidbody rb; // Reference to the agent's Rigidbody

    public override void Initialize()
    {
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // Reset the agent's position and rotation
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.localPosition = new Vector3(0f, 0.5f, 0f);
        transform.rotation = Quaternion.identity;

        // Choose the first target and reset steps
        currentTargetIndex = 0;
        stepsTaken = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add the agent's position and the position of the current target as observations
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targets[currentTargetIndex].localPosition);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Apply continuous force for movement
        Vector3 moveDirection = new Vector3(actionBuffers.ContinuousActions[0], 0, actionBuffers.ContinuousActions[1]);
        rb.AddForce(moveDirection * moveSpeed, ForceMode.VelocityChange);

        // Increment steps taken
        stepsTaken += 1f;

        // Apply small time penalty for each step
        AddReward(-0.001f);

        // End episode if taking too long
        if (stepsTaken >= maxStepTime)
        {
            AddReward(-0.5f); // Penalty for inefficiency
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the agent collided with the current target
        if (collision.transform == targets[currentTargetIndex])
        {
            Debug.Log("LOL");
            AddReward(1.0f); // Reward for reaching the target
            stepsTaken = 0f; // Reset step count for the next target

            // Move to the next target
            currentTargetIndex = (currentTargetIndex + 1) % targets.Length;
        }
        // Check if the agent collided with a wall
        else if (collision.transform.CompareTag("Wall"))
        {
            Debug.Log("Lose points");

            AddReward(-0.5f); // Penalty for colliding with a wall
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Define manual control for testing
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }
}
