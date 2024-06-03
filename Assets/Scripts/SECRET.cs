using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SECRET : MonoBehaviour
{
    private Collider hitbox;

    void Start()
    {
        hitbox = GetComponent<Collider>();
        if (!hitbox)
            hitbox = transform.AddComponent<SphereCollider>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.GetComponent<Player>() != null)
            SceneManager.LoadScene("level 4");
    }
}
