using UnityEngine;

public class PropellorRotation : MonoBehaviour
{
    private float rotationSpeed = 200f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);   
    }
}
