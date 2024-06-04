using UnityEngine;

public class Bullet : MonoBehaviour
{
    private SphereCollider hitbox;
    public Effect effect;
    public float effectDuration;
    public float damage;
    public float despawnTimer = 30;
    public Transform shooter;

    public enum Effect
    {
        none,
        burn,
        slow,
    }

    void Start()
    {
        hitbox = GetComponent<SphereCollider>();
        gameObject.layer = LayerMask.NameToLayer("Bullet");
        Physics.IgnoreLayerCollision(gameObject.layer, gameObject.layer, true);
    }

    void Update()
    {
        if (GameManager.GetInstance().paused)
            return;

        despawnTimer -= Time.deltaTime;
        if (despawnTimer < 0)
            Destroy(gameObject);
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.transform != shooter)
        {
            if (damage > 0)
                collision.transform.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            if (effect != 0)
            {
                object[] arr = new object[] { effect, effectDuration };
                collision.transform.SendMessage("ApplyEffect", arr, SendMessageOptions.DontRequireReceiver);
            }
            Destroy(gameObject);
        }
    }
}
