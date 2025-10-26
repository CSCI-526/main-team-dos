using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    public enum Direction { Up, Down, Left, Right }
    public Direction moveDirection = Direction.Up;

    [Tooltip("Distance the platform travels from its start position.")]
    public float moveDistance = 3f;

    [Tooltip("Movement speed of the platform.")]
    public float speed = 2f;

    private Vector3 startPos;
    private Vector3 endPos;
    private bool movingToEnd = true;

    void Start()
    {
        startPos = transform.position;
        SetEndPosition();
    }

    void Update()
    {
        // Move between start and end positions
        transform.position = Vector3.MoveTowards(
            transform.position,
            movingToEnd ? endPos : startPos,
            speed * Time.deltaTime
        );

        // When close enough to either point, reverse direction
        if (Vector3.Distance(transform.position, movingToEnd ? endPos : startPos) < 0.05f)
        {
            movingToEnd = !movingToEnd;
        }
    }

    void SetEndPosition()
    {
        switch (moveDirection)
        {
            case Direction.Up:
                endPos = startPos + Vector3.up * moveDistance;
                break;
            case Direction.Down:
                endPos = startPos + Vector3.down * moveDistance;
                break;
            case Direction.Left:
                endPos = startPos + Vector3.left * moveDistance;
                break;
            case Direction.Right:
                endPos = startPos + Vector3.right * moveDistance;
                break;
        }
    }

    // Optional: visualize in the Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 endPreview = Vector3.zero;
        switch (moveDirection)
        {
            case Direction.Up:
                endPreview = transform.position + Vector3.up * moveDistance;
                break;
            case Direction.Down:
                endPreview = transform.position + Vector3.down * moveDistance;
                break;
            case Direction.Left:
                endPreview = transform.position + Vector3.left * moveDistance;
                break;
            case Direction.Right:
                endPreview = transform.position + Vector3.right * moveDistance;
                break;
        }
        Gizmos.DrawLine(transform.position, endPreview);
        Gizmos.DrawSphere(endPreview, 0.1f);
    }
}
