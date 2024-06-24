using UnityEngine;
using UnityEngine.SceneManagement;

public class Menuscript : MonoBehaviour
{
    private void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;
    }

    public void SwichScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.Escape) && Input.GetKey(KeyCode.E) && Input.GetKey(KeyCode.Alpha3))
        {
            SwichScene("e3");
        }
    }
}