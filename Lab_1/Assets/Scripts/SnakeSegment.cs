using UnityEngine;

public class SnakeSegment : MonoBehaviour
{
    public Transform target;
    public float smoothTime = 0.2f;

    private Vector3 velocity = Vector3.zero;

    void Update()
    {
        //Vector3 targetPosition = target.TransformPoint(new Vector3(0, 0, 0));
        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, smoothTime);
    }   

}