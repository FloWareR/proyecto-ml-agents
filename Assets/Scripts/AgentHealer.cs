using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class HealerAgent : Agent
{
    public float healingAmount = 10f;
    public float moveSpeed = 3f;
    public float healingRange = 2f;
    
    public TargetManager targetManager;  // Reference to TargetManager

    private int selectedTargetIndex = 0;  // Default target index

    public override void Initialize()
    {
        if (targetManager == null)
        {
            Debug.LogError("TargetManager is not assigned in the Inspector!");
        }
    }

    public override void OnEpisodeBegin()
    {
        foreach (var targetData in targetManager.targets)
        {
            targetData.currentHP = targetData.maxHP;
        }
        
        transform.position = new Vector3(0f, 0.4f, 0f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        foreach (var targetData in targetManager.targets)
        {
            Vector3 relativePosition = targetData.position - transform.position;
            sensor.AddObservation(relativePosition.x);
            sensor.AddObservation(relativePosition.z);
            sensor.AddObservation(targetData.NormalizedHP);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int targetIndex = actions.DiscreteActions[0]; // Select target index
        int moveOrHeal = actions.DiscreteActions[1];  // Move (0) or heal (1)
        // Ensure the target index is valid
        if (targetIndex < 0 || targetIndex >= targetManager.targets.Count) return;

        // Retrieve the selected target's data
        var targetData = targetManager.targets[targetIndex];

        // Execute the chosen action
        if (moveOrHeal == 0)
        {
            MoveTowardsTarget(targetData);
        }
        else if (moveOrHeal == 1 && Vector3.Distance(transform.position, targetData.position) <= healingRange)
        {
            HealTarget(targetData);
        }
    }

    private void MoveTowardsTarget(FriendlyTargetData targetData)
    {
        Vector3 direction = (targetData.position - transform.position).normalized;
        transform.position += direction * (moveSpeed * Time.deltaTime);
    }

    private void HealTarget(FriendlyTargetData targetData)
    {
        targetData.currentHP = Mathf.Min(targetData.maxHP, targetData.currentHP + healingAmount);
        AddReward(1.0f);  // Reward for healing
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;

        // **Target Selection** using 1, 2, 3 keys
        if (Input.GetKey(KeyCode.Alpha1)) selectedTargetIndex = 0;
        if (Input.GetKey(KeyCode.Alpha2)) selectedTargetIndex = 1;
        if (Input.GetKey(KeyCode.Alpha3)) selectedTargetIndex = 2;

        // Set the target index based on selection
        discreteActionsOut[0] = selectedTargetIndex;

        // **Move or Heal** based on W and S keys
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[1] = 0;  // Move action
        }
        else if (Input.GetKey(KeyCode.S))
        {
            discreteActionsOut[1] = 1;  // Heal action
        }
    }
}
