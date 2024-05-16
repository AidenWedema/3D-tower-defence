using UnityEngine;

public class Tower : MonoBehaviour
{
    public Stats stats;
    public Transform player;

    public class Stats
    {
        public string name;
        public float range;
        public float reload;
        public int cost;
        public int damage;
        public string effect;
        public float timer;
    }

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, player.position) > 2)
            return;


    }

    void Build()
    {

    }
}
