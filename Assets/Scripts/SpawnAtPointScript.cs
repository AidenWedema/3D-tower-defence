using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnAtPointScript : MonoBehaviour
{

    public GameObject spawnPoint;
    // Start is called before the first frame update
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Hit met {collision.gameObject.name}");
        if (collision.gameObject.CompareTag("Player"))
        {
            if (spawnPoint)
            {
                collision.transform.position = spawnPoint.transform.position;
            }
            else
            {
                Debug.Log("NO SPAWNPOINT ASSIGNED IN INSPECTOR");
            }
        }
    }
}
