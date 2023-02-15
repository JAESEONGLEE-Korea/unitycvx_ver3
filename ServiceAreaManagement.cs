using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServiceAreaManagement
{
    // public �����µ� 220714 �߰�
    public ServiceArea[] ServiceArea_State; // ServiceArea_State : ServiceArea Ŭ������ ���� �� ���� �ִ� array, �� ���� ���� �̸�, ����� �̸�, ����� �ڼ� ������ ���� �ִ� Ŭ����

    public ServiceAreaManagement() //int numofservicearea)
    {
        //ServiceArea_State = new ServiceArea[numofservicearea];
    }
        
    public void SetServiceArea(int numofservicearea)
    {
        ServiceArea_State = new ServiceArea[numofservicearea]; // numofservicearea���� ������ ������ŭ ServiceArea�� ����
    }

    public void AddInfo(int n, string Areaname, string Aircraftname, float angle) // AddInfo �Լ��� ����ؼ� ������ ����ָ� �ش� ServiceArea_State[n] != null
    {
        ServiceArea_State[n] = new ServiceArea(Areaname);
        ServiceArea_State[n].SetAircraft(angle, Aircraftname);
    }

    public List<int> GetEmptyList()
    {
        List<int> Result = new List<int>();

        for (int i = 0; i < ServiceArea_State.Length; i++) // ServiceArea_State.Length : �׻� ó���� �������� ��(10��)
        {
            if (ServiceArea_State[i] == null)
            {
                Result.Add(i);
            }
        }
        return Result;
    }
}
