using UnityEngine;

public class GradualRotation : MonoBehaviour
{
    public float totalRotation = 90f; 
    public float rotationDuration = .5f;  
    public float interval = 1.2f;         

    private float rotationStep;
    private float timeElapsed;

    private void Start()
    {

        float steps = rotationDuration / Time.fixedDeltaTime;
        rotationStep = totalRotation / steps;

        InvokeRepeating(nameof(StartRotation), interval, interval + rotationDuration);
    }

    private void StartRotation()
    {
        timeElapsed = 0f;
        InvokeRepeating(nameof(ApplyRotationStep), 0f, Time.fixedDeltaTime);
    }

    private void ApplyRotationStep()
    {
        if (timeElapsed < rotationDuration)
        {
            transform.Rotate(0, 0, rotationStep);
            timeElapsed += Time.fixedDeltaTime;
        }
        else
        {
            CancelInvoke(nameof(ApplyRotationStep));
        }
    }
}