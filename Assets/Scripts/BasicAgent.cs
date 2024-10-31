using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using Random = UnityEngine.Random;

public class MoveToTargetAgent : Agent
{
   [SerializeField] private float moveSpeed;
   [SerializeField] private float jumpForce;
   [SerializeField] private Transform target;
   

   private Rigidbody _rigidbody;
   private bool isGrounded; 
   
   public override void Initialize()
   {
      _rigidbody = GetComponent<Rigidbody>();
   }

   public override void OnEpisodeBegin()
   {
      transform.localPosition = new Vector3(Random.Range(-4.5f, 4.5f), 0.5f, Random.Range(-6f, 4.5f));
      target.localPosition = new Vector3(Random.Range(-4.5f, 4.5f), 0.5f, Random.Range(7f, 19));

      _rigidbody.velocity = Vector3.zero;
      _rigidbody.angularVelocity = Vector3.zero;
      isGrounded = true;
   }

   public override void CollectObservations(VectorSensor sensor)
   {
      sensor.AddObservation(target.localPosition);
      sensor.AddObservation(transform.position);
      sensor.AddObservation(_rigidbody.velocity);
      sensor.AddObservation(transform.rotation);
   }

   public override void OnActionReceived(ActionBuffers actions)
   {
      var moveForward = actions.ContinuousActions[0];
      var moveRotate = actions.ContinuousActions[1];
      
      _rigidbody.MovePosition(transform.position + transform.forward *(moveForward * moveSpeed * Time.deltaTime));
      transform.Rotate(0f, moveRotate * moveSpeed, 0f, Space.Self);
      
      
      AddReward(-0.05f);
   }

   private void OnCollisionEnter(Collision other)
   {
      if (!other.gameObject.TryGetComponent(out Enviroment envScript)) return;

      AddReward(-10);
      EndEpisode();
   }

   private void OnTriggerEnter(Collider other)
   {
      if (!other.TryGetComponent(out AgentTarget targetScript)) return;
      
      AddReward(10f);
      EndEpisode();
   }
}
