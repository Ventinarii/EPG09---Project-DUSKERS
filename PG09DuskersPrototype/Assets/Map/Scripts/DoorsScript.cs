using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorsScript : MonoBehaviour
{
    [HideInInspector]
    public GameObject roomA = null, roomB = null;
    private Vector3 doorShift;

    public GameObject Text;
    public GameObject DoorOpenLeft, DoorClosedRight;
    public bool open, online = false, airlock  = false;
    //general
    void Start()
    {
        doorShift = DoorOpenLeft.transform.localPosition - DoorClosedRight.transform.localPosition;
        doorShift = -doorShift/4;
        DoorOpenLeft.transform.localPosition = -doorShift + new Vector3(0, 0.1f, 0);
        string name = this.name;
        if (open)
        {
            open = !open;
            bool pwr = online;
            online = true;
            OpenDoor();
            online = pwr;
        }
        Vector3 tmp;
        if (transform.rotation.y == 0)
        { tmp = Vector3.forward * doorShift.x; } else
        { tmp = Vector3.right   * doorShift.x; }
        {
            Vector3 hitFrom = transform.position + tmp + Vector3.up;
            RaycastHit[] hit = Physics.RaycastAll(hitFrom, Vector3.down);
            for (int i = 0; i < hit.Length; i++)
                if (hit[i].transform.parent.name.StartsWith("R"))
                    roomA = hit[i].transform.parent.gameObject;
        }
        {
            Vector3 hitFrom = transform.position - tmp + Vector3.up;
            RaycastHit[] hit = Physics.RaycastAll(hitFrom, Vector3.down);
            for (int i = 0; i < hit.Length; i++)
                if (hit[i].transform.parent.name.StartsWith("R"))
                    roomB = hit[i].transform.parent.gameObject;
        }
        if (roomA != null)
            roomA.GetComponent<RoomScript>().Doors.Add(gameObject);
        if (roomB != null)
            roomB.GetComponent<RoomScript>().Doors.Add(gameObject);

        airlock = name.StartsWith("A");

        if(online)
            Text.GetComponent<NumberScript>().Energize();
    }
    //power
    public void Energize()
    {
        Text.GetComponent<NumberScript>().Energize();
        online = true;
    }
    public void DeEnergize()
    {
        Text.GetComponent<NumberScript>().DeEnergize();
        online = false;
    }
    //specyfic
    public bool OpenDoor()
    {
        GameMaster.GeenLight = true;
        if (online)
        {
            if (!open)
            {
                DoorOpenLeft.transform.localPosition = -doorShift * 3 + new Vector3(0, 0.1f, 0);
                DoorClosedRight.transform.localPosition = doorShift * 3 + new Vector3(0, 0.1f, 0);
                open = true;
                if(airlock)
                    CheckIfExposedToVoidOrRadiation();
                return true;
            }
        }
        return false;
    }
    public bool CloseDoor()
    {
        GameMaster.GeenLight = true;
        if (online)
        {
            if (open)
            {
                DoorOpenLeft.transform.localPosition = -doorShift + new Vector3(0, 0.1f, 0);
                DoorClosedRight.transform.localPosition = doorShift + new Vector3(0, 0.1f, 0);
                open = false;
                return true;
            }
        }
        return false;
    }
    public bool SwapDoor()
    {
        GameMaster.GeenLight = true;
        if (open)
        {
            CloseDoor();
        }
        else
        {
            OpenDoor();
        }
        return true;
    }
    public bool ForceOpenDoor()
    {
        if (!open)
        {
            DoorOpenLeft.transform.localPosition = -doorShift * 3 + new Vector3(0, 0.1f, 0);
            DoorClosedRight.transform.localPosition = doorShift * 3 + new Vector3(0, 0.1f, 0);
            open = true;
            return true;
        }
        return false;
    }
    //enviroment
    private void CheckIfExposedToVoidOrRadiation() {
        if (roomA == null || roomB == null) {//exposed to outside
            if (roomA == null){
                roomB.GetComponent<RoomScript>().Exposed(true, true, this);
            } else {
                roomA.GetComponent<RoomScript>().Exposed(true, true, this);
            }
        }
    }
}
