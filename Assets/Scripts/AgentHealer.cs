using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class HealerAgent : Agent
{
    public float healingAmount = 10f;
    public float moveSpeed = 3f;
    public float healingRange = 2f;
    
    public TargetManager targetManager;  // Assign TargetManager in the Inspector

    public override void Initialize()
    {
        if (targetManager == null)
        {
            Debug.LogError("TargetManager is not assigned in the Inspector!");
        }
    }

    public override void OnEpisodeBegin()
    {
        // Optionally reset the health of all targets at the beginning of each episode
        foreach (var targetData in targetManager.targets)
        {
            targetData.currentHP = targetData.maxHP;
            Debug.Log(targetData.currentHP);
        }
        
        transform.localPosition = new Vector3(0, 0.5f, 0);  // Reset agent position
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);
        
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
        // Agent actions: select target and decide to move or heal
        int targetIndex = Mathf.FloorToInt(actions.DiscreteActions[0]);
        int moveOrHeal = Mathf.FloorToInt(actions.DiscreteActions[1]);

        if (targetIndex < 0 || targetIndex >= targetManager.targets.Count)
            return;

        var targetData = targetManager.targets[targetIndex];

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
        // Optional: provide heuristic control for testing
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0; // Select the first target by default
        discreteActionsOut[1] = 0; // Move action
    }
}
