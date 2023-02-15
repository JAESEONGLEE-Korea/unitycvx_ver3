using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NFP;
using System;

public class AircraftNFP : MonoBehaviour
{
    public GameObject F35B_Object; // 움직일 목표 함재기
    public GameObject Obstacle; // 장애물 함재기
    public GameObject dot; // Polygon 그리려고

    double s_Angle; // 장애물 함재기 x 각도
    double o_Angle; // 목표 함재기 x 각도
    VERTEX s_location; // 장애물 함재기 위치 좌표

    List<Tuple<VERTEX, VERTEX>> NFPResult; // 좌표로 나온 NFP 형태?
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

        s_Angle = 360.0f - Obstacle.transform.eulerAngles.x; // 장애물 함재기의 X 각도 : 315
        o_Angle = 360.0f - F35B_Object.transform.eulerAngles.x; // 목표 함재기의 X 각도 : 45
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
                // Item1 = 시작점, Item2 = 끝점
            }
        }
    }

    void Update() // 각도가 달라질때마다 실행되는 함수 -> 모두 주석 처리함
    {
        //s_Angle = Obstacle.transform.eulerAngles.x; // 장애물 함재기의 X 각도 : 315
        //o_Angle = F35B_Object.transform.eulerAngles.x; // 목표 함재기의 X 각도 : 45
        //s_location = new VERTEX(Obstacle.transform.position.x, Obstacle.transform.position.y); // (-25, -5)

        //NFPResult = MakeNFP.Get(s_Angle, o_Angle, s_location);
        //Design();

        //Debug.Log(NFPResult);
    }
}
