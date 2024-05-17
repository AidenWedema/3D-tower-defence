using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerBase : MonoBehaviour
{
    public Transform player;
    public Transform buildList;
    public List<Tower> towers = new List<Tower>();
    public List<Button> buttons = new List<Button>();
    private Camera cam;

    void Start()
    {
        buildList = new GameObject(gameObject.name).transform;
        buildList.transform.parent = GameObject.Find("WorldCanvas").transform;
        buildList.position = transform.position + Vector3.up * 2;
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/Towers");
        for (int i = 0; i < prefabs.Length; i++)
        {
            GameObject prefab = prefabs[i];
            Tower tower = prefab.GetComponent<Tower>();
            Transform button = Instantiate(Resources.Load<GameObject>("Prefabs/button")).transform;
            button.parent = buildList;
            button.localPosition = i * Vector3.right - prefabs.Length / 2 * Vector3.right;

            towers.Add(tower);
            buttons.Add(button.GetComponent<Button>());
        }

        player = GameObject.FindWithTag("Player").transform;
        gameObject.layer = LayerMask.NameToLayer("Tower");
        cam = Camera.main;

        buildList.gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        // Check if the player is close enough
        if (Vector3.Distance(transform.position, player.position) > 2)
        {
            buildList.gameObject.SetActive(false);
            return;
        }

        RotateButonsTowardsCamera();

        // Is the primary mouse button pressed this frame?
        if (!Input.GetMouseButtonDown(0))
            return;

        // Shoot a ray and return everything it hits. Show the buildlist if the towerbase is hit, otherwise hide it. If a button is hit, click it.
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach (RaycastHit hit in hits)
        {
            Transform hittransform = hit.collider.transform;
            Debug.Log(hittransform.name);
            if (transform == hittransform)
            {
                SetBuildableTowers();
                buildList.gameObject.SetActive(true);
                return;
            }
            if (hittransform.parent == buildList)
            {
                int index = buttons.IndexOf(hittransform.GetComponent<Button>());
                Button button = buttons[index];
                if (!button.interactable)
                    return;
                BuildTower(towers[index]);
                break;
            }
        }
        buildList.gameObject.SetActive(false);
    }

    void SetBuildableTowers()
    {
        // Loop through every tower to check if the player has enough gold to build it
        for (int i = 0; i < towers.Count; i++)
        {
            Tower tower = towers[i];
            Button button = buttons[i];
            if (tower.stats.cost <= GameManager.gold)
            {
                button.interactable = true;
                continue;
            }
            button.interactable = false;
        }
    }

    void RotateButonsTowardsCamera()
    {
        buildList.rotation = Quaternion.Euler(cam.transform.eulerAngles);
    }

    public void BuildTower(Tower tower)
    {
        Debug.Log($"Building tower {tower.name}");
    }
}
