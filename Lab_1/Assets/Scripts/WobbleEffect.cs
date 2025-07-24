using UnityEngine;

public class WobbleEffect : MonoBehaviour
{
    public float amplitude = 0.1f; // Wobble amplitude
    public float frequency = 1f; // Wobble frequency
    public Vector3 wobbleAxis = Vector3.up; // Wobble axis
    private Vector3 originalPosition;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originalPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float wobble = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.localPosition = originalPosition + wobbleAxis * wobble;
    }
}
