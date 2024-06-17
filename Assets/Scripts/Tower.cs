using System;
using System.Collections.Generic;
using UnityEngine;

public class Tower : TowerBase
{
    [SerializeField] private AudioSource audioSource;
    public BulletStats bulletStats;
    public Transform target;
    public List<Transform> targets = new List<Transform>();
    public bool active;
    public Vector3 rotationOffset;

    [Serializable]
    public class BulletStats
    {
        public GameObject prefab; // the bullet to load
        public Vector3 size; // the size of the bullet
        public Vector3 shootposition; // an offset position from which the bullet will be shot
        public Bullet.Effect effect; // the effect the bullet gives
        public float effectDuration; // the time the effect takes
        public float damage; // the damage the bullet does
        public float speed; // the speed at which the bullet travvels
        public int amount; // amount of bullets to shoot
        public float spread; // the spread of bullets
        public float acuracy; // the acuracy of the bullets
        public float delay; // the delay between bullets shot
        public bool useGravity; // should the bullet use gravity
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        gameObject.layer = LayerMask.NameToLayer("Tower");
        cam = Camera.main;
        gameManager = GameManager.GetInstance();
        hitbox = gameObject.GetComponent<Collider>();
        audioSource = gameObject.GetComponent<AudioSource>();
        MakeBuildList();
    }

    void Update()
    {
        if (!active)
            return;

        ManageBuildList();

        if (!target)
            FindTarget();
        else
            Aim();

        stats.timer -= Time.deltaTime;
        if (stats.timer > 0)
            return;

        if (targets.Count > 0)
            Shoot();
    }

    private void FindTarget()
    {
        targets.Clear();
        Collider[] hits = Physics.OverlapSphere(transform.position, stats.range, LayerMask.GetMask("Enemy"));

        if (hits.Length == 0)
        {
            target = null;
            return;
        }

        foreach (Collider hit in hits)
        {
            targets.Add(hit.transform);
        }
        target = targets[^1];
    }

    void Shoot()
    {
        stats.timer = stats.reload;
        if (!audioSource.isPlaying)
            audioSource.Play();

        Transform bullet = Instantiate(bulletStats.prefab).transform;
        Rigidbody body = bullet.GetComponent<Rigidbody>();
        body.useGravity = bulletStats.useGravity;
        Bullet script = bullet.GetComponent<Bullet>();
        bullet.position = GetShootPosition();
        bullet.localScale = bulletStats.size;
        script.effect = bulletStats.effect;
        script.effectDuration = bulletStats.effectDuration;
        script.damage = bulletStats.damage;
        script.shooter = transform;
        Physics.IgnoreCollision(hitbox, bullet.GetComponent<Collider>(), true);

        Vector3 direction = (PredictMovement() - bullet.position).normalized;
        direction += (UnityEngine.Random.insideUnitSphere * bulletStats.acuracy).normalized;
        body.velocity = direction * bulletStats.speed;
    }

    Vector3 PredictMovement()
    {
        if (!target)
            return Vector3.zero;

        // get distance and time to the target
        float distance = Vector3.Distance(transform.position + bulletStats.shootposition, target.position);
        float time = distance / (bulletStats.speed * Time.deltaTime);

        // get the target's point after time
        EnemyOld enemy = target.GetComponent<EnemyOld>();
        Vector3 pos = TargetMove();

        // get the new distance to the predicted target position
        distance = Vector3.Distance(transform.position + bulletStats.shootposition, pos);
        time = distance / (bulletStats.speed * Time.deltaTime);

        // return the predicted position
        return TargetMove();

        Vector3 TargetMove()
        {
            int index = enemy.spline.GetClosestPointIndex(transform.position);
            try
            {
                int i = Mathf.Min(index + (int)time, enemy.spline.points.Count - 1);
                Vector3 point = enemy.spline.points[i];
                return Vector3.MoveTowards(target.position, point, enemy.currentSpeed * Time.deltaTime * time);
            }
            catch (Exception e)
            {
                Debug.LogError($"{e}\ntime:{time}, index:{index}\nProbably a devide by 0 error on with bulletStats.speed...");
                Vector3 point = enemy.spline.points[index + 5];
                return Vector3.MoveTowards(target.position, point, enemy.currentSpeed * Time.deltaTime * time);
            }
        }
    }

    void Aim()
    {
        transform.LookAt(target.position);
        transform.eulerAngles += rotationOffset;
    }

    Vector3 GetShootPosition()
    {
        return transform.TransformPoint(bulletStats.shootposition);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stats.range);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(GetShootPosition(), 0.1f);

        if (!target)
            return;

        Vector3 point = PredictMovement();
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(point, 0.1f);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(GetShootPosition(), point);
    }
}
