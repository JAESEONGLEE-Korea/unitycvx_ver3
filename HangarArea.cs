using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HangarArea // ���񱸿��� �� �̸�, ����� �̸� ������ ���ϰ� �ִ� Ŭ����
{
    string Area_Name;
    string Aircraft_Name;

    // �ٱ����� ���� �����ϰ� �Ϸ��� ���� Property
    public string AREA_NAME { get { return Area_Name; } }
    public string AIRCRAFT_NAME { get { return Aircraft_Name; } }

    public HangarArea(string name) // HangarArea Ŭ������ ������
    {
        Area_Name = name;
    }

    public void SetAircraft(string name = null) // ����� �̸�
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
