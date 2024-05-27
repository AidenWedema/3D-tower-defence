using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TowerBase : MonoBehaviour
{
    public Transform player;
    public Transform buildList;
    public List<GameObject> towers = new List<GameObject>();
    public Dictionary<Image, bool> buttons = new Dictionary<Image, bool>();
    protected Camera cam;
    private GameManager gameManager;

    void Start()
    {
        MakeBuildList();

        player = GameObject.FindWithTag("Player").transform;
        gameObject.layer = LayerMask.NameToLayer("Tower");
        cam = Camera.main;
        gameManager = GameManager.GetInstance();
    }

    void FixedUpdate()
    {
        ManageBuildList();
    }

    private void MakeBuildList()
    {
        buildList = new GameObject(gameObject.name).transform;
        buildList.transform.parent = GameObject.Find("WorldCanvas").transform;
        buildList.position = transform.position + Vector3.up * 2;
        List<GameObject> prefabs = Resources.LoadAll<GameObject>("Prefabs/Towers").ToList();
        prefabs.Remove(Resources.Load<GameObject>("Prefabs/Towers/TowerBase"));
        for (int i = 0; i < prefabs.Count; i++)
        {
            GameObject prefab = prefabs[i];
            //Tower tower = prefab.GetComponent<Tower>();
            Transform button = Instantiate(Resources.Load<GameObject>("Prefabs/TowerButton")).transform;
            button.SetParent(buildList, false);
            button.localPosition = i * Vector3.right - prefabs.Count / 2 * Vector3.right;

            towers.Add(prefab);
            buttons.Add(button.GetComponent<Image>(), false);
        }

        buildList.gameObject.SetActive(false);
    }

    protected void ManageBuildList()
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
        RaycastHit[] hits = Physics.RaycastAll(ray, Mathf.Infinity);
        foreach (RaycastHit hit in hits)
        {
            Transform hittransform = hit.collider.transform;
            if (transform == hittransform)
            {
                SetBuildableTowers();
                buildList.gameObject.SetActive(true);
                return;
            }
            if (hittransform.parent == buildList)
            {
                List<Image> bs = new List<Image>(buttons.Keys);
                int index = bs.IndexOf(hittransform.GetComponent<Image>());
                Image button = bs[index];
                if (!buttons[button])
                    return;
                BuildTower(towers[index]);
                break;
            }
        }
        buildList.gameObject.SetActive(false);
    }

    protected void SetBuildableTowers()
    {
        // Loop through every tower to check if the player has enough gold to build it
        for (int i = 0; i < towers.Count; i++)
        {
            Tower tower = towers[i].GetComponent<Tower>();
            Image button = new List<Image>(buttons.Keys)[i];
            if (tower.stats.cost <= gameManager.gold)
            {
                button.color = Color.white;
                buttons[button] = true;
                continue;
            }
            button.color = new Color32(120, 120, 120, 150);
            buttons[button] = false;
        }
    }

    protected void RotateButonsTowardsCamera()
    {
        buildList.rotation = Quaternion.Euler(cam.transform.eulerAngles);
    }

    public void BuildTower(GameObject prefab)
    {
        // subtract the cost of the tower from the players gold
        GameManager.GetInstance().gold -= prefab.GetComponent<Tower>().stats.cost;
        // clone the tower object and place it on the same position as the tower base
        Transform tower = Instantiate(prefab).transform;
        tower.SetPositionAndRotation(transform.position, transform.rotation);
        // activate the tower and destroy the tower base (this object)
        tower.GetComponent<Tower>().active = true;
        Destroy(gameObject);
    }
}
