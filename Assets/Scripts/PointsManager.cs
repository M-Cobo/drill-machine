using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointsManager : MonoBehaviour
{
    private int mineralsCollected = 0;
    public int MineralsCollected 
    {
        get { return mineralsCollected; }
    }

    private UIManager uiManager = null;

    private void Awake() 
    {
        uiManager = this.GetComponent<UIManager>();
    }

    public void AddMinerals(int x) 
    { 
        mineralsCollected += x;
        
        uiManager.UpdatePointsUI(MineralsCollected);
    }
}
