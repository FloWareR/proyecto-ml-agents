using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

public class MoveToTargetAgent : Agent
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform env;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float airControlFactor; 

    private Rigidbody m_Rigidbody;
    
    //private bool isGrounded = true; 
 

    private void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // Set initial position & rotation
        transform.localPosition = new Vector3(Random.Range(-4.5f, 4.5f), 0.5f, Random.Range(-5.5f, 5.5f));
        target.localPosition = new Vector3(Random.Range(-4.5f, 4.5f), 0.5f, Random.Range(8.5f, 22f));
        transform.rotation = Quaternion.identity;

        // Reset Rigidbody velocities
        m_Rigidbody.velocity = Vector3.zero;
        m_Rigidbody.angularVelocity = Vector3.zero;

        //isGrounded = true; 
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(m_Rigidbody.velocity);
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(transform.localPosition);
        //sensor.AddObservation(isGrounded); 

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var moveRotate = actions.ContinuousActions[0];
        var moveForward = actions.ContinuousActions[1];

        m_Rigidbody.MovePosition(transform.position + transform.forward * (moveForward * moveSpeed * Time.deltaTime));
        transform.Rotate(0f, moveRotate * moveSpeed, 0f, Space.Self);
        

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Wall wallScript))
        {
            AddReward(-0.01f);
        }
        //if (!collision.gameObject.TryGetComponent(out Ground groundScript)) return;
        //{
            //isGrounded = true;
       // }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if(collision.TryGetComponent(out Pit pitScript))
        {
            AddReward(-1.0f);  
            EndEpisode();     
        }
        
        if(collision.TryGetComponent(out Wall wallScript))
        {
            AddReward(-1.0f);  
            EndEpisode();     
        }
        if(collision.TryGetComponent(out SafeZone safeZoneScript))
        {
            AddReward(1.5f);
        }
        
        if (!collision.TryGetComponent(out AgentTarget targetScript)) return;
        AddReward(7.5f);
        EndEpisode();
    }

    public void OnTriggerExit(Collider collision)
    {
        if (!collision.TryGetComponent(out Enviroment enviromentScript)) return;
        AddReward(-15f);
        EndEpisode();
    }

    public void OnTriggerStay(Collider collision)
    {
        if (!collision.TryGetComponent(out DamageZone damageZoneScript)) return;
        AddReward(-0.01f);
    }
}
