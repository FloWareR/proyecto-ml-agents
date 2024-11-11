using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class AgentJump : Agent
{
    public Transform target;
    public Transform[] checkpoints;
    public float moveSpeed = 5f;
    public float turnSpeed = 200f;
    public float jumpForce = 10f;
    public float jumpDistanceThreshold = 10f;
    private Rigidbody rb;

    private int currentCheckpointIndex = 0;
    private bool hasJumped = false;
    private bool isOnPlatform = false;

    public float fallThreshold = -1.0f;
    private float rewardMultiplier = 1.0f;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.localPosition = new Vector3(0, 0.5f, -8);

        currentCheckpointIndex = 0;
        target.localPosition = checkpoints[currentCheckpointIndex].localPosition;
        hasJumped = false;
        isOnPlatform = false;
        rewardMultiplier = 1.0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        sensor.AddObservation(directionToTarget);
        sensor.AddObservation(Vector3.Distance(transform.position, target.position));
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveInput = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float turnInput = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);
        int jumpAction = actions.DiscreteActions[0]; // Acción discreta para el salto (0 = no saltar, 1 = saltar)

        Vector3 forwardMove = transform.forward * moveInput * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + forwardMove);

        float turn = turnInput * turnSpeed * Time.deltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, turn, 0));

        AddReward(-0.1f * Time.deltaTime);

        if (transform.localPosition.y < fallThreshold)
        {
            SetReward(-2f);
            EndEpisode();
        }

        float distanceToTarget = Vector3.Distance(transform.localPosition, target.localPosition);

        // Verificar si está en el checkpoint 5 o 7, dentro de la distancia de salto, y si se dio la acción de salto
        if ((currentCheckpointIndex == 5 || currentCheckpointIndex == 7) &&
            distanceToTarget <= jumpDistanceThreshold && isOnPlatform && !hasJumped)
        {
            if (jumpAction == 1) // Si la acción de salto está activada
            {
                JumpTowardsTarget();
                AddReward(2.0f);
                hasJumped = true;
            }
        }

        AddReward(-distanceToTarget * 0.001f);

        print("Velocity" + rb.velocity);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Vertical");
        continuousActions[1] = Input.GetAxis("Horizontal");

        // Asigna el salto en la heurística
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0; // Presiona espacio para saltar
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Platform"))
        {
            isOnPlatform = true;
            hasJumped = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Platform"))
        {
            isOnPlatform = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            if (currentCheckpointIndex < checkpoints.Length - 1)
            {
                // Asigna una recompensa exponencial usando el multiplicador
                AddReward(1.0f * rewardMultiplier);

                rewardMultiplier *= 1.5f;

                currentCheckpointIndex++;
                target.localPosition = checkpoints[currentCheckpointIndex].localPosition;
                hasJumped = false;
                print("Target reached" + rewardMultiplier);
            }
            else
            {
                // Recompensa adicional al finalizar el último checkpoint
                AddReward(7.0f * rewardMultiplier);
                EndEpisode();
            }
        }
        else if (other.CompareTag("Obstacle"))
        {
            AddReward(-3.0f);
            EndEpisode();
        }
    }

    private void JumpTowardsTarget()
    {
        Vector3 jumpDirection = (target.position - transform.position).normalized;
        rb.AddForce(new Vector3(jumpDirection.x, 1.5f, jumpDirection.z) * jumpForce, ForceMode.Impulse);
    }
}
