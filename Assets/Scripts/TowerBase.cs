using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerBase : MonoBehaviour
{
    public Transform player;
    public Transform buildList;
    public Dictionary<Tower, Transform> towers;

    void Start()
    {
        foreach (GameObject prefab in Resources.LoadAll<GameObject>("Prefabs/Towers"))
        {
            Tower tower = prefab.GetComponent<Tower>();
            Transform button = Resources.Load<GameObject>("Prefabs/button").transform;

            towers.Add(tower, button);
        }

        player = GameObject.FindWithTag("Player").transform;
        gameObject.layer = LayerMask.NameToLayer("Tower");
    }

    void FixedUpdate()
    {
        if (Vector3.Distance(transform.position, player.position) > 2)
            return;

        SetBuildableTowers();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, gameObject.layer))
        {
            buildList.gameObject.SetActive(true);
            return;
        }
        buildList.gameObject.SetActive(false);
    }

    void SetBuildableTowers()
    {
        foreach (Tower tower in towers.Keys)
        {
            Button button = towers[tower].GetComponent<Button>();
            if (tower.stats.cost <= GameManager.gold)
            {
                button.enabled = true;
                continue;
            }
            button.enabled = false;
        }
    }
}
