using UnityEngine;

public class Teraindetail : MonoBehaviour
{
    private Collider hitbox;
    public Vector2 size = Vector2.one;
    public bool moveToGround;

    void Start()
    {
        transform.localScale = Vector3.one * Random.Range(size.x, size.y);

        if (!moveToGround)
            return;

        hitbox = GetComponent<Collider>();

        Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit);
        Vector3 position = hit.point + hitbox.bounds.extents.y * Vector3.up;

        Physics.Raycast(transform.position + hitbox.bounds.extents.x * Vector3.one, -Vector3.up, out RaycastHit hit1);
        Physics.Raycast(transform.position + hitbox.bounds.extents.y * Vector3.one, -Vector3.up, out RaycastHit hit2);
        Physics.Raycast(transform.position - hitbox.bounds.extents.x * Vector3.one, -Vector3.up, out RaycastHit hit3);
        Physics.Raycast(transform.position - hitbox.bounds.extents.y * Vector3.one, -Vector3.up, out RaycastHit hit4);
        // Calculate average normal of the terrain beneath the car
        Vector3 averageNormal = (hit1.normal + hit2.normal + hit3.normal + hit4.normal).normalized;
        // Calculate rotation to align with the surface normal
        Quaternion rotation = Quaternion.FromToRotation(transform.up, averageNormal) * transform.rotation;

        transform.SetPositionAndRotation(position, rotation);
        Destroy(this);
    }

    private void OnDrawGizmos()
    {
        if (!moveToGround)
            return;
        hitbox = GetComponent<Collider>();
        Gizmos.color = Color.green;
        if (Physics.Raycast(transform.position, -Vector3.up, out RaycastHit hit))
            Gizmos.DrawLine(transform.position, hit.point);
    }
}
