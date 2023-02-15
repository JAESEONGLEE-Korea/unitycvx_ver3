using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangarAreaManagement
{
    // public �����µ� 220714 �߰�
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

        for (int i = 0; i < HangarArea_State.Length; i++) // ServiceArea_State.Length : �׻� ó���� �������� ��(10��)
        {
            if (HangarArea_State[i] == null)
            {
                Result.Add(i);
            }
        }
        return Result;
    }
}
