using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerBase : MonoBehaviour
{
    public Transform player;
    public Transform buildList;
    public Dictionary<Tower, Transform> towers = new Dictionary<Tower, Transform>();
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
            towers.Add(tower, button);
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

        // Shoot a ray and return evetytihng it hits. Open the buildlist if the towerbase is hit, otherwise close it. 
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach (RaycastHit hit in hits)
        {
            if (transform != hit.collider.transform)
                continue;

            buildList.gameObject.SetActive(true);
            SetBuildableTowers();
            return;
        }
        buildList.gameObject.SetActive(false);
    }

    void SetBuildableTowers()
    {
        // Loop through every tower to check if the player has enough gold to build it
        foreach (Tower tower in towers.Keys)
        {
            Button button = towers[tower].GetComponent<Button>();
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
}
