using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class Robot : MonoBehaviour
{
    public GameObject hull, lightPoint, lightGlow;
    public NavMeshAgent nav;
    public bool enemy, dead;
    public float hp=100,DPS=20;
    private int Objective = -1;
    public enum ObjectiveDefninitions{
        Wait = -1,
        Enemy = 0,
        Gather = 1,
        Generator = 2,
        Scanner = 3,
        Cowbar = 4,
        IsInVoid = 5
    }
    //general
    void Start()
    {
        if (enemy) {
            Objective = (int)ObjectiveDefninitions.Enemy;
            GameObject.Destroy(lightPoint);
            GameObject.Destroy(lightGlow);
            nav.stoppingDistance = 2.1f;
        }
        if(dead)
        {
            GameObject
            tmp = transform.GetChild(1).gameObject;
            GameObject.Destroy(tmp);
        }
    }
    void Update()
    {
        if (hp < 0 && !dead)
        {
            GameMaster.Enemies.Remove(gameObject);
            GameMaster.Robots.Remove(gameObject);
            dead = true;
            GameObject tmp = transform.GetChild(2).gameObject;
            GameObject.Destroy(tmp);
            tmp = transform.GetChild(1).gameObject;
            GameObject.Destroy(tmp);
            nav.isStopped = true;
        }
        if(!dead)    
            switch (Objective)
            {
                case (int)ObjectiveDefninitions.Enemy: PlayEnemy(); break;
                case (int)ObjectiveDefninitions.Gather: ContinueGather();    break;
                case (int)ObjectiveDefninitions.Generator: ContinueGenerator(); break;
                case (int)ObjectiveDefninitions.Scanner: ContinueScanner();   break;
                case (int)ObjectiveDefninitions.Cowbar: ContinueCowbar();    break;
            }
    }
    //basic
    public void NavigateTo(Vector3 position)
    {

        if (currentDestination == null)
            currentDestination = gameObject;
        nav.SetDestination(position);
        GameMaster.GeenLight = true;

    }
    public GameObject GetRoomImIn()
    {
        var coords = transform.position + Vector3.up;
        RaycastHit[] hit = Physics.RaycastAll(coords, Vector3.down);
        GameObject room = null;
        for (int i = 0; i < hit.Length; i++)
            if(hit[i].transform.parent!=null)
                if (hit[i].transform.parent.name.StartsWith("R"))
                    room = hit[i].transform.parent.gameObject;
        return room;
    }
    private GameObject GetOBJBelow()
    {
        var coords = transform.position + Vector3.up;
        //Physics.Raycast(coords, Vector3.down, out var hit);

        RaycastHit[] hit = Physics.RaycastAll(coords, Vector3.down);
        GameObject OBJ = null;
        for (int i = 0; i < hit.Length; i++)
            if (hit[i].transform.gameObject.tag.Equals("OBJ"))
                OBJ = hit[i].transform.gameObject;
        return OBJ;
    }
    //specyfic
    public GameObject currentDestination;
    private bool IsOnTheMove = false;
    private float TimeToAIRefresh = 0, TimeToRoomChange = 0;
    private void PlayEnemy()
    {
        TimeToAIRefresh -= Time.deltaTime;         
        if (TimeToAIRefresh < 0)
        {
            TimeToAIRefresh = 1;
            GameObject enemy = GameMaster.GetRobotForEnemy(transform.position);
            IsOnTheMove = !(enemy == null || 15<(Vector3.Distance(transform.position, enemy.transform.position)));

            if (IsOnTheMove) {
                hull.SetActive(true);
                if (enemy != null)
                {
                    if (enemy.name.StartsWith("B"))
                    {
                        try
                        {
                            var navMeshPath = new NavMeshPath();

                            nav.CalculatePath(enemy.transform.position, navMeshPath);
                            IsOnTheMove = (navMeshPath.status == NavMeshPathStatus.PathComplete);
                            if (IsOnTheMove)
                            {
                                NavigateTo(enemy.transform.position);
                                OnCloseContact(enemy, TimeToAIRefresh);//====================================>work around dla private void OnTriggerStay(Collider other)
                            }
                        }
                        catch (Exception ex) { GameMaster.Console.text += ex.ToString(); }
                    }
                }
            }
            if(!IsOnTheMove) {
                hull.SetActive(false);
                TimeToRoomChange -= TimeToAIRefresh;
                if (TimeToRoomChange < 0)
                {
                    TimeToRoomChange = 30;
                    GameObject room = GetRoomImIn();
                    if (room != null)
                    {
                        List<GameObject> rooms = new List<GameObject>();
                        room.GetComponent<RoomScript>().Doors.ForEach(d =>
                        {
                            DoorsScript ds = d.GetComponent<DoorsScript>();
                            if (ds.open&&!ds.airlock)
                            {
                                if (room.name != ds.roomA.name)
                                    rooms.Add(ds.roomA);
                                if (room.name != ds.roomB.name)
                                    rooms.Add(ds.roomB);
                            }
                        });
                        rooms.Add(room);
                        int index = Random.Range(0, rooms.Count);
                        currentDestination = rooms[index];
                        var tmp = rooms.ToArray();
                    }
                    NavigateTo(currentDestination.transform.position);
                }
            }
        }
    }//todo check for collisions and kill/damage
    private void OnCloseContact(GameObject enemy, float dT)
    {
        if (Vector3.Distance(transform.position, enemy.transform.position) < 3)
            enemy.gameObject.GetComponent<Robot>().hp -= DPS * dT;
    }
    public  void Gather()
    {
        GameMaster.GeenLight = true;
        Objective = (int)ObjectiveDefninitions.Gather;
        currentDestination = null;
    }
    private void ContinueGather()
    {
        if (currentDestination == null) {
            GameObject room = GetRoomImIn();
            if(room == null) {
                Objective = (int)ObjectiveDefninitions.Wait;
                return;
            }
            if (room.GetComponent<RoomScript>().Fuel.Count != 0) {
                currentDestination = room.GetComponent<RoomScript>().Fuel[0];
                room.GetComponent<RoomScript>().Fuel.RemoveAt(0);
            } else if (room.GetComponent<RoomScript>().Scrap.Count != 0) {

                //currentDestination = room.GetComponent<RoomScript>().Scrap[0];
                //room.GetComponent<RoomScript>().Scrap.RemoveAt(0);
                //old -> modernized -> add return feture

                var roomS = room.GetComponent<RoomScript>();
                var pos = transform.position;
                var objs = new List<GameObject>(roomS.Scrap)
                    .OrderBy(s=>Vector3.Distance(pos,s.transform.position))
                    .ToList();
                currentDestination = objs[0];
                roomS.Scrap.Remove(currentDestination);

            } else  {
                Objective = (int)ObjectiveDefninitions.Wait;
                return;
            }
        } else {
            if (!IsOnTheMove)
            {
                NavigateTo(currentDestination.transform.position); 
                IsOnTheMove = true;
            }

            if (Vector3.Distance(transform.position,currentDestination.transform.position) < 1)
            {
                switch (currentDestination.name)
                {
                    case "Scrap": {
                            currentDestination.GetComponent<ScrapOrFuel>().Collect();
                            GameObject.Destroy(currentDestination); currentDestination = null;
                        } break;
                    case "Fuel": {
                            currentDestination.GetComponent<ScrapOrFuel>().Collect();
                            currentDestination = null;
                        } break;
                }
                IsOnTheMove = false;
            }
        }
    }
    public  void Generator()
    {
        GameMaster.GeenLight = true;
        Objective = (int)ObjectiveDefninitions.Generator;

        GameObject room = GetRoomImIn();
        if (room == null) {//is NOT in room?
            Objective = (int)ObjectiveDefninitions.Wait;
            return;
        }else if (room.GetComponent<RoomScript>().Generator.Count != 0) {//room has generator?
            currentDestination = room.GetComponent<RoomScript>().Generator[0];
            NavigateTo(currentDestination.transform.position);
        } else {//room DOES NOT have generator
            Objective = (int)ObjectiveDefninitions.Wait;
            return;
        }
    }
    private void ContinueGenerator()
    {
        GameObject gen = GetOBJBelow();
        if(gen!=null)
            if (gen.name.Equals("Generator"))
                gen.GetComponent<GeneratorScript>().Energize();
    }
    public  void Scanner()
    {
        GameMaster.GeenLight = true;
        Objective = (int)ObjectiveDefninitions.Scanner;
        rooms = new List<RoomScript>();
        var room = GetRoomImIn();
        if (room == null)
        {
            Objective = (int)ObjectiveDefninitions.Wait;
            return;
        }
        var var = room.GetComponent<RoomScript>().Doors;
        rooms = new List<RoomScript>();
        room.GetComponent<RoomScript>().Doors.ForEach(d =>
        {
            DoorsScript ds = d.GetComponent<DoorsScript>();
            if (room.name != ds.roomA.name)
                rooms.Add(ds.roomA.GetComponent<RoomScript>());
            if (room.name != ds.roomB.name)
                rooms.Add(ds.roomB.GetComponent<RoomScript>());
        });
        currentDestination = null;
    }
    private List<RoomScript> rooms;
    private void ContinueScanner()
    {
        rooms.ForEach(r => r.Scan());
        if (currentDestination != null)
        {
            if (Objective == (int)ObjectiveDefninitions.Scanner)
                Objective = (int)ObjectiveDefninitions.Wait;
        }
    }//todo
    public  void Cowbar(GameObject targetDoor)
    {
        GameMaster.GeenLight = true;
        Objective = (int)ObjectiveDefninitions.Cowbar;
        currentDestination = targetDoor;
        NavigateTo(targetDoor.transform.position);
    }
    private void ContinueCowbar()
    {
        var tmp = Vector3.Distance(transform.position, currentDestination.transform.position);
        if (tmp < 3.5f)
        {
            Objective = (int)ObjectiveDefninitions.Wait;
            currentDestination.GetComponent<DoorsScript>().ForceOpenDoor();
            currentDestination = null;
        }
    }
    //enviroment

}
