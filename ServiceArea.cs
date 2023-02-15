using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServiceArea // ���� ���� �̸�, ����� �̸�, ����� �ڼ� ������ ���ϰ� �ִ� Ŭ����
{
    string Area_Name;
    string Aircraft_Name;
    float Aircraft_Posture;

    // �ٱ����� ���� �����ϰ� �Ϸ��� ���� Property
    public string AREA_NAME { get { return Area_Name; } }
    public string AIRCRAFT_NAME { get { return Aircraft_Name; } }
    public float AIRCRAFT_POSTURE { get { return Aircraft_Posture; } }

    public ServiceArea(string name) // ServiceArea Ŭ������ ������
    {
        Area_Name = name; // ���� �̸�
    }

    public void SetAircraft(float angle, string name=null) // ����� �̸�
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
