using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyOld : MonoBehaviour
{
    [SerializeField] private Collider hitbox;
    [SerializeField] private AudioSource audioSource;

    public SplinePath spline; // the spline to drive along
    public Dictionary<Effect, float> effects = new Dictionary<Effect, float>();
    public float t;
    public float speed;
    public float currentSpeed;
    public int audioChance;

    public string soundFolder;
    public float health;
    public int damage;
    public float timer;
    public int gold;
    public AudioClip[] sounds;

    public enum Effect
    {
        none,
        burn,
        slow,
    }

    private void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Enemy");
        hitbox = gameObject.GetComponent<Collider>();
        audioSource = gameObject.GetComponent<AudioSource>();
        if (!hitbox)
            hitbox = gameObject.AddComponent<BoxCollider>();
        if (!audioSource)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.spatialBlend = 1;
        
        sounds = Resources.LoadAll<AudioClip>($"Audio/Enemy soundeffects/{soundFolder}");

        currentSpeed = speed;
    }

    void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            return;
        }

        if (effects.Count > 0)
        {
            GetAffectedByEffect();
        }

        if (Time.frameCount % 60 == 0 && !audioSource.isPlaying)
        {
            if (Random.Range(0, 100) < audioChance)
                PlayAudio();
        }

        int index = spline.GetClosestPointIndex(transform.position);
        Vector3 point = spline.points[index + 1];
        transform.position = Vector3.MoveTowards(transform.position, point, currentSpeed * Time.deltaTime);

        LookAtPath();

        t = spline.GetTimeFromPoint(transform.position);

        if (t == 1)
        {
            //GameManager.TakeDamage(damage);
            //GameManager.UpdateUI();
            Destroy(gameObject);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health > 0)
            return;

        GameManager.GetInstance().gold += gold;
        Destroy(gameObject);
    }

    public void ApplyEffect(object[] arr)
    {
        Effect effect = (Effect)arr[0];
        float duration = (float)arr[1];
        // Apply the effect if it is not already, otherwise set the duration if it is lower than the effect's currect duration.
        if (!effects.ContainsKey(effect))
            effects.Add(effect, duration);
        else if (effects[effect] < duration)
            effects[effect] = duration;
    }

    void GetAffectedByEffect()
    {
        float deltatime = Time.deltaTime;
        for (int i = 0; i < effects.Count; i++)
        {
            Effect effect = new List<Effect>(effects.Keys)[i];
            float duration = effects[effect];
            effects[effect] = duration - deltatime;
            if (effects[effect] <= 0)
            {
                currentSpeed = speed;
                effects.Remove(effect);
                continue;
            }

            switch (effect)
            {
                case Effect.burn:
                    TakeDamage(0.1f);
                    break;

                case Effect.slow:
                    currentSpeed = speed * 0.5f;
                    break;
            }
        }
    }

    void LookAtPath()
    {
        // get the point in front of the enemy and the point on the spline closest to that point
        Vector3 point = transform.position + transform.forward * 0.2f;
        Vector3 splinePoint = spline.GetClosestPoint(point);

        float distance = Vector3.Distance(point, splinePoint);

        // if the point is close enough to the spline just continue walking normaly
        if (distance < 0.005f)
            return;

        // get the direction to the spline
        Vector3 directionToSpline = splinePoint - point;

        // Calculate the desired forward direction for the Ai
        Vector3 desiredForward = Vector3.Lerp(transform.forward, directionToSpline.normalized, 1);

        float angle = Vector3.SignedAngle(transform.forward, desiredForward, transform.up) / 180f;

        transform.Rotate(Vector3.up, angle);
    }

    void PlayAudio()
    {
        AudioClip sound = sounds[Random.Range(0, sounds.Length)];
        audioSource.clip = sound;
        audioSource.Play();
    }
}

