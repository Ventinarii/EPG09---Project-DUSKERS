using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrapOrFuel : MonoBehaviour
{
    public int Scrap = 0, FTL = 0, STL = 0;
    public List<GameObject> lights;
    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.name.Equals("Fuel")) {
            if (FTL == 0 && STL == 0)
            {
                FTL = 1;
                STL = 3;
            }
        } else  {
            if(Scrap == 0)
            {
                Scrap = 1;
            }
        }
    }
    public void Collect()
    {
        GameMaster.scrapCollected += Scrap;
        GameMaster.ftlCollected += FTL;
        GameMaster.stlCollected += STL;
        Scrap = 0;
        FTL = 0;
        STL = 0;
        if (gameObject.name.Equals("Fuel")){
            lights.ForEach(light => light.GetComponent<Animator>().SetTrigger("TurnRed"));
        }
    }
}
