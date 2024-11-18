using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using TMPro;

public class HealthTargetAgent : Agent
{
    public Transform[] targetTransforms;
    private float[] targetHealth;
    private float healthDecreaseRate = 3.5f;
    private float moveSpeed = 9.5f;

    public TextMeshPro[] targetHealthTexts;
    public TextMeshPro pointsCounterText;

    private int score = 0; 
    private float correctHealMultiplier = 1.0f;
    private float incorrectHealMultiplier = 1.0f;

    public GameObject objective; // The "Objective" GameObject that the agent follows

    private Vector3 previousPosition; // Store the previous position of the agent
    private float previousDistanceToObjective; // Track previous distance to objective

    public override void Initialize()
    {
        targetHealth = new float[targetTransforms.Length];
        if (objective == null)
        {
            Debug.LogError("Objective GameObject is not assigned!");
        }

        previousPosition = transform.position; // Initialize previous position
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = Vector3.zero;

        // Randomize health for each target and update the health display
        for (int i = 0; i < targetHealth.Length; i++)
        {
            targetHealth[i] = Random.Range(15f, 100f);
            UpdateHealthDisplay(i);
        }

        // Reset variables for a new episode
        score = 0;
        correctHealMultiplier = 1.0f;
        incorrectHealMultiplier = 1.0f;
        UpdatePointsDisplay();

        // Track the initial distance to the objective
        previousDistanceToObjective = Vector3.Distance(transform.position, objective.transform.position);
        previousPosition = transform.position;

        Debug.Log("Episode Started. Agent is looking for the lowest health target.");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(objective.transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int lowestIndex = GetLowestHealthTarget();

        // Move the objective towards the lowest health target
        if (objective != null)
        {
            var direction = (targetTransforms[lowestIndex].position - objective.transform.position).normalized;
            objective.transform.position += direction * (moveSpeed * Time.deltaTime);
        }

        // Check if the agent has fallen off the map
        if (transform.position.y < -0.4f)
        {
            Debug.Log("Agent fell off the map! Ending episode with negative reward.");
            AddReward(-1.0f); // Negative reward for falling off the map
            EndEpisode();
            return;
        }

        // Read continuous movement actions from the agent
        ActionSegment<float> continuousActions = actions.ContinuousActions;
        float moveX = continuousActions[0]; // Horizontal movement input (X-axis)
        float moveZ = continuousActions[1]; // Vertical movement input (Z-axis)

        // Create movement direction based on agent's input
        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        // Apply movement to the agent
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // Penalize if the agent is not moving
        if (moveDirection.magnitude == 0)
        {
            AddReward(-0.1f); 
        }

        // Check if the agent is moving away from the objective
        float currentDistanceToObjective = Vector3.Distance(transform.position, objective.transform.position);
        if (currentDistanceToObjective > previousDistanceToObjective)
        {
            // Penalize for moving further away from the objective
            AddReward(-0.05f); // You can adjust the penalty here as needed
        }
        else
        {
            // Reward for moving closer to the objective
            AddReward(0.01f); // Adjust reward based on how much reward you want
        }

        // Update the previous distance to the objective and the position
        previousDistanceToObjective = currentDistanceToObjective;

        // Update target health and check for healing
        bool targetWithZeroHealth = false;
        for (int i = 0; i < targetHealth.Length; i++)
        {
            targetHealth[i] -= healthDecreaseRate * Time.deltaTime;
            targetHealth[i] = Mathf.Max(targetHealth[i], 0);
            UpdateHealthDisplay(i);

            // Check if a target's health has reached zero
            if (targetHealth[i] <= 0)
            {
                targetWithZeroHealth = true;
            }

            // Heal the target if the agent is close enough to it
            if (Vector3.Distance(transform.position, targetTransforms[i].position) < 2.0f) // 2 units as healing range
            {
                HealTarget(i); // Heal target if close enough
            }
        }

        // End the episode if any target's health reaches zero
        if (targetWithZeroHealth)
        {
            Debug.Log("A target's health reached 0. Ending episode.");
            EndEpisode();
        }
    }

    private void HealTarget(int targetIndex)
    {
        int lowestIndex = GetLowestHealthTarget();
        
        // Check if the agent is healing the correct target (lowest health)
        if (targetIndex == lowestIndex)
        {
            // Randomize heal amount between 15 and 50
            float healAmount = Random.Range(15f, 50f); 
            targetHealth[targetIndex] = Mathf.Min(targetHealth[targetIndex] + healAmount, 100f); // Prevent overhealing
            Debug.Log($"Healed Target {targetIndex + 1}. New Health: {targetHealth[targetIndex]}");

            // Reward for healing the correct target
            score += (int)(5 * correctHealMultiplier); // Adjust reward based on the correct healing multiplier
            correctHealMultiplier += 0.5f; // Increase multiplier for future healing actions
            AddReward(1.0f); // Add a reward for healing the correct target
            UpdatePointsDisplay();

            Debug.Log($"Next target is Target {GetLowestHealthTarget() + 1} with {targetHealth[GetLowestHealthTarget()]} HP");
        }
        else
        {
            // Penalize for healing the wrong target
            Debug.Log("Agent healed the wrong target! Penalizing...");
            score -= (int)(10 * incorrectHealMultiplier); // Decrease score for incorrect healing
            incorrectHealMultiplier += 0.5f; // Increase penalty multiplier for incorrect actions
            AddReward(-0.5f); // Negative reward for healing the wrong target
            UpdatePointsDisplay();
        }
    }

    private int GetLowestHealthTarget()
    {
        int lowestIndex = 0;
        float lowestHealth = targetHealth[0];

        // Find the target with the lowest health
        for (int i = 1; i < targetHealth.Length; i++)
        {
            if (targetHealth[i] < lowestHealth)
            {
                lowestHealth = targetHealth[i];
                lowestIndex = i;
            }
        }
        return lowestIndex;
    }

    private void UpdateHealthDisplay(int index)
    {
        if (targetHealthTexts != null && targetHealthTexts.Length > index)
        {
            targetHealthTexts[index].text = $"Health: {targetHealth[index]:F1}";
        }
    }

    private void UpdatePointsDisplay()
    {
        if (pointsCounterText != null)
        {
            pointsCounterText.text = $"Points: {score}";
        }
    }

    // Heuristic method to control movement with WASD/arrow keys
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;

        // Get the horizontal (X) and vertical (Z) input (using Unity's default input axes)
        float moveX = Input.GetAxis("Horizontal"); // -1 to 1
        float moveZ = Input.GetAxis("Vertical");   // -1 to 1

        // Set the movement values in the continuous actions buffer
        continuousActions[0] = moveX; // Horizontal input for X axis
        continuousActions[1] = moveZ; // Vertical input for Z axis
    }
}
