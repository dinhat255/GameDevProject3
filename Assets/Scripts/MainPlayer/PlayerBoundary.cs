using UnityEngine;

public class PlayerBoundary : MonoBehaviour
{
    public Transform mapBounds;
    private BoxCollider2D col;
    private Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        col = mapBounds.GetComponent<BoxCollider2D>();
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void LateUpdate()
    {
        Bounds b = col.bounds;

        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, b.min.x, b.max.x);
        pos.y = Mathf.Clamp(pos.y, b.min.y, b.max.y);

        transform.position = pos;
    }
}
