using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class Metagame : MonoBehaviour
{
    public List<GameObject> locations = new List<GameObject>();
    public NavMeshAgent nav;

    private bool canGo = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            nav.SetDestination(locations[0].transform.position);
            canGo = false;
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            nav.SetDestination(locations[1].transform.position);
            canGo = true;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            nav.SetDestination(locations[2].transform.position);
            canGo = true;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            nav.SetDestination(locations[3].transform.position);
            canGo = true;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            nav.SetDestination(locations[4].transform.position);
            canGo = true;
        }
        if (Input.GetKeyDown(KeyCode.Return)&&canGo)
            SceneManager.LoadScene(3);
    }
}
