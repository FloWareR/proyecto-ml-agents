using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class GapJumpAgent : Agent
{
    public Transform target; // El objetivo al que debe dirigirse el agente
    public Transform[] checkpoints;
    public float moveSpeed = 5f;
    public float turnSpeed = 200f;
    private Rigidbody rb;

    private int isTargetReached = 0;

    // Altura mínima antes de considerar que el agente ha caído de la plataforma
    public float fallThreshold = -1.0f;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // Reiniciar posición, velocidad y rotación del agente y del objetivo
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.localPosition = new Vector3(0, 0.5f, -8);

        // Coloca el objetivo en la primera posición
        target.localPosition = checkpoints[0].localPosition;
        isTargetReached = 0;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Observaciones relacionadas con la posición del objetivo
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        sensor.AddObservation(directionToTarget); // Dirección hacia el objetivo
        sensor.AddObservation(Vector3.Distance(transform.position, target.position)); // Distancia al objetivo
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Control de movimiento basado en acciones del modelo
        float moveInput = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f); // Valor de avance/retroceso
        float turnInput = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f); // Valor de rotación

        // Movimiento del agente usando Rigidbody
        Vector3 forwardMove = transform.forward * moveInput * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + forwardMove);

        // Rotación del agente usando Rigidbody
        float turn = turnInput * turnSpeed * Time.deltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0, turn, 0));

        // Penalización por el tiempo que pasa (0.01 por segundo)
        AddReward(-0.01f * Time.deltaTime);

        // Verificar si el agente ha caído de la plataforma
        if (transform.localPosition.y < fallThreshold)
        {
            // Penalización por caer de la plataforma
            SetReward(-5f);
            EndEpisode(); // Terminar el episodio
        }

        float distanceToTarget = Vector3.Distance(transform.localPosition, target.localPosition);
        AddReward(-distanceToTarget * 0.001f);

        print("Target checkpoint: " + isTargetReached);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Control manual para pruebas
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Vertical");
        continuousActions[1] = Input.GetAxis("Horizontal");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            if (isTargetReached == 0)
            {
                AddReward(1.0f);
                target.localPosition = checkpoints[1].localPosition;
                isTargetReached = 1;
            }
            else if (isTargetReached == 1)
            {
                AddReward(2.0f);
                target.localPosition = checkpoints[2].localPosition;
                isTargetReached = 2;
            }
            else if (isTargetReached == 2)
            {
                AddReward(4.0f);
                target.localPosition = checkpoints[3].localPosition;
                isTargetReached = 3;
            }
            else if (isTargetReached == 3)
            {
                AddReward(7.0f);
                target.localPosition = checkpoints[3].localPosition;
                EndEpisode();
            }
        }
        else if (other.CompareTag("Obstacle"))
        {
            // Penalización por chocar con un obstáculo
            AddReward(-3.0f);
            EndEpisode();
        }
    }
}
