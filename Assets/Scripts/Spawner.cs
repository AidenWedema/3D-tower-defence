using System;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public string level; // the level folder to load waves from
    public string wavefile; // the specific file to load waves from
    public SplinePath path; // the path spawned enemies will follow
    public float timer; // time to wait
    public Wave wave; // current wave
    public List<Wave> waves = new List<Wave>(); // all waves
    public bool asleep; // waiting for the next wave
    public bool end; // try to end the level
    private (GameObject enemy, int amount, float time) spawnQue; // enemies to spawn

    [Serializable]
    public class Wave
    {
        public int index = 0;
        public List<string> functs = new List<string>();
        public List<string[]> args = new List<string[]>();

        public void Next()
        {
            index++;
        }

        public string Getfunct()
        {
            return functs[index];
        }

        public string[] GetArgs()
        {
            return args[index];
        }
    }

    void Start()
    {
        LoadWaves();
        wave = waves[0];
    }

    void Update()
    {
        if (end)
        {
            if (transform.childCount == 0)
                GameManager.GetInstance().EndWave();
            return;
        }

        timer -= Time.deltaTime;
        if (timer > 0 || asleep)
            return;

        if (spawnQue.amount <= 0)
        {
            NextFunct();
            return;
        }
        SpawnEnemy();
    }

    public void NextWave()
    {
        asleep = false;
        timer = 0;
        wave = waves[waves.IndexOf(wave) + 1];
    }

    public void NextFunct()
    {
        GameManager gameManager = GameManager.GetInstance();
        string funct = wave.Getfunct();
        string[] args = wave.GetArgs();

        switch (funct)
        {
            // triggers a new wave, takes no arguments
            case "wave":
                gameManager.NewWave();
                break;

            // spawn enemies, takes 3 arguments: Enemy name, Amount, Time between spawns
            case "spawn":
                spawnQue = (Resources.Load<GameObject>($"Prefabs/Enemies/{args[0]}"), int.Parse(args[1]), float.Parse(args[2]));
                break;

            // wait for amount of seconds, takes 1 argument: seconds to wait
            case "wait":
                timer = float.Parse(args[0]);
                break;

            // wait for next wave to start, takes no arguments
            case "sleep":
                asleep = true;
                break;

            // wait for all enemies to die, then end the stage
            case "end":
                end = true;
                break;

            // default, throws error
            default:
                Debug.LogError($"Invalid funct: {funct}");
                break;
        }
        wave.Next();
    }

    void SpawnEnemy()
    {
        timer = spawnQue.time;
        spawnQue.amount -= 1;

        Transform enemy = Instantiate(spawnQue.enemy).transform;
        enemy.parent = transform;
        enemy.localPosition = Vector3.zero;
        enemy.localEulerAngles = Vector3.zero;
        EnemyOld script = enemy.GetComponent<EnemyOld>();
        script.spline = path;
    }

    void LoadWaves()
    {
        // load the wave file for this level, if it exists
        TextAsset file = Resources.Load<TextAsset>($"Waves/{level}/{wavefile}");
        if (!file)
        {
            Destroy(this);
            return;
        }

        // get all lines from the file
        string[] lines = file.text.ToLower().Split("\n");

        // split each line into its arguments and variables
        Wave newwave = new Wave();
        foreach (string line in lines)
        {
            string[] funct = line.Trim().Split(": ");
            string[] args = { };
            if (funct.Length > 1)
                args = funct[1].Split(", ");
            newwave.functs.Add(funct[0]);
            newwave.args.Add(args);

            if (funct[0] == "sleep" || funct[0] == "wave")
            {
                waves.Add(newwave);
                newwave = new Wave();
            }
        }
    }
}
