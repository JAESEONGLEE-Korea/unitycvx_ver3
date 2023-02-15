using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangarArea // 정비구역의 각 이름, 함재기 이름 정보를 지니고 있는 클래스
{
    string Area_Name;
    string Aircraft_Name;

    // 바깥에서 접근 가능하게 하려고 만든 Property
    public string AREA_NAME { get { return Area_Name; } }
    public string AIRCRAFT_NAME { get { return Aircraft_Name; } }

    public HangarArea(string name) // HangarArea 클래스의 생성자
    {
        Area_Name = name;
    }

    public void SetAircraft(string name = null) // 함재기 이름
    {
        if (name != null)
        {
            Aircraft_Name = name;
        }

        else
        {
            Aircraft_Name = null;
        }
    }
}
