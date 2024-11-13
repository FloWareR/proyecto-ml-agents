using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using TMPro;

public class HealthTargetAgent : Agent
{
    public Transform[] targetTransforms; 
    private float[] targetHealth;
    private float healthDecreaseRate = 0.1f;
    private float moveSpeed = 2f;

    public TextMeshPro[] targetHealthTexts;
    public TextMeshPro pointsCounterText;

    private int currentTargetIndex;
    private int score = 0; 
    private float correctHealMultiplier = 1.0f;
    private float incorrectHealMultiplier = 1.0f;
    private int consecutiveIncorrectHeals = 0;

    public override void Initialize()
    {
        targetHealth = new float[targetTransforms.Length];
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = Vector3.zero;

        // Randomize health for each target and update the health display
        for (int i = 0; i < targetHealth.Length; i++)
        {
            targetHealth[i] = Random.Range(10f, 100f);
            UpdateHealthDisplay(i);
        }

        // Reset variables for a new episode
        currentTargetIndex = GetLowestHealthTarget();
        score = 0;
        correctHealMultiplier = 1.0f;
        incorrectHealMultiplier = 1.0f;
        consecutiveIncorrectHeals = 0;
        UpdatePointsDisplay();

        Debug.Log("Episode Started. Agent is looking for the lowest health target.");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        foreach (float health in targetHealth)
        {
            sensor.AddObservation(health);
        }

        sensor.AddObservation(transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];

        // Perform action based on the received action index
        switch (action)
        {
            case 0: // Stay Still
                Debug.Log("Agent Action: Stay Still");
                AddReward(-0.1f); // Small penalty for staying still
                break;

            case 1: // Move to Target 1
                Debug.Log("Agent Action: Move to Target 1");
                MoveToTarget(0);
                break;

            case 2: // Move to Target 2
                Debug.Log("Agent Action: Move to Target 2");
                MoveToTarget(1);
                break;

            case 3: // Move to Target 3
                Debug.Log("Agent Action: Move to Target 3");
                MoveToTarget(2);
                break;

            default:
                Debug.LogWarning("Agent selected an invalid action!");
                AddReward(-0.1f); // Penalize for invalid actions
                return;
        }

        // Check if the agent has fallen off the map (y position < -0.4f)
        if (transform.position.y < -0.4f)
        {
            Debug.Log("Agent fell off the map! Ending episode with negative reward.");
            AddReward(-1.0f); // Negative reward for falling off the map
            EndEpisode();
            return;
        }

        // Gradually decrease health of all targets and update their displays
        bool targetWithZeroHealth = false;
        for (int i = 0; i < targetHealth.Length; i++)
        {
            targetHealth[i] -= healthDecreaseRate * Time.deltaTime;
            targetHealth[i] = Mathf.Max(targetHealth[i], 0);
            UpdateHealthDisplay(i);

            // If any target's health reaches 0, mark this and end the episode
            if (targetHealth[i] <= 0)
            {
                targetWithZeroHealth = true;
            }
        }

        // End the episode only if any target's health reached 0
        if (targetWithZeroHealth)
        {
            Debug.Log("A target's health reached 0. Ending episode.");
            EndEpisode();
        }
    }


    private void MoveToTarget(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= targetTransforms.Length)
        {
            Debug.LogWarning("Invalid target index selected by agent.");
            AddReward(-0.1f); // Penalize for invalid target index
            return;
        }

        Vector3 direction = (targetTransforms[targetIndex].position - transform.position).normalized;
        transform.position += direction * (moveSpeed * Time.deltaTime);

        float distanceToTarget = Vector3.Distance(transform.position, targetTransforms[targetIndex].position);
        if (distanceToTarget < 1.0f)
        {
            Debug.Log($"Agent reached Target {targetIndex + 1}");

            // Check if this is the correct target (lowest health)
            if (targetIndex == GetLowestHealthTarget())
            {
                // Heal the target by 50 HP
                targetHealth[targetIndex] += 50; 
                targetHealth[targetIndex] = Mathf.Min(targetHealth[targetIndex], 100f); // Cap at 100 HP
                Debug.Log($"Healed Target {targetIndex + 1}. New Health: {targetHealth[targetIndex]}");

                score += (int)(5 * correctHealMultiplier); // 5 points for correct heal with multiplier
                correctHealMultiplier += 0.5f; // Increase multiplier for consecutive correct heals
                UpdatePointsDisplay();

                // After healing the correct target, move on to the next lowest health target
                currentTargetIndex = GetLowestHealthTarget();
                consecutiveIncorrectHeals = 0; // Reset incorrect heal count

                Debug.Log($"Next target is Target {currentTargetIndex + 1} with {targetHealth[currentTargetIndex]} HP");
            }
            else
            {
                Debug.Log("Agent healed the wrong target! Penalizing...");
                score -= (int)(10 * incorrectHealMultiplier); // 10 points penalty for wrong heal with multiplier
                incorrectHealMultiplier += 0.5f; // Increase penalty multiplier for consecutive wrong heals
                UpdatePointsDisplay();
            }
        }
    }

    // Get the index of the target with the lowest health
    private int GetLowestHealthTarget()
    {
        int lowestIndex = 0;
        float lowestHealth = targetHealth[0];

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

    // Method to update the TextMeshPro component with current health
    private void UpdateHealthDisplay(int index)
    {
        if (targetHealthTexts != null && targetHealthTexts.Length > index)
        {
            targetHealthTexts[index].text = $"Health: {targetHealth[index]:F1}";
        }
    }

    // Method to update the points counter display
    private void UpdatePointsDisplay()
    {
        if (pointsCounterText != null)
        {
            pointsCounterText.text = $"Points: {score}";
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.Alpha0))
        {
            discreteActions[0] = 0; // Stay Still
        }
        else if (Input.GetKey(KeyCode.Alpha1))
        {
            discreteActions[0] = 1; // Move to Target 1
        }
        else if (Input.GetKey(KeyCode.Alpha2))
        {
            discreteActions[0] = 2; // Move to Target 2
        }
        else if (Input.GetKey(KeyCode.Alpha3))
        {
            discreteActions[0] = 3; // Move to Target 3
        }
    }
}
