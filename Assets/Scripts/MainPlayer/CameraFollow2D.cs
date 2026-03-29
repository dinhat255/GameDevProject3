using UnityEngine;

[DefaultExecutionOrder(10000)]
public class CameraFollow2D : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;
    [SerializeField] private Transform mapBounds;

    [Header("Follow")]
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 0f, -10f);
    [SerializeField] private float followSpeed = 12f;
    [SerializeField] private bool smoothFollow = true;

    private BoxCollider2D boundsCollider;
    private Camera targetCamera;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Start()
    {
        if (target == null)
        {
            MainPlayerController player = FindFirstObjectByType<MainPlayerController>();
            if (player != null)
            {
                target = player.transform;
            }
        }

        if (mapBounds != null)
        {
            boundsCollider = mapBounds.GetComponent<BoxCollider2D>();
        }
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        Vector3 desiredPosition = target.position + followOffset;

        if (smoothFollow)
        {
            float t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
            desiredPosition = Vector3.Lerp(transform.position, desiredPosition, t);
        }

        desiredPosition = ClampToBounds(desiredPosition);
        transform.position = desiredPosition;
    }

    private Vector3 ClampToBounds(Vector3 position)
    {
        if (boundsCollider == null)
        {
            return position;
        }

        Bounds map = boundsCollider.bounds;

        if (targetCamera == null || !targetCamera.orthographic)
        {
            position.x = Mathf.Clamp(position.x, map.min.x, map.max.x);
            position.y = Mathf.Clamp(position.y, map.min.y, map.max.y);
            return position;
        }

        float verticalExtent = targetCamera.orthographicSize;
        float horizontalExtent = verticalExtent * targetCamera.aspect;

        float minX = map.min.x + horizontalExtent;
        float maxX = map.max.x - horizontalExtent;
        float minY = map.min.y + verticalExtent;
        float maxY = map.max.y - verticalExtent;

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

        return position;
    }
}
