using System;
using UnityEngine;

public class Tower : MonoBehaviour
{
    public Stats stats;
    public Transform player;

    [Serializable]
    public class Stats
    {
        public string name; // name of the tower
        public float range; // range from the tower center
        public float reload; // time it take to reload after shooting
        public int cost; // amount of gold needed to build this tower
        public int damage; // amount of damage a bullet does
        public string effect; // optional effect to inflict on any enemies that are hit
        public float timer; // time left until shooting again
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
