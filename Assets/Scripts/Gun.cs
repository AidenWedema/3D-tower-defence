using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Gun : MonoBehaviour
{
    private Vector3 target;
    //public AudioSource audioSource;
    public LayerMask mask;
    public float cooldown;
    public float maxCooldown;
    public float reloadTime;
    public float maxReloadTime;

    public float bulletDamage;
    public float bulletSpeed;
    public float bulletSize;
    public int bulletAmount;
    public float bulletSpread;
    public float bulletAcuracy;
    public float bulletDelay;

    [SerializeField]private bool forceEquip;

    private void Start()
    {
        /*
        audioSource = gameObject.GetOrAddComponent<AudioSource>();
        audioSource.clip = Resources.Load<AudioClip>("Gunshot pewpew");
        audioSource.volume = 0.5F;
        audioSource.spatialBlend = 0.5F;
        */
    }

    private void Update()
    {
        if (forceEquip)
        {
            Equip();
            forceEquip = false;
        }

        if (Input.GetMouseButtonDown(1))
        {
            Shoot();
        }
    }

    public void Shoot()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition + Vector3.up);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100, ~mask);
        target = Vector3.zero;
        if (hits.Length > 0)
        {
            // loop through all objects in hits and find the closest one
            float closestDistance = Mathf.Infinity;
            foreach (RaycastHit hit in hits)
            {
                Vector3 directionToHit = (hit.point - transform.position).normalized;
                // ignore objects behind the gun
                if (Vector3.Dot(directionToHit, transform.forward) <= 0)
                    continue;

                if (hit.distance < closestDistance)
                {
                    target = hit.point;
                    closestDistance = hit.distance;
                }
            }
        }
        if (target == Vector3.zero) // set the target to the end of the ray if nothing was hit
            target = ray.GetPoint(100);

        if (bulletAmount == 1)
        {
            MakeBullet();
            return;
        }
        StartCoroutine(Pew());
    }
    public void Shoot(Vector3 t)
    {
        target = t;
        if (bulletAmount == 1)
        {
            MakeBullet();
            return;
        }
        StartCoroutine(Pew());
    }

    IEnumerator Pew()
    {
        int bulletsToFire = bulletAmount;
        while (bulletsToFire > 0)
        {
            MakeBullet();
            bulletsToFire--;
            if (bulletDelay > 0)
                yield return new WaitForSeconds(bulletDelay);
        }
    }

    void MakeBullet()
    {
        //audioSource.Play();

        Transform bullet = Instantiate(Resources.Load<GameObject>("Prefabs/bullet")).transform;
        Rigidbody body = bullet.GetOrAddComponent<Rigidbody>();
        SphereCollider hitbox = bullet.GetOrAddComponent<SphereCollider>();
        Bullet script = bullet.GetComponent<Bullet>();
        bullet.position = transform.position;
        bullet.localScale = new Vector3(bulletSize, bulletSize, bulletSize);
        script.effect = Bullet.Effect.slow;
        script.effectDuration = 1;
        script.damage = bulletDamage;
        script.shooter = transform.parent;

        Vector3 direction = (target - bullet.position).normalized;
        direction += (Random.insideUnitSphere * bulletAcuracy).normalized;
        body.velocity = direction * bulletSpeed;
    }

    public void Equip()
    {
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;

        transform.parent = player;
        transform.localPosition = new Vector3(0.75f, 0, 0.75f);
        transform.localEulerAngles = Vector3.zero;
    }
}
