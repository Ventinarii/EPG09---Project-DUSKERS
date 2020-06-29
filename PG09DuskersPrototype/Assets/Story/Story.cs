using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class Story : MonoBehaviour
{
    public bool Active,IsOneWay,Trigered;
    /*#Active    => is listening for trigger
      #IsOneWay  => Can be activated multiple times
      #Triggered => Was triggered -> if triggered it will exec it's program ONCE
      in order to be triggered again CheckIfTriggered() must return false and then true again.

      TODO => add some resonable way to have function active when triggered or not -> for example in case there is task like
      cut throu door and we need some time (120 sec) to do it but our drone can disengage and do other tings in meantime.
      probably through c# as i have no other idea how to make efficeint script engine so and NO.
    */
    [TextArea(1, 200)]
    public string ProtoSkrypt_WriteCustomActionsHere_ExecOnActivate;
    [TextArea(1, 200)]
    public string SayOnActivate;
    
    public List<GameObject> PlayerOrEnemy;
    public List<string> ToolOfPlayer;
    public float WaitFor, triggeredFor = 0;

    [TextArea(1, 200)]
    public string ProtoSkrypt_WriteCustomActionsHere_ExecOnTrigger;
    public GameObject UnityIsNotAllowingInterfacesGreat;
    [TextArea(1, 200)]
    public string SayOnTriggered;
    public List<GameObject> StoryiesToActivate;
    public List<GameObject> StoryiesToDeactivate;
    
    void Start()
    {
        if (Active)
            Activate();
    }            //done
    void Update()
    {
        if (Active && 0 <= WaitFor)
        {
            if (0 < WaitFor) 
                WaitFor -= Time.deltaTime;
            if (WaitFor < 0)
            {
                WaitFor--;
                if (CheckIfTriggered())
                    ExecTrigerred();
            }
        }
        if (Active && Trigered)
            triggeredFor += Time.deltaTime;
    }           //done

    public void Activate()
    {
        Active = true;
        ExecCustomCode(ProtoSkrypt_WriteCustomActionsHere_ExecOnActivate);
        GameMaster.WriteInConsole.AddRange(SplitStringToArr(SayOnActivate));
    }  //done
    public void DeActivate()
    {
        Active = false;
    }//done

    private List<GameObject> InRange = new List<GameObject>();
    private void OnTriggerEnter(Collider other)
    {
        InRange.Add(other.gameObject);
        if(Active)
            if (CheckIfTriggered())
                ExecTrigerred();
    }//done
    private void OnTriggerExit(Collider other)
    {
        InRange.Remove(other.gameObject);
        if (Active)
            if (CheckIfTriggered())
                ExecTrigerred();
    } //done

    private bool CheckIfTriggered() {
        bool trig = false;
        if (!trig && WaitFor < 0)//check for time passed
            trig = true;
        if(!trig) {//check for contact
            PlayerOrEnemy.ForEach(p => {
                if (!trig && InRange.Contains(p))
                    trig = true;
            });
        }
        if (!trig) {//check for tools

        }

        if (!Trigered && trig){//if it is triggered for the first time
            Trigered = true;
            return true;
        }
        if (Trigered && !trig && !IsOneWay)//if is NOT one way can REARM
            Trigered = false;

        return false;
    }        //todo--------------
    private void ExecTrigerred() {
        ExecCustomCode(ProtoSkrypt_WriteCustomActionsHere_ExecOnTrigger);
        try
        {
            if (UnityIsNotAllowingInterfacesGreat != null)
                UnityIsNotAllowingInterfacesGreat.GetComponent<SPI>().Run(this);
        }
        catch (Exception e) { GameMaster.Console.text += e.ToString(); }
        GameMaster.WriteInConsole.AddRange(SplitStringToArr(SayOnTriggered));
        StoryiesToActivate.ForEach(s =>
        {
            try
            {
                s.GetComponent<Story>().Activate();
            }
            catch (Exception e) { GameMaster.Console.text += e.ToString(); }
        });
        StoryiesToDeactivate.ForEach(s =>
        {
            try
            {
                s.GetComponent<Story>().DeActivate();
            }
            catch (Exception e) { GameMaster.Console.text += e.ToString(); }
        });
    }           //done
    private void ExecCustomCode(string str) {
        GameMaster.StoryCommands(this, str);
    }//done

    private readonly int HowLong = 5;//assuming in seconds and 60 FPS
    private List<string> SplitStringToArr(string input)
    {
        List<string> list = new List<string>();
        var chars = input.ToCharArray();
        var howManyCharsTotal = chars.Length;
        double howManyPerPacket = (double)howManyCharsTotal / (double)(HowLong * 60);
        //how long we want to speak? i think some ~ 5 sek i good enough

        string aggregator = "";
        double whenSendPacket = howManyPerPacket;
        for (int index = 0; index < howManyCharsTotal; index ++){
            if(whenSendPacket <= index){
                whenSendPacket += howManyPerPacket;
                list.Add(aggregator);
                aggregator = "";
            }
            aggregator += chars[index];
        }
        whenSendPacket += howManyPerPacket;
        list.Add(aggregator);
        aggregator = "";

        return list;
    }
}
