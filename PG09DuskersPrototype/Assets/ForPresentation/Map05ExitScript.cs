using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Map05ExitScript : MonoBehaviour
{
    public Button RESTART, EXIT;
    public Text RES, EXI;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private bool selected = true;
    private void Update()
    {
        if (selected)
        {
            RESTART.GetComponent<Image>().color = Color.green;
            RES.color = Color.black;
            EXIT.GetComponent<Image>().color = Color.black;
            EXI.color = Color.green;
        }
        else
        {
            RESTART.GetComponent<Image>().color = Color.black;
            RES.color = Color.green;
            EXIT.GetComponent<Image>().color = Color.green;
            EXI.color = Color.black;
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
            selected = !selected;
        if (Input.GetKeyDown(KeyCode.Return))
            if (selected) { Restart(); } else { Exit(); }
    }
    public void Restart()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(1);
    }
    public void Exit()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Application.Quit(0);
    }
}
