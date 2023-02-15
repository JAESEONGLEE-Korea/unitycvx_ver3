using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServiceArea // 서비스 구역 이름, 함재기 이름, 함재기 자세 정보를 지니고 있는 클래스
{
    string Area_Name;
    string Aircraft_Name;
    float Aircraft_Posture;

    // 바깥에서 접근 가능하게 하려고 만든 Property
    public string AREA_NAME { get { return Area_Name; } }
    public string AIRCRAFT_NAME { get { return Aircraft_Name; } }
    public float AIRCRAFT_POSTURE { get { return Aircraft_Posture; } }

    public ServiceArea(string name) // ServiceArea 클래스의 생성자
    {
        Area_Name = name; // 구역 이름
    }

    public void SetAircraft(float angle, string name=null) // 함재기 이름
    {
        if (name != null)
        {
            Aircraft_Name = name;
            Aircraft_Posture = angle;
        }

        else
        {
            Aircraft_Name = null;
            Aircraft_Posture = 0.0f;
        }
    }
}
