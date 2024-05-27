using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tower : TowerBase
{
    public Stats stats;
    public BulletStats bulletStats;
    public Transform target;
    public List<Transform> targets = new List<Transform>();
    public bool active;

    [Serializable]
    public class Stats
    {
        public string name; // name of the tower
        public float range; // range from the tower center
        public float reload; // time it take to reload after shooting
        public int cost; // amount of gold needed to build this tower
        public int damage; // amount of damage a bullet does
        public float timer; // time left until shooting again
    }

    [Serializable]
    public class BulletStats
    {
        public GameObject prefab;
        public Vector3 size;
        public Vector3 shootposition;
        public Bullet.Effect effect;
        public float effectDuration;
        public float damage;
        public float speed;
        public int amount;
        public float spread;
        public float acuracy;
        public float delay;
        public bool useGravity;
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        gameObject.layer = LayerMask.NameToLayer("Tower");
        cam = Camera.main;
        MakeBuildList();
    }

    void Update()
    {
        if (!active)
            return;

        ManageBuildList();

        stats.timer -= Time.deltaTime;
        if (stats.timer > 0)
            return;

        FindTarget();
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

        Transform bullet = Instantiate(bulletStats.prefab).transform;
        Rigidbody body = bullet.GetComponent<Rigidbody>();
        body.useGravity = bulletStats.useGravity;
        Bullet script = bullet.GetComponent<Bullet>();
        bullet.position = transform.position + bulletStats.shootposition;
        bullet.localScale = bulletStats.size;
        script.effect = bulletStats.effect;
        script.effectDuration = bulletStats.effectDuration;
        script.damage = bulletStats.damage;
        script.shooter = transform.parent;

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
            Vector3 point = enemy.spline.points[index + (int)time];
            return Vector3.MoveTowards(target.position, point, enemy.currentSpeed * Time.deltaTime * time);
        }
    }

    private void MakeBuildList()
    {
        TextAsset file = Resources.Load<TextAsset>($"Tower upgrades");
        // get all lines from the file
        string[] lines = file.text.ToLower().Split("\n");
        string[] upgrades = { "TowerBase" };
        foreach (string line in lines)
        {
            string[] tower = line.Split(": ");
            if (tower[0] == stats.name)
            {
                upgrades = tower[1].Split(", ");
            }
        }

        buildList = new GameObject(gameObject.name).transform;
        buildList.transform.parent = GameObject.Find("WorldCanvas").transform;
        buildList.position = transform.position + Vector3.up * 2;
        for(int i = 0; i < upgrades.Length; i++)
        {
            string upgrade = upgrades[i];
            GameObject prefab = Resources.Load<GameObject>($"Prefabs/Towers/{upgrade}");
            Transform button = Instantiate(Resources.Load<GameObject>("Prefabs/TowerButton")).transform;
            button.SetParent(buildList, false);
            button.localPosition = i * Vector3.right - upgrades.Length / 2 * Vector3.right;

            towers.Add(prefab);
            buttons.Add(button.GetComponent<Image>(), false);
        }

        buildList.gameObject.SetActive(false);
    }

    public void Build(Transform towerbase)
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stats.range);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(PredictMovement(), 0.1f);
    }
}
