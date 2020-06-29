using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomScript : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> Boxes     = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> Fuel      = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> Generator = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> Scrap     = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> Dead      = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> Misc      = new List<GameObject>();
    [HideInInspector]
    public List<GameObject> Doors     = new List<GameObject>();
    //text & material
    public GameObject Text;
    public Material ScanSafe, ScanUknown, ScanEnemy;
    public GameObject Floor,MaskScan,VoidMarker,RadiationMarker,InRoom;
    //enviroment
    public  bool ScanAble = true, IsVoid = false, IsContaminated = false, online = false;
    public  float VoidTakes = 2, RadiationTakes=10;
    private float VoidIn = 3, RadiationIn = 11, CheckIfSafeWarranty = 0;
    public  DoorsScript responsibleOne;
    //==============================================================================
    //general
    void Start()
    {
        for (int i = 0; i < InRoom.transform.childCount; i++)
            switch (        InRoom.transform.GetChild(i).name)
            {
                case "Fuel": { Fuel.Add(          InRoom.transform.GetChild(i).gameObject); } break;
                case "Generator": { Generator.Add(InRoom.transform.GetChild(i).gameObject); } break;
                case "Scrap": { Scrap.Add(        InRoom.transform.GetChild(i).gameObject); } break;
                case "D-DEAD": { Dead.Add(        InRoom.transform.GetChild(i).gameObject); } break;
                default: { } break;
            }
        InRoom.SetActive(false);
        if(online)
            Text.GetComponent<NumberScript>().Energize();
    }
    void Update()
    {
        HandleScan();
        HandleExpousere();
    }
    //material
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name.StartsWith("B"))
            InRoom.SetActive(true);
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
    //Scan
    private bool isScanned = false;
    private float timeToDataLost = 0, timeToRefresh = 0;
    public void Scan(){//first time scan and refresh from drone
        if (!isScanned){
            isScanned = true;
            MaskScan.SetActive(true);
            if (ScanAble)
            {
                if (IsHereEnemy())
                {
                    MaskScan.GetComponent<MeshRenderer>().material = ScanEnemy;
                }
                else
                {
                    MaskScan.GetComponent<MeshRenderer>().material = ScanSafe;
                }
            }
            else
            {
                MaskScan.GetComponent<MeshRenderer>().material = ScanUknown;
            }
            timeToRefresh = 1;
        }
        timeToDataLost = 2;
    }
    private void HandleScan(){//stable scan
        if (isScanned){//if there is scan in progress
            timeToDataLost -= Time.deltaTime;//check IF scan IS in progress
            if (timeToDataLost < 0){//hide scan mask when done
                MaskScan.SetActive(false);
                isScanned = false;
            }

            //handle refresh IF needed
            if (ScanAble){//is scan able -> result other than yellow
                if (timeToRefresh < 0){
                    timeToRefresh -= Time.deltaTime;
                    timeToRefresh = 1;
                    if (IsHereEnemy()){
                        MaskScan.GetComponent<MeshRenderer>().material = ScanEnemy;
                    }else{
                        MaskScan.GetComponent<MeshRenderer>().material = ScanSafe;
                    }
                }
            }
        }
    }
    public bool IsHereEnemy()
    {
        bool isGood = false;
        GameMaster.Enemies.ForEach(e => {
            GameObject room = e.GetComponent<Robot>().GetRoomImIn();
            if(room != null)
                if (room.name == name && !isGood)
                    isGood = true;
        });
        return isGood;
    }
    //enviroment
    public void Exposed(bool Void, bool Radiation, DoorsScript responsibleOne){
        //if responsibleOne is null, we conside room breeched.
        //first time exposure => set timers rest will be handled in HandleExpousere()
        if (Void && !IsVoid && (VoidTakes < VoidIn)) {
            VoidIn = VoidTakes;
            this.responsibleOne = responsibleOne;
            GameMaster.WriteInConsole.Add("SYS: Room " + gameObject.name + " is vented into space!\n");
        }//if NOT contaminated AND NOT already counting
        if (Radiation && !IsContaminated && (RadiationTakes < RadiationIn)){
            RadiationIn = RadiationTakes;
            this.responsibleOne = responsibleOne;
            GameMaster.WriteInConsole.Add("SYS: Room " + gameObject.name + " is flooding with radiation!\n");
        }
            
    }
    private float HandleExpousereTimeStep = 0;
    private void HandleExpousere(){
        HandleExpousereTimeStep -= Time.deltaTime;
        if (HandleExpousereTimeStep<0){
            HandleExpousereTimeStep = 1;
            //sim update env once a sec
            if (!(VoidTakes < VoidIn))
            {//is beign exposed to void for first time
                VoidIn -= HandleExpousereTimeStep;
                if (VoidIn <= 0)
                {
                    VoidIn = VoidTakes + 1;
                    IsVoid = true;
                    VoidMarker.SetActive(true);
                    GameMaster.WriteInConsole.Add("SYS: Room " + gameObject.name + " is now void\n");
                }
            }
            if (!(RadiationTakes < RadiationIn))
            {//is beign exposed to radiation for first time
                RadiationIn -= HandleExpousereTimeStep;
                if (RadiationIn <= 0)
                {
                    RadiationIn = RadiationTakes + 1;
                    IsContaminated = true;
                    RadiationMarker.SetActive(true);
                    GameMaster.WriteInConsole.Add("SYS: Room " + gameObject.name + " is now contaminated\n");
                }
            }
            //update clocks if needed
            RecursiveCheckMarker = false;
            RecursiveQuick = false;
            //reset values if needed;
            if ((IsVoid || IsContaminated)&&responsibleOne!=null)//it there reason to care and is there source?
                if (!responsibleOne.open){//is source of this mess closed?
                    if (RecursiveExpousureCheckCaller(out var room, out var airlock)) {
                        room.QuickExpand(airlock);
                    }else{
                        QuickClean();
                        responsibleOne = null;
                    }
                }
            //check if hole sealed
            if (IsVoid || IsContaminated)
                Doors.ForEach(door => {
                    var script = door.GetComponent<DoorsScript>();
                    if (!script.airlock&&script.open){
                        script.roomA.GetComponent<RoomScript>().Exposed(IsVoid, IsContaminated, script);
                        script.roomB.GetComponent<RoomScript>().Exposed(IsVoid, IsContaminated, script);
                    }
                });
            //expand if posible (after check so that noting weird can happen if was source)
        }
    }
    private bool RecursiveCheckMarker = false, RecursiveQuick = false;
    public bool RecursiveExpousureCheckCaller(out RoomScript ExpandFromHere,out GameObject responsibleOne){
        ExpandFromHere = null;
        responsibleOne = null;
        if (RecursiveCheckMarker)
            return false;
        //was here before?

        RecursiveCheckMarker = true;
        for(int i = 0; i< Doors.Count; i++){
            var door = Doors[i].GetComponent<DoorsScript>();
            if (door.open){
                if (door.airlock){
                    ExpandFromHere = this;
                    responsibleOne = Doors[i];
                    return true;//this room has open airlock
                }
                else{
                    var room = door.roomA;
                    if (room == gameObject)
                        room = door.roomB;
                    if (room.GetComponent<RoomScript>().RecursiveExpousureCheckCaller(out var source,out var airlock))
                    {
                        ExpandFromHere = source; responsibleOne = airlock;
                        return true;//this room have ref to room with open airlock
                    }
                }
            }
        }
        return false;
        //no airlock found on this rooms OR any folowing
    }
    public void QuickExpand(GameObject responsibleOne){//ONLY overwrites responsible one IF not null (was flooded or in progress)
        if (RecursiveQuick)
            return;
        //was here before?
        if (responsibleOne == null)
            return;
        //is there responsible one to overwrite?
        RecursiveQuick = true;
        this.responsibleOne = responsibleOne.GetComponent<DoorsScript>();
        //fix room

        for (int i = 0; i < Doors.Count; i++)
        {
            var door = Doors[i].GetComponent<DoorsScript>();
            if (door.open)
            {
                if (!door.airlock){
                    var room = door.roomA;
                    if (room == gameObject)
                        room = door.roomB;
                    room.GetComponent<RoomScript>().QuickExpand(Doors[i]);
                }
            }
        }
        //expand change
    }
    public void QuickClean(){//cleans ALL following rooms that have responsible one != null
        if (RecursiveQuick)
            return;
        //was here before?
        if (responsibleOne == null)
            return;
        //is there responsible one to overwrite?
        RecursiveQuick = true;
        responsibleOne = null;
        IsVoid = false;
        VoidIn = VoidTakes + 1;
        RadiationIn = RadiationTakes + 1;

        VoidMarker.SetActive(false);
        //fix room

        for (int i = 0; i < Doors.Count; i++)
        {
            var door = Doors[i].GetComponent<DoorsScript>();
            if (door.open)
            {
                if (!door.airlock)
                {
                    var room = door.roomA;
                    if (room == gameObject)
                        room = door.roomB;
                    room.GetComponent<RoomScript>().QuickClean();
                }
            }
        }
        //expand change
    }
    public void StoryClean()
    {
        IsContaminated = false;
        RadiationIn = RadiationTakes + 1;
        RadiationMarker.SetActive(false);
    }
}
