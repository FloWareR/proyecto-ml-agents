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

    private float healRange = 2.0f;

    public override void Initialize()
    {
        targetHealth = new float[targetTransforms.Length];
    }

    public override void OnEpisodeBegin()
    {
        // Reset agent position
        transform.localPosition = Vector3.zero;

        // Randomize health for each target
        for (int i = 0; i < targetHealth.Length; i++)
        {
            targetHealth[i] = Random.Range(15f, 100f);
            UpdateHealthDisplay(i);
        }

        // Reset variables
        score = 0;
        correctHealMultiplier = 1.0f;
        incorrectHealMultiplier = 1.0f;
        UpdatePointsDisplay();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        foreach (var target in targetTransforms)
        {
            sensor.AddObservation(target.localPosition);
        }
        foreach (var health in targetHealth)
        {
            sensor.AddObservation(health / 100f); // Normalize health
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        ActionSegment<float> continuousActions = actions.ContinuousActions;
        float moveX = continuousActions[0];
        float moveZ = continuousActions[1];

        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // Reward for moving closer to the target with the lowest health
        int lowestIndex = GetLowestHealthTarget();
        float distanceToLowest = Vector3.Distance(transform.position, targetTransforms[lowestIndex].position);

        if (distanceToLowest < healRange)
        {
            HealTarget(lowestIndex);
        }

        AddReward(-0.01f); // Small penalty for time taken
    }

    private void HealTarget(int targetIndex)
    {
        float healAmount = Random.Range(15f, 50f);
        targetHealth[targetIndex] = Mathf.Min(targetHealth[targetIndex] + healAmount, 100f);
        score += (int)(10 * correctHealMultiplier);
        AddReward(1.0f);
        UpdateHealthDisplay(targetIndex);
        UpdatePointsDisplay();
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
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }
}
