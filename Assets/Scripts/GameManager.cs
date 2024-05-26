using UnityEngine;

public class GameManager
{
    public static GameManager instance;

    public bool paused;
    public int gold;
    public int wave;

    private GameManager()
    {
        paused = false;
        gold = 500;
        wave = 0;
    }

    public static GameManager GetInstance()
    {
        if (instance == null)
        {
            instance = new GameManager();
        }

        return instance;
    }

    public void NewWave()
    {
        wave++;
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Spawner");
        foreach (GameObject obj in objects)
            obj.GetComponent<Spawner>().NextWave();
    }

    public void EndWave()
    {

    }
}
