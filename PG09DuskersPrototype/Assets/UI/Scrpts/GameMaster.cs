using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = System.Random;
public class GameMaster : MonoBehaviour
{
    public Text NonStaticConsole, NonStaticStats;
    public static Text Console, Stats;
    public InputField NonStaticConsoleEntry;
    public static InputField ConsoleEntry;

    public static List<GameObject> Robots = new List<GameObject>();
    public static List<GameObject> Enemies = new List<GameObject>();
    public static List<GameObject> Rooms = new List<GameObject>();
    public static List<GameObject> Doors = new List<GameObject>();
    public static List<GameObject> Airlocks = new List<GameObject>();

    public static int scrapCollected = 0, ftlCollected = 0, stlCollected = 0;

    void Start()
    {
        Robots = new List<GameObject>();
        Enemies = new List<GameObject>();
        Rooms = new List<GameObject>();
        Doors = new List<GameObject>();
        Airlocks = new List<GameObject>();

        Console = NonStaticConsole;
        Stats = NonStaticStats;
        ConsoleEntry = NonStaticConsoleEntry;
        cam = GetComponent<Camera>();
        for (int i = 1; i <= 6; i++)
        {
            string name = "B";
            if (i < 10)
                name += "0";
            name += i;
            GameObject robot = GameObject.Find(name);
            if (robot != null)
                Robots.Add(robot);
        }//robots
        for (int i = 1; i <= 255; i++)
        {
            string name = "E";
            if (i < 10)
                name += "0";
            name += i;
            GameObject enemy = GameObject.Find(name);
            if (enemy != null)
            {
                Enemies.Add(enemy);
            }
            else
                i = 255;
        }//enemies
        for (int i = 1; i <= 255; i++)
        {
            string name = "R";
            if (i < 10)
                name += "0";
            name += i;
            GameObject room = GameObject.Find(name);
            if (room != null)
            {
                Rooms.Add(room);
            }
            else
                i = 255;
        }//rooms
        for (int i = 1; i <= 255; i++)
        {
            string name = "D";
            if (i < 10)
                name += "0";
            name += i;
            GameObject door = GameObject.Find(name);
            if (door != null)
            {
                Doors.Add(door);
            }
            else
                i = 255;
        }//doors
        for (int i = 1; i <= 255; i++)
        {
            string name = "A";
            if (i < 10)
                name += "0";
            name += i;
            GameObject airlock = GameObject.Find(name);
            if (airlock != null)
            {
                Airlocks.Add(airlock);
            }
            else
                i = 255;
        }//airlocks
    }
    private bool afterFocusLose = false;
    void Update()
    {
        MoveCam();
        if (Input.GetKeyDown(KeyCode.Return))
            ParseComand();
        CarryOutCommands();
        ListStats();

        if (WriteInConsole.Count != 0)
        {
            Console.text += WriteInConsole[0];
            WriteInConsole.RemoveAt(0);
        }

        if (!ConsoleEntry.isFocused) {
            EventSystem.current.SetSelectedGameObject(ConsoleEntry.gameObject, null);
            ConsoleEntry.OnPointerClick(new PointerEventData(EventSystem.current));
            ConsoleEntry.selectionAnchorPosition = ConsoleEntry.selectionFocusPosition;
            afterFocusLose = true;
        }else if (afterFocusLose) {
            ConsoleEntry.caretPosition = ConsoleEntry.text.Length;
            afterFocusLose = false;
        }
    }

