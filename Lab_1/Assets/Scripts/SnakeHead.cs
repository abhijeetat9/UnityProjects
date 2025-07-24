using System.Collections.Generic;
using UnityEngine;

public class SnakeHead : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f; // Movement speed
    [SerializeField] private float turnSpeed = 360f; // Turning speed

    [Header("Snake Body Settings")]
    [SerializeField] private GameObject segmentPrefab;
    [SerializeField] private float segmentSpacing = 0.5f; // Minimum distance between segments
    [SerializeField] private int initialSegments = 3;

    private List<Transform> bodySegments = new List<Transform>();
    private List<Vector3> positionHistory = new List<Vector3>();

    private Vector3 input;

    private void Start()
    {
        // Initialize the snake with the head as the first segment
        bodySegments.Add(transform);

        // Add initial body segments
        for (int i = 0; i < initialSegments; i++)
        {
            AddSegment();
        }
    }

    private void Update()
    {
        GatherInput();
        if (input != Vector3.zero)
        {
            Look();
            UpdateSegments();
        }
    }

    private void FixedUpdate()
    {
        if (input != Vector3.zero)
        {
            Move();
        }
    }

    /// <summary>
    /// Captures player input for movement.
    /// </summary>
    private void GatherInput()
    {
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    /// <summary>
    /// Rotates the snake head towards the input direction in an isometric space.
    /// </summary>
    private void Look()
    {
        var isometricDirection = ToIso(input);
        var targetRotation = Quaternion.LookRotation(isometricDirection, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Moves the snake head forward in the direction it is facing.
    /// </summary>
    private void Move()
    {
        transform.position += transform.forward * input.normalized.magnitude * moveSpeed * Time.fixedDeltaTime;

        // Store the head's position in the history
        positionHistory.Insert(0, transform.position);

        // Limit the history size to prevent memory issues
        int maxHistorySize = bodySegments.Count * Mathf.CeilToInt(segmentSpacing);
        if (positionHistory.Count > maxHistorySize)
        {
            positionHistory.RemoveAt(positionHistory.Count - 1);
        }
    }

    /// <summary>
    /// Updates the position and rotation of the snake's body segments.
    /// </summary>
    private void UpdateSegments()
{
    for (int i = 1; i < bodySegments.Count; i++)
    {
        Transform segment = bodySegments[i];
        int targetIndex = Mathf.Min(i * Mathf.CeilToInt(segmentSpacing), positionHistory.Count - 1);

        if (targetIndex >= 0 && targetIndex < positionHistory.Count)
        {
            Vector3 targetPosition = positionHistory[targetIndex];

            // Smoothly move the segment to its target position
            segment.position = Vector3.Lerp(segment.position, targetPosition, moveSpeed * Time.deltaTime);
            segment.rotation = Quaternion.Lerp(segment.rotation, bodySegments[i - 1].rotation, moveSpeed * Time.deltaTime);
        }
    }
}

    /// <summary>
    /// Converts world input into isometric directions.
    /// </summary>
    private Vector3 ToIso(Vector3 input)
    {
        return new Vector3(input.x + input.z, 0, input.z - input.x);
    }

    /// <summary>
    /// Adds a new segment to the snake body.
    /// </summary>
    public void AddSegment()
    {
        Vector3 spawnPosition = bodySegments[bodySegments.Count - 1].position;

        GameObject newSegment = Instantiate(segmentPrefab, spawnPosition, Quaternion.identity);
        bodySegments.Add(newSegment.transform);

        //newSegment.AddComponent<WobbleEffect>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            AddSegment();
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Obstacle") || other.CompareTag("SnakeBody"))
        {
            Debug.Log("Game Over!");
            // Trigger game-over logic here
        }
    }
}