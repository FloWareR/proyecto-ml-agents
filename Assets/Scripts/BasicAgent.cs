using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;


public class MoveToTargetAgent : Agent
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform env;
    [SerializeField] private float moveSpeed;
    private Rigidbody m_Rigidbody; 
    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        //Set initial position & rotation
        transform.localPosition = new Vector3(Random.Range(-4.5f, 4.5f), 0.5f,Random.Range(-4.5f, 4.5f));
        target.localPosition = new Vector3(Random.Range(-4.5f, 4.5f), 0.5f,Random.Range(-4.5f, 4.5f));
        transform.rotation = Quaternion.identity;

        //Set rigidbody to zero 
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((Vector3)m_Rigidbody.velocity); 
        sensor.AddObservation((Vector3)target.localPosition);
        sensor.AddObservation((Vector3)transform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var moveX = actions.ContinuousActions[0];
        var moveZ = actions.ContinuousActions[1];
        var force = new Vector3(moveX, 0, moveZ) * moveSpeed;
        
        m_Rigidbody.AddForce(force);
    }

    public void OnTriggerEnter(Collider collision)
    {
        if (!collision.TryGetComponent(out AgentTarget targetScript)) return;
        AddReward(5f);
        EndEpisode();
        Debug.Log("I DID IT");
    }

    public void OnTriggerExit(Collider collision)
    {
        if (!collision.TryGetComponent(out Wall envTrigger)) return;
        AddReward(-10f);
        EndEpisode();
    }
}