    private Vector3 axisZ = new Vector3(0, 0, 1), axisX = new Vector3(1, 0, 0);
    public float panningSpeed = 10;
    private void MoveCam()
    {
        if (Input.GetKey(KeyCode.UpArrow))
            transform.position += axisZ * panningSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.DownArrow))
            transform.position -= axisZ * panningSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.RightArrow))
            transform.position += axisX * panningSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftArrow))
            transform.position -= axisX * panningSpeed * Time.deltaTime;
        MoveCamMouse();
    }

    private float statsDealy = 0;
    private void ListStats(){
        statsDealy -= Time.deltaTime;
        if (statsDealy < 0)
        {
            statsDealy = .5f;
            Stats.text = "Robots status: \n";
            for (int x = 0; x < 6; x++)
            {
                string hp = "N/A";
                if (x < Robots.Count)
                {
                    Robot robot = Robots[x].GetComponent<Robot>();
                    if (robot.hp < 0)
                        hp = "00";
                    else
                        hp = robot.hp + "";
                }
                Stats.text += "Robot: " + (x + 1) + "|hp: " + hp + "tools: <tbd> \n";
            }
            Stats.text += "Fuel Collected: FTL: " + ftlCollected + " STL: " + stlCollected + " |" +
                          "Scrap Collected: " + scrapCollected + "\n";
            Stats.text += "Addctional notes: (...)";

            var lines = Console.text.Split('\n');
            for (int i = 0; (32 <= lines.Length - i); i++)
            {
                Console.text = Console.text.Remove(0, lines[i].Length + 1);
            }
        }
    }zazAXzzzaXaaza

    private List<string> commands = new List<string>();
    public static List<string> WriteInConsole = new List<string>();//ponieważ unity ma 'problem'  z startem i zmiennumi stałymi DLA STORY

    public static bool GeenLight = true;
    public static GameObject GetRobotForEnemy(Vector3 forPos)
    {
        if (Robots.Count == 0)
            return null;

        GameObject robot = Robots.OrderBy(r => Vector3.Distance(r.transform.position, forPos)).First();
        return robot;
    }
    private void ParseComand()
    {
        string commandRAW = ConsoleEntry.text;
        ConsoleEntry.text = "";
        WriteInConsole.Add("USR:" + commandRAW + "\n");
        string[] commandsARR = commandRAW.Split(';');
        for (int i = 0; i < commandsARR.Length; i++)
            commands.Add(commandsARR[i]);
    }//jako że C# odpowiednik mapy z Java jest słaby- to zastowałem tu switch. wiem, słabe, ale na razie wystarczy.
    private void CarryOutCommands()
    {
        if (GeenLight && commands.Count != 0)
        {
            try
            {
                GeenLight = false;
                string cmdComplete = commands[0];
                string[] cmd = cmdComplete.Trim().Split(' ');
                commands.RemoveAt(0);

                {
                    byte A = (byte)'A', a = (byte)'a', delta = (byte)(a - A);
                    for (byte i = (byte)'A'; i <= (byte)'Z'; i++)
                    {
                        char source = (char)i, target = (char)((byte)(i + delta));
                        cmd[0] = cmd[0].Replace(source, target);
                    }
                }

                switch (cmd[0])
                {
                    case "door":
                        {
                            for (int i = 1; i < cmd.Length; i++)
                            {
                                if (!Int32.TryParse(cmd[i], out int result))
                                {
                                    WriteInConsole.Add("SYS:" + cmd[i] + " is NaN;\n");
                                }
                                else if (result < 1 || Doors.Count <= (result - 1))
                                {
                                    WriteInConsole.Add("SYS:no doors with index: " + result + " found;\n");
                                }
                                else
                                {
                                    Doors[result - 1].GetComponent<DoorsScript>().SwapDoor();
                                }
                            }
                        }
                        break;
                    case "airlock":
                        {
                            for (int i = 1; i < cmd.Length; i++)
                            {
                                if (!Int32.TryParse(cmd[i], out int result))
                                {
                                    WriteInConsole.Add("SYS:" + cmd[i] + " is NaN;\n");
                                }
                                else if (result < 1 || Airlocks.Count <= (result - 1))
                                {
                                    WriteInConsole.Add("SYS:no doors with index: " + result + " found;\n");
                                }
                                else
                                {
                                    Airlocks[result - 1].GetComponent<DoorsScript>().SwapDoor();
                                }
                            }
                        }
                        break;
                    case "nav":
                        {
                            string[] split = cmdComplete.Split('t'),
                                     robots = split[0].Split(' ');
                            if (!Int32.TryParse(split[1].Trim(), out int roomIndex))
                            {
                                WriteInConsole.Add("SYS:" + cmd[1].Trim() + " is NaN;\n");
                            }
                            else if (roomIndex < 1 || Rooms.Count <= (roomIndex - 1))
                            {
                                WriteInConsole.Add("SYS:no room with index: " + roomIndex + " found;\n");
                            }
                            else
                            {
                                Vector3 target = Rooms[roomIndex - 1].transform.position;
                                for (int i = 1; i < robots.Length; i++)
                                {
                                    if (!String.IsNullOrWhiteSpace(robots[i]))
                                        if (!Int32.TryParse(robots[i], out int result))
                                        {
                                            WriteInConsole.Add("SYS:" + robots[i] + " is NaN;\n");
                                        }
                                        else if (result < 1 || Robots.Count <= (result - 1))
                                        {
                                            WriteInConsole.Add("SYS:no robot with index: " + result + " found;\n");
                                        }
                                        else
                                        {
                                            Robots[result - 1].GetComponent<Robot>().NavigateTo(target);
                                        }
                                }
                            }
                        }
                        break;
                    case "gather":
                        {
                            if (!Int32.TryParse(cmd[1], out int result))
                            {
                                WriteInConsole.Add("SYS:" + cmd[1] + " is NaN;\n");
                            }
                            else if (result < 1 || Robots.Count <= (result - 1))
                            {
                                WriteInConsole.Add("SYS:no robot with index: " + result + " found;\n");
                            }
                            else
                            {
                                Robots[result - 1].GetComponent<Robot>().Gather();
                            }
                        }
                        break;
                    case "gen":
                        {
                            if (!Int32.TryParse(cmd[1], out int result))
                            {
                                WriteInConsole.Add("SYS:" + cmd[1] + " is NaN;\n");
                            }
                            else if (result < 1 || Robots.Count <= (result - 1))
                            {
                                WriteInConsole.Add("SYS:no robot with index: " + result + " found;\n");
                            }
                            else
                            {
                                Robots[result - 1].GetComponent<Robot>().Generator();
                            }
                        }
                        break;
                    case "scan":
                        {
                            if (!Int32.TryParse(cmd[1], out int result))
                            {
                                WriteInConsole.Add("SYS:" + cmd[1] + " is NaN;\n");
                            }
                            else if (result < 1 || Robots.Count <= (result - 1))
                            {
                                WriteInConsole.Add("SYS:no robot with index: " + result + " found;\n");
                            }
                            else
                            {
                                Robots[result - 1].GetComponent<Robot>().Scanner();
                            }
                        }
                        break;
                    case "pow":
                        {
                            string[] split = cmdComplete.Split('t'),
                                     robots = split[0].Split(' ');
                            if (!Int32.TryParse(split[1].Trim(), out int doorIndex))
                            {
                                WriteInConsole.Add("SYS:" + cmd[1].Trim() + " is NaN;\n");
                            }
                            else if (doorIndex < 1 || Doors.Count <= (doorIndex - 1))
                            {
                                WriteInConsole.Add("SYS:no door with index: " + doorIndex + " found;\n");
                            }
                            else
                            {
                                GameObject target = Doors[doorIndex - 1];
                                for (int i = 1; i < robots.Length; i++)
                                {
                                    if (!String.IsNullOrWhiteSpace(robots[i]))
                                        if (!Int32.TryParse(robots[i], out int result))
                                        {
                                            WriteInConsole.Add("SYS:" + robots[i] + " is NaN;\n");
                                        }
                                        else if (result < 1 || Robots.Count <= (result - 1))
                                        {
                                            WriteInConsole.Add("SYS:no robot with index: " + result + " found;\n");
                                        }
                                        else
                                        {
                                            Robots[result - 1].GetComponent<Robot>().Cowbar(target);
                                        }
                                }
                            }
                        }
                        break;
                    case "help":
                        {
                            ListCommands();
                        }
                        break;
                    case "exit":
                        {
                            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                        }
                        break;
                    case "cls": { Console.text = ""; } break;
                    default:
                        {
                            Console.text += "SYS:" + cmdComplete + " not recognized as internal command;\n";
                            GeenLight = true;
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                Console.text += "SYS=>Error:" + e + ";\n";
                GeenLight = true;
            }
        }
    }
    private void ListCommands()
    {
        WriteInConsole.Add(@"Commdans:
door <door index> => opens/closes doors
nav <drone index> t <room index> => navigates selected drones to selected room
gather <drone index> => orders selected drone to gather all items in room
gen <drone index> => orders given drone to power generator in it's room if such exist
scan <drone index> => allow drone to scan it's environment for enemies
pow <drone index> => <door  index> => orders drone to force open selected doors
help => displays this message
exit =>  finishes mission -> notice that drones get killed if outside of R1 when you type this.");
    }

    public static void StoryCommands(Story story, string command)
    {
        if (1 <= command.Length)
            new List<string>(command.Split(';')).ForEach(cmdP =>
            {
                try
                {
                    string cmdComplete = cmdP.Trim();
                    string[] cmd = cmdComplete.Trim().Split(' ');
                    {
                        byte A = (byte)'A', a = (byte)'a', delta = (byte)(a - A);
                        for (byte i = (byte)'A'; i <= (byte)'Z'; i++)
                        {
                            char source = (char)i, target = (char)((byte)(i + delta));
                            cmd[0] = cmd[0].Replace(source, target);
                        }
                    }
                    switch (cmd[0])
                    {
                        case "door":{
                                for (int i = 1; i < cmd.Length; i++)
                                {
                                    if (!Int32.TryParse(cmd[i], out int result))
                                    {
                                        Console.text += "SYS:" + cmd[i] + " is NaN;\n";
                                    }
                                    else if (result < 1 || Doors.Count <= (result - 1))
                                    {
                                        Console.text += "SYS:no doors with index: " + result + " found;\n";
                                    }
                                    else
                                    {
                                        Doors[result - 1].GetComponent<DoorsScript>().SwapDoor();
                                    }
                                }
                            }break;
                        case "airlock":{
                                for (int i = 1; i < cmd.Length; i++)
                                {
                                    if (!Int32.TryParse(cmd[i], out int result))
                                    {
                                        Console.text += "SYS:" + cmd[i] + " is NaN;\n";
                                    }
                                    else if (result < 1 || Airlocks.Count <= (result - 1))
                                    {
                                        Console.text += "SYS:no doors with index: " + result + " found;\n";
                                    }
                                    else
                                    {
                                        Airlocks[result - 1].GetComponent<DoorsScript>().SwapDoor();
                                    }
                                }
                            }break;
                        case "nav":{
                                string[] split = cmdComplete.Split('t'),
                                         robots = split[0].Split(' ');
                                if (!Int32.TryParse(split[1].Trim(), out int roomIndex))
                                {
                                    Console.text += "SYS:" + cmd[1].Trim() + " is NaN;\n";
                                }
                                else if (roomIndex < 1 || Rooms.Count <= (roomIndex - 1))
                                {
                                    Console.text += "SYS:no room with index: " + roomIndex + " found;\n";
                                }
                                else
                                {
                                    Vector3 target = Rooms[roomIndex - 1].transform.position;
                                    for (int i = 1; i < robots.Length; i++)
                                    {
                                        if (!String.IsNullOrWhiteSpace(robots[i]))
                                            if (!Int32.TryParse(robots[i], out int result))
                                            {
                                                Console.text += "SYS:" + robots[i] + " is NaN;\n";
                                            }
                                            else if (result < 1 || Robots.Count <= (result - 1))
                                            {
                                                Console.text += "SYS:no robot with index: " + result + " found;\n";
                                            }
                                            else
                                            {
                                                Robots[result - 1].GetComponent<Robot>().NavigateTo(target);
                                            }
                                    }
                                }
                            }break;
                        case "gather":{
                                if (!Int32.TryParse(cmd[1], out int result))
                                {
                                    Console.text += "SYS:" + cmd[1] + " is NaN;\n";
                                }
                                else if (result < 1 || Robots.Count <= (result - 1))
                                {
                                    Console.text += "SYS:no robot with index: " + result + " found;\n";
                                }
                                else
                                {
                                    Robots[result - 1].GetComponent<Robot>().Gather();
                                }
                            }break;
                        case "gen":{
                                if (!Int32.TryParse(cmd[1], out int result))
                                {
                                    Console.text += "SYS:" + cmd[1] + " is NaN;\n";
                                }
                                else if (result < 1 || Robots.Count <= (result - 1))
                                {
                                    Console.text += "SYS:no robot with index: " + result + " found;\n";
                                }
                                else
                                {
                                    Robots[result - 1].GetComponent<Robot>().Generator();
                                }
                            }break;
                        case "scan":{
                                if (!Int32.TryParse(cmd[1], out int result))
                                {
                                    Console.text += "SYS:" + cmd[1] + " is NaN;\n";
                                }
                                else if (result < 1 || Robots.Count <= (result - 1))
                                {
                                    Console.text += "SYS:no robot with index: " + result + " found;\n";
                                }
                                else
                                {
                                    Robots[result - 1].GetComponent<Robot>().Scanner();
                                }
                            }break;
                        case "pow":{
                                string[] split = cmdComplete.Split('t'),
                                         robots = split[0].Split(' ');
                                if (!Int32.TryParse(split[1].Trim(), out int doorIndex))
                                {
                                    Console.text += "SYS:" + cmd[1].Trim() + " is NaN;\n";
                                }
                                else if (doorIndex < 1 || Doors.Count <= (doorIndex - 1))
                                {
                                    Console.text += "SYS:no door with index: " + doorIndex + " found;\n";
                                }
                                else
                                {
                                    GameObject target = Doors[doorIndex - 1];
                                    for (int i = 1; i < robots.Length; i++)
                                    {
                                        if (!String.IsNullOrWhiteSpace(robots[i]))
                                            if (!Int32.TryParse(robots[i], out int result))
                                            {
                                                Console.text += "SYS:" + robots[i] + " is NaN;\n";
                                            }
                                            else if (result < 1 || Robots.Count <= (result - 1))
                                            {
                                                Console.text += "SYS:no robot with index: " + result + " found;\n";
                                            }
                                            else
                                            {
                                                Robots[result - 1].GetComponent<Robot>().Cowbar(target);
                                            }
                                    }
                                }
                            }break;
                        case "exit":{
                                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                            }break;
                        case "cls": { Console.text = ""; } break;
                        case "decontaminate": {
                                for (int i = 1; i < cmd.Length; i++)
                                {
                                    if (!Int32.TryParse(cmd[i], out int result))
                                    {
                                        Console.text += "SYS:" + cmd[i] + " is NaN;\n";
                                    }
                                    else if (result < 1 || Rooms.Count <= (result - 1))
                                    {
                                        Console.text += "SYS:no room with index: " + result + " found;\n";
                                    }
                                    else
                                    {
                                        Rooms[result - 1].GetComponent<RoomScript>().StoryClean();
                                    }
                                }
                            } break;
                        default:{
                                Console.text += "SYS:" + cmdComplete + " not recognized as internal command;\n";
                                GeenLight = true;
                            }break;
                    }
                }
                catch (Exception e)
                {
                    Console.text += "SYS=>Error:" + e + ";\n";
                    GeenLight = true;
                }
            });
    }

    //obsługa kamery https://kylewbanks.com/blog/unity3d-panning-and-pinch-to-zoom-camera-with-touch-and-mouse-input
    private Camera cam;
    private Vector3 lastPanPosition;
    private int panFingerId;
    private bool wasZoomingLastFrame;
    private Vector2[] lastZoomPositions;
    private readonly float PanSpeed = 55f, ZoomSpeedMouse = 20f;
    private readonly float[] BoundsX = new float[] { -10f, 100f }, BoundsZ = new float[] { -100f, 10f },
                             ZoomBounds = new float[] { 5f, 60f };
    private void MoveCamMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            lastPanPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            PanCamera(Input.mousePosition);
        }
        ZoomCamera(Input.GetAxis("Mouse ScrollWheel"), ZoomSpeedMouse);
    }
    void PanCamera(Vector3 newPanPosition) {
        // Determine how much to move the camera
        Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
        float PanSpeed = this.PanSpeed * (cam.fieldOfView / ZoomBounds[1]);
        Vector3 move = new Vector3(offset.x * PanSpeed, 0, offset.y * PanSpeed);

        // Perform the movement
        transform.Translate(move, Space.World);

        // Ensure the camera remains within bounds.
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(transform.position.x, BoundsX[0], BoundsX[1]);
        pos.z = Mathf.Clamp(transform.position.z, BoundsZ[0], BoundsZ[1]);
        transform.position = pos;

        // Cache the position
        lastPanPosition = newPanPosition;
    }
    void ZoomCamera(float offset, float speed) {
        if (offset == 0)
        {
            return;
        }
        cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - (offset * speed), ZoomBounds[0], ZoomBounds[1]);
    }
}
