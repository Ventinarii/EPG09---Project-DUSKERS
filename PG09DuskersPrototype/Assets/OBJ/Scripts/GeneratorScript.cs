using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorScript : MonoBehaviour
{
    public List<GameObject> Rooms = new List<GameObject>();
    public List<GameObject> Door  = new List<GameObject>();
    public void Energize()
    {
        if (1 < TimeSinceRobotReponse) {
            TimeSinceRobotReponse = 0;
            for (int i = 0; i < Rooms.Capacity; i++)
                Rooms[i].GetComponent<RoomScript>().Energize();
            for (int i = 0; i < Door.Capacity; i++)
                Door[i].GetComponent<DoorsScript>().Energize();
        }
        TimeSinceRobotReponse = 0;
    }
    public void DeEnergize()
    {
        for (int i = 0; i < Rooms.Capacity; i++)
            Rooms[i].GetComponent<RoomScript>().DeEnergize();
        for (int i = 0; i < Door.Capacity; i++)
            Door[i].GetComponent<DoorsScript>().DeEnergize();
    }
    [HideInInspector]
    public float TimeSinceRobotReponse = 0;
    public void Update()
    {
        if (TimeSinceRobotReponse < 10000)
            TimeSinceRobotReponse += Time.deltaTime;
        if (1 < TimeSinceRobotReponse)
            DeEnergize();
    }
}
