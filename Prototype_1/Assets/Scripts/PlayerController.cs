using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private float carSpeed = 20f;
    private float turnSpeed = 45f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //move vehicle forward
        //transform.Translate(0, 0, 1);
        transform.Translate(Vector3.forward * Time.deltaTime * carSpeed * Input.GetAxis("Vertical"));
        //rotate vehicle
        transform.Rotate(Vector3.up, Time.deltaTime * turnSpeed * Input.GetAxis("Horizontal"));
        
    }
}
