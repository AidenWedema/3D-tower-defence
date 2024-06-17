using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public TMPro.TMP_Text HP_text;
    public TMPro.TMP_Text Gold_text;
    public TMPro.TMP_Text Wave_text;
    public Slider Stamina_slider;

    private GameManager gameManager;
    private Player player;
    private float t;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        gameManager = GameManager.GetInstance();
    }

    void LateUpdate()
    {
        t += Time.deltaTime * 2;
        HP_text.text = $"HP {gameManager.hp}";
        Gold_text.text = $"${gameManager.gold}";
        Wave_text.text = $"Wave {gameManager.wave}";

        if (player.stamina < player.maxStamina)
        {
            Stamina_slider.gameObject.SetActive(true);
            Stamina_slider.value = player.stamina / player.maxStamina;
            if (player.noRunningAlowed)
            {
                if (t <= 0.5f)
                    Stamina_slider.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = new Color32(229, 135, 35, 255);
                else
                {
                    Stamina_slider.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = new Color32(0, 0, 0, 0);
                    if (t >= 1)
                        t = 0;
                }
            }
            else
                Stamina_slider.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = new Color32(35, 229, 52, 255);
            return;
        }
        Stamina_slider.gameObject.SetActive(false);
    }

    public void NewWave() 
    {
        StartCoroutine(NewWaveFlash());
    }

    IEnumerator NewWaveFlash()
    {
        Color32[] colorloop = { Color.white, Color.red, new(255, 165, 0, 255), Color.yellow, Color.green, Color.cyan, Color.blue, new(160, 32, 240, 255), Color.white };
        int loops = 2;
        int i = 0;
        float t = 0;

        while (loops > 0)
        {
            Wave_text.color = Color.Lerp(colorloop[i], colorloop[i + 1], t);
            t += Time.deltaTime * 5;
            if (t >= 1)
            {
                t = 0;
                i++;
                if (i == colorloop.Length - 1)
                {
                    i = 0;
                    loops--;
                }
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }
}
