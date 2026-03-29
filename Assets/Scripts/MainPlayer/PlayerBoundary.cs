using UnityEngine;

public class PlayerBoundary : MonoBehaviour
{
    public Transform mapBounds;
    [SerializeField] private Vector2 boundaryPadding = Vector2.zero;

    private BoxCollider2D boundsCollider;

    private void Start()
    {
        if (mapBounds != null)
        {
            boundsCollider = mapBounds.GetComponent<BoxCollider2D>();
        }
    }

    private void LateUpdate()
    {
        if (boundsCollider == null)
        {
            return;
        }

        Bounds map = boundsCollider.bounds;
        Vector3 position = transform.position;

        float minX = map.min.x + boundaryPadding.x;
        float maxX = map.max.x - boundaryPadding.x;
        float minY = map.min.y + boundaryPadding.y;
        float maxY = map.max.y - boundaryPadding.y;

        if (minX > maxX)
        {
            position.x = map.center.x;
        }
        else
        {
            position.x = Mathf.Clamp(position.x, minX, maxX);
        }

        if (minY > maxY)
        {
            position.y = map.center.y;
        }
        else
        {
            position.y = Mathf.Clamp(position.y, minY, maxY);
        }

        transform.position = position;
    }
}
