using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangarAreaManagement
{
    // public 없었는데 220714 추가
    public HangarArea[] HangarArea_State;

    public HangarAreaManagement()
    {

    }

    public void SetHangarArea(int numofhangararea)
    {
        HangarArea_State = new HangarArea[numofhangararea];
    }

    public void AddInfo(int n, string Areaname, string Aircraftname)
    {
        HangarArea_State[n] = new HangarArea(Areaname);
        HangarArea_State[n].SetAircraft(Aircraftname);
    }

    public List<int> GetEmptyList()
    {
        List<int> Result = new List<int>();

        for (int i = 0; i < HangarArea_State.Length; i++) // ServiceArea_State.Length : 항상 처음에 지정해준 값(10개)
        {
            if (HangarArea_State[i] == null)
            {
                Result.Add(i);
            }
        }
        return Result;
    }
}
