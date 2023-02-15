using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NFP;
using System;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class Real : MonoBehaviour // Real 클래스 : 목표 함재기 이동하며, 생성된 NFP 가시화
{
    public TextAsset txt; // 경로 좌표 값들이 들어있는 텍스트

    #region 함재기 위치 이동(텍스트 읽어오기)
    string[,] Sentence;
    int lineSize, rowSize;
    int k = 0;
    #endregion

    public GameObject Object_F35B; // 움직일 목표 함재기
    public GameObject Obstacle_F35B; // 장애물 함재기
    public GameObject Obstacle_HELO; // 장애물 함재기

    public GameObject Image; // "충돌"이라는 이미지 보여주려고

    // 생성된 NFP 그리려고
    public GameObject DrawdotS; 
    public GameObject DrawdotH;

    double o_Angle; // 목표 함재기 x 각도

    // 필드로 해야하나 지역 변수로 해야하나
    //VERTEX STOVL_Obslocation; // (STOVL)장애물 함재기 위치 좌표
    //VERTEX HELO_Obslocation; // (HELO)장애물 함재기 위치 좌표

    // List<Tuple<VERTEX, VERTEX>> => item1 : 시작점, item2 : 끝점
    List<Tuple<VERTEX, VERTEX>> STOVL_NFPResult;
    List<Tuple<VERTEX, VERTEX>> STOVL_PreNFPResult;

    List<Tuple<VERTEX, VERTEX>> HELO_NFPResult; 
    List<Tuple<VERTEX, VERTEX>> HELO_PreNFPResult;

    // Line 렌더링을 위한 변수
    LineRenderer lr_s;
    LineRenderer lr_h;

    public TextMeshProUGUI OrbitalXAngle; // 목표 함재기 이동하며 바뀌는 각도 보여주려고

    int index = 0;

    void Start()
    {
        #region 함재기 이동 관련, 텍스트 파일 읽기
        string currentText = txt.text.Substring(0, txt.text.Length - 1);
        string[] line = currentText.Split('\n');
        lineSize = line.Length;
        rowSize = line[0].Split('\t').Length;
        Sentence = new string[lineSize, rowSize];
        for (int i = 0; i < lineSize; i++)
        {
            string[] row = line[i].Split('\t');
            for (int j = 0; j < rowSize; j++) Sentence[i, j] = row[j];
        }
        #endregion
    }

    void FixedUpdate() //Update()
    {
        #region 텍스트 파일 읽고 함재기 이동
        float tractor_x = float.Parse(Sentence[k, 0]);
        float tractor_y = float.Parse(Sentence[k, 1]);

        float w_deg = float.Parse(Sentence[k, 2]);

        Object_F35B.transform.position = new Vector3(tractor_x, tractor_y, -1.9f);
        Object_F35B.transform.rotation = Quaternion.Euler(w_deg, 90, -90);
        k = k + 1;
        #endregion

        NFPResultFunc();
        InOutCheck();
        MakeText();
    }

    void MakeText() // 함재기 이동하며 각도를 보여주는 함수
    {
        OrbitalXAngle.text = "[STOVL's Angle]:" + Object_F35B.transform.eulerAngles.x.ToString();//(360.0f - Object_F35B.transform.eulerAngles.x).ToString();
    }

    float AngleAdjustFunc(float myAngle) // 입력 인자로 들어가는 각도가 달라서 맞춰주는 함수
    {
        #region 각도 정리! (오빠들 각도 = 360 - 내 각도)
        // 민수오빠 각도 = 진호오빠 각도
        // 내가 45도 -> 오빠들은 315도
        // 내가 135도 -> 오빠들은 225도(오빠들 각도 = 360 - 내 각도)
        #endregion

        float Result_Angle; 
        return Result_Angle = 360.0f - myAngle;
    }

    void NFPResultFunc() // NFP 생성 함수
    {
        // o(Obital) : 이동하려는, 목표 함재기 / s(Statinary) : 정지해있는, 장애물 함재기[Obstacle]
        o_Angle = AngleAdjustFunc(Object_F35B.transform.eulerAngles.x); // 목표 함재기의 X 각도 : 45(내 기준)

        // 장애물 함재기 x 각도
        double STOVL_ObsAngle = AngleAdjustFunc(Obstacle_F35B.transform.eulerAngles.x); // (STOVL)장애물 함재기의 X 각도 : 315(내 기준)
        double HELO_ObsAngle = 0.0f; // (HELO)장애물 함재기 x 각도

        // 장애물 함재기 위치 좌표
        VERTEX STOVL_Obslocation = new VERTEX(Obstacle_F35B.transform.position.x, Obstacle_F35B.transform.position.y); // (-25, -5)
        VERTEX HELO_Obslocation = new VERTEX(Obstacle_HELO.transform.position.x, Obstacle_HELO.transform.position.y); // (-60, -5)

        // MakeNFP의 필요 함수 인자 : 장애물 함재기 각도, 목표 함재기 각도, 장애물 함재기 위치
        STOVL_NFPResult = MakeNFP.GetSTOVL(STOVL_ObsAngle, o_Angle, STOVL_Obslocation);
        HELO_NFPResult = MakeNFP.GetHELO(HELO_ObsAngle, o_Angle, HELO_Obslocation);

        DeleteNFPDesign();
        NFPDesign();
    }

    void DeleteNFPDesign() // 이전 NFP 그림 삭제 함수
    {
        GameObject.Find("DrawingSphereS").tag = "Untagged";
        if (index > 0)
        {
            for (int i = 0; i < GameObject.FindGameObjectsWithTag("DrawingSphereS").Length; i++)
            {
                if (GameObject.Find("DrawingSphereS(Clone)") != null)
                {
                    Destroy(GameObject.FindGameObjectsWithTag("DrawingSphereS")[i]);
                }
            }
        }
        GameObject.Find("DrawingSphereS").tag = "DrawingSphereS";

        GameObject.Find("DrawingSphereH").tag = "Untagged";
        if (index > 0)
        {
            for (int i = 0; i < GameObject.FindGameObjectsWithTag("DrawingSphereH").Length; i++)
            {
                if (GameObject.Find("DrawingSphereH(Clone)") != null)
                {
                    Destroy(GameObject.FindGameObjectsWithTag("DrawingSphereH")[i]);
                }
            }
        }
        GameObject.Find("DrawingSphereH").tag = "DrawingSphereH";
    }

    void NFPDesign() // 현재 NFP 그림 가시화
    {
        if (STOVL_NFPResult != null) // STOVL 함재기 기준으로 생성된 NFP
        {
            for (int i = 0; i < STOVL_NFPResult.Count-1; i++)
            {
                var ob = Instantiate(DrawdotS, new Vector3((float)STOVL_NFPResult[i].Item1.X, (float)STOVL_NFPResult[i].Item1.Y, -1.0f), Quaternion.identity); // Item1 = 시작점, Item2 = 끝점
                ob.AddComponent<LineRenderer>();

                lr_s = ob.GetComponent<LineRenderer>();

                lr_s.widthMultiplier = 0.1f; //선 너비
                lr_s.startColor = Color.yellow; //선 시작점 색
                lr_s.endColor = Color.yellow; //선 끝점 색

                var cube1Pos = new Vector3((float)STOVL_NFPResult[i].Item1.X, (float)STOVL_NFPResult[i].Item1.Y, -1.0f);
                var cube2Pos = new Vector3((float)STOVL_NFPResult[i].Item2.X, (float)STOVL_NFPResult[i].Item2.Y, -1.0f);
                lr_s.SetPosition(0, cube1Pos);
                lr_s.SetPosition(1, cube2Pos);
            }
        }

        if (HELO_NFPResult != null)
        {
            for (int i = 0; i < HELO_NFPResult.Count-1; i++)
            {
                var ob = Instantiate(DrawdotH, new Vector3((float)HELO_NFPResult[i].Item1.X, (float)HELO_NFPResult[i].Item1.Y, -1.0f), Quaternion.identity); // Item1 = 시작점, Item2 = 끝점

                ob.AddComponent<LineRenderer>();

                lr_h = ob.GetComponent<LineRenderer>();

                lr_h.widthMultiplier = 0.1f; //선 너비
                lr_h.startColor = Color.yellow; //선 시작점 색
                lr_h.endColor = Color.yellow; //선 끝점 색

                var cube1Pos = new Vector3((float)HELO_NFPResult[i].Item1.X, (float)HELO_NFPResult[i].Item1.Y, -1.0f);
                var cube2Pos = new Vector3((float)HELO_NFPResult[i].Item2.X, (float)HELO_NFPResult[i].Item2.Y, -1.0f);
                lr_h.SetPosition(0, cube1Pos);
                lr_h.SetPosition(1, cube2Pos);
            }
        }
        index++;
    }

    void InOutCheck()
    {
        Image.SetActive(false);
        bool check;
        float OrbitalposX = Object_F35B.transform.position.x + 8.0f;
        float OrbitalposY = Object_F35B.transform.position.y;

        check = IsInside(new VERTEX(OrbitalposX, OrbitalposY), STOVL_NFPResult);

        if (check)
        {
            Image.SetActive(true);
        }
        #region 저리가
        //bool Cheking1;
        //bool Cheking2;
        //if (Object_F35B.transform.position.x == -19.697f)
        //{
        //    float OrbitalposX = Object_F35B.transform.position.x + 8.0f;
        //    float OrbitalposY = Object_F35B.transform.position.y;
        //    Cheking1 = IsInside(new VERTEX(OrbitalposX, OrbitalposY), STOVL_NFPResult);

        //    if (Cheking1)
        //    {
        //        Image.SetActive(true);
        //    }
        //}

        //else if (Object_F35B.transform.position.x == -9.197f)
        //{
        //    Cheking2 = IsInside(new VERTEX(-1.197f, Object_F35B.transform.position.y), STOVL_NFPResult);
        //    Image.SetActive(false);
        //}
        #endregion
    }

    bool IsInside(VERTEX B, List<Tuple<VERTEX, VERTEX>> tuples) // 목표 함재기의 기준점이 NFP 안으로 들어오면 True
    {
        int crosses = 0; //crosses는 점q와 오른쪽 반직선과 다각형과의 교점의 개수
        for (int i = 0 ; i < tuples.Count() ; i++)
        {        
            int j = (i+1)%tuples.Count();
            if((tuples[i].Item1.Y > B.Y) != (tuples[j].Item1.Y > B.Y)) //점 B가 선분 (p[i], p[j])의 y좌표 사이에 있음
            {  
                //atX는 점 B를 지나는 수평선과 선분 (p[i], p[j])의 교점   
                double atX = (tuples[j].Item1.X- tuples[i].Item1.X)*(B.Y-tuples[i].Item1.Y)/(tuples[j].Item1.Y-tuples[i].Item1.Y)+tuples[i].Item1.X;
                //atX가 오른쪽 반직선과의 교점이 맞으면 교점의 개수를 증가시킨다.    
                if (B.X < atX) crosses++;
                else if (B.X == atX) return false;  //B가 직선의 Y범주안에 있으면서 우방향 반직선과 직선의 교차점의 x값과 B.x가 같다면 b는 직선위의 점이다. -> 안에 있는 점이 아님
            }
        }
        return crosses % 2 > 0; // true : 정점이 polygon 안에 있음
    }
}