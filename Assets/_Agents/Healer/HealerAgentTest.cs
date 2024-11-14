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
    private int previousAction = -1; // -1 to indicate no action has been taken yet

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
            targetHealth[i] = Random.Range(15f, 100f);
            UpdateHealthDisplay(i);
        }

        // Reset variables for a new episode
        score = 0;
        correctHealMultiplier = 1.0f;
        incorrectHealMultiplier = 1.0f;
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
        sensor.AddObservation(targetTransforms[GetLowestHealthTarget()].localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        int lowestIndex = GetLowestHealthTarget();

        if (action == lowestIndex + 1)
        {
            MoveToTarget(lowestIndex);
            AddReward(0.1f); // Reward for moving to the correct target
        }
        else
        {
            AddReward(-0.05f); // Small penalty for moving to the wrong target
        }

        if (transform.position.y < -0.4f)
        {
            Debug.Log("Agent fell off the map! Ending episode with negative reward.");
            AddReward(-1.0f); // Negative reward for falling off the map
            EndEpisode();
            return;
        }

        bool targetWithZeroHealth = false;
        for (int i = 0; i < targetHealth.Length; i++)
        {
            targetHealth[i] -= healthDecreaseRate * Time.deltaTime;
            targetHealth[i] = Mathf.Max(targetHealth[i], 0);
            UpdateHealthDisplay(i);

            if (targetHealth[i] <= 0)
            {
                targetWithZeroHealth = true;
            }
        }

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
            AddReward(-0.1f);
            return;
        }

        Vector3 direction = (targetTransforms[targetIndex].position - transform.position).normalized;
        transform.position += direction * (moveSpeed * Time.deltaTime);

        float distanceToTarget = Vector3.Distance(transform.position, targetTransforms[targetIndex].position);
        AddReward(0.01f / distanceToTarget); // Reward for getting closer

        if (distanceToTarget < 1.5f)
        {
            HealTarget(targetIndex);
        }
    }

    private void HealTarget(int targetIndex)
    {
        if (targetIndex == GetLowestHealthTarget())
        {
            targetHealth[targetIndex] += Random.Range(15f, 45f);
            targetHealth[targetIndex] = Mathf.Min(targetHealth[targetIndex], 100f);
            Debug.Log($"Healed Target {targetIndex + 1}. New Health: {targetHealth[targetIndex]}");

            score += (int)(5 * correctHealMultiplier);
            correctHealMultiplier += 0.5f;
            UpdatePointsDisplay();

            Debug.Log($"Next target is Target {GetLowestHealthTarget() + 1} with {targetHealth[GetLowestHealthTarget()]} HP");
        }
        else
        {
            Debug.Log("Agent healed the wrong target! Penalizing...");
            score -= (int)(10 * incorrectHealMultiplier);
            incorrectHealMultiplier += 0.5f;
            UpdatePointsDisplay();
        }
    }

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