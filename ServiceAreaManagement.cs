using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServiceAreaManagement
{
    // public 없었는데 220714 추가
    public ServiceArea[] ServiceArea_State; // ServiceArea_State : ServiceArea 클래스를 여러 개 갖고 있는 array, 즉 서비스 구역 이름, 함재기 이름, 함재기 자세 정보를 갖고 있는 클래스

    public ServiceAreaManagement() //int numofservicearea)
    {
        //ServiceArea_State = new ServiceArea[numofservicearea];
    }
        
    public void SetServiceArea(int numofservicearea)
    {
        ServiceArea_State = new ServiceArea[numofservicearea]; // numofservicearea으로 들어오는 개수만큼 ServiceArea가 생성
    }

    public void AddInfo(int n, string Areaname, string Aircraftname, float angle) // AddInfo 함수를 사용해서 정보를 담아주면 해당 ServiceArea_State[n] != null
    {
        ServiceArea_State[n] = new ServiceArea(Areaname);
        ServiceArea_State[n].SetAircraft(angle, Aircraftname);
    }

    public List<int> GetEmptyList()
    {
        List<int> Result = new List<int>();

        for (int i = 0; i < ServiceArea_State.Length; i++) // ServiceArea_State.Length : 항상 처음에 지정해준 값(10개)
        {
            if (ServiceArea_State[i] == null)
            {
                Result.Add(i);
            }
        }
        return Result;
    }
}
