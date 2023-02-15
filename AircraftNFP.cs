using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NFP;
using System;

public class AircraftNFP : MonoBehaviour
{
    public GameObject F35B_Object; // ������ ��ǥ �����
    public GameObject Obstacle; // ��ֹ� �����
    public GameObject dot; // Polygon �׸�����

    double s_Angle; // ��ֹ� ����� x ����
    double o_Angle; // ��ǥ ����� x ����
    VERTEX s_location; // ��ֹ� ����� ��ġ ��ǥ

    List<Tuple<VERTEX, VERTEX>> NFPResult; // ��ǥ�� ���� NFP ����?
    List<VERTEX> S = new List<VERTEX>();

    // Start is called before the first frame update
    void Start()
    {
        #region
        S.Add(new VERTEX(5.3, 0.275));
        S.Add(new VERTEX(4.75, 0.275));
        S.Add(new VERTEX(4.65, 0.4));
        S.Add(new VERTEX(4.375, 0.4));
        S.Add(new VERTEX(4.314, 0.579));
        S.Add(new VERTEX(3.4, 1));
        S.Add(new VERTEX(2.55, 1.1));
        S.Add(new VERTEX(2.55, 1.35));
        S.Add(new VERTEX(1.85, 1.35));
        S.Add(new VERTEX(1.3, 1.55));
        S.Add(new VERTEX(-2.27, 1.55));
        S.Add(new VERTEX(-2.3, 1.05));
        S.Add(new VERTEX(-3.8, 0.45));
        S.Add(new VERTEX(-7.7, 0.45));
        S.Add(new VERTEX(-7.7, -0.45));
        S.Add(new VERTEX(-3.8, -0.45));
        S.Add(new VERTEX(-2.3, -1.05));
        S.Add(new VERTEX(-2.27, -1.55));
        S.Add(new VERTEX(1.3, -1.55));
        S.Add(new VERTEX(1.85, -1.35));
        S.Add(new VERTEX(2.55, -1.35));
        S.Add(new VERTEX(2.55, -1.1));
        S.Add(new VERTEX(3.4, -1));
        S.Add(new VERTEX(4.314, -0.579));
        S.Add(new VERTEX(4.375, -0.4));
        S.Add(new VERTEX(4.65, -0.4));
        S.Add(new VERTEX(4.75, -0.275));
        S.Add(new VERTEX(5.3, -0.275));

        for (int i = 0; i < S.Count; i++)
        {
            Instantiate(dot, new Vector3((float)S[i].X, (float)S[i].Y, -1.0f), Quaternion.identity);
        }
        #endregion

        s_Angle = 360.0f - Obstacle.transform.eulerAngles.x; // ��ֹ� ������� X ���� : 315
        o_Angle = 360.0f - F35B_Object.transform.eulerAngles.x; // ��ǥ ������� X ���� : 45
        s_location = new VERTEX(Obstacle.transform.position.x, Obstacle.transform.position.y); // (-25, -5)

        NFPResult = MakeNFP.GetSTOVL(s_Angle, o_Angle, s_location);
        //Design();
    }

    void Design()
    {
        if (NFPResult != null)
        {
            for (int i = 0; i < NFPResult.Count; i++)
            {
                Instantiate(dot, new Vector3((float)NFPResult[i].Item1.X, (float)NFPResult[i].Item1.Y), Quaternion.identity);
                // Item1 = ������, Item2 = ����
            }
        }
    }

    void Update() // ������ �޶��������� ����Ǵ� �Լ� -> ��� �ּ� ó����
    {
        //s_Angle = Obstacle.transform.eulerAngles.x; // ��ֹ� ������� X ���� : 315
        //o_Angle = F35B_Object.transform.eulerAngles.x; // ��ǥ ������� X ���� : 45
        //s_location = new VERTEX(Obstacle.transform.position.x, Obstacle.transform.position.y); // (-25, -5)

        //NFPResult = MakeNFP.Get(s_Angle, o_Angle, s_location);
        //Design();

        //Debug.Log(NFPResult);
    }
}
