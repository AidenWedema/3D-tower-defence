using UnityEngine;
using UnityEngine.SceneManagement;

// singleton
public class GameManager
{
    public static GameManager instance;

    public bool paused;
    public int gold;
    public int wave;
    public int hp;

    private GameManager()
    {
        paused = false;
        gold = 500;
        hp = 100;
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

    public void TakeDamage(int damage)
    {
        hp -= damage;
        if (hp <= 0)
            GameOver();
    }

    public void GameOver()
    {
        SceneManager.LoadScene("Lose");
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
        SceneManager.LoadScene("Win");
    }
}
