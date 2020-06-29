using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NumberScript : MonoBehaviour
{
    public GameObject text;
    // Start is called before the first frame update
    void Start()
    {
        string str = transform.parent.name.Remove(0,1);
        text.GetComponent<TextMeshPro>().text = str;
    }
    public void Energize()
    {
        text.GetComponent<TextMeshPro>().color = new Color(0,1,0,1);

    }
    public void DeEnergize()
    {
        text.GetComponent<TextMeshPro>().color = new Color(1, 1, 1, 1);
    }
}
