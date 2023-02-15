using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NFP;
using System;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class Real : MonoBehaviour // Real Ŭ���� : ��ǥ ����� �̵��ϸ�, ������ NFP ����ȭ
{
    public TextAsset txt; // ��� ��ǥ ������ ����ִ� �ؽ�Ʈ

    #region ����� ��ġ �̵�(�ؽ�Ʈ �о����)
    string[,] Sentence;
    int lineSize, rowSize;
    int k = 0;
    #endregion

    public GameObject Object_F35B; // ������ ��ǥ �����
    public GameObject Obstacle_F35B; // ��ֹ� �����
    public GameObject Obstacle_HELO; // ��ֹ� �����

    public GameObject Image; // "�浹"�̶�� �̹��� �����ַ���

    // ������ NFP �׸�����
    public GameObject DrawdotS; 
    public GameObject DrawdotH;

    double o_Angle; // ��ǥ ����� x ����

    // �ʵ�� �ؾ��ϳ� ���� ������ �ؾ��ϳ�
    //VERTEX STOVL_Obslocation; // (STOVL)��ֹ� ����� ��ġ ��ǥ
    //VERTEX HELO_Obslocation; // (HELO)��ֹ� ����� ��ġ ��ǥ

    // List<Tuple<VERTEX, VERTEX>> => item1 : ������, item2 : ����
    List<Tuple<VERTEX, VERTEX>> STOVL_NFPResult;
    List<Tuple<VERTEX, VERTEX>> STOVL_PreNFPResult;

    List<Tuple<VERTEX, VERTEX>> HELO_NFPResult; 
    List<Tuple<VERTEX, VERTEX>> HELO_PreNFPResult;

    // Line �������� ���� ����
    LineRenderer lr_s;
    LineRenderer lr_h;

    public TextMeshProUGUI OrbitalXAngle; // ��ǥ ����� �̵��ϸ� �ٲ�� ���� �����ַ���

    int index = 0;

    void Start()
    {
        #region ����� �̵� ����, �ؽ�Ʈ ���� �б�
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
        #region �ؽ�Ʈ ���� �а� ����� �̵�
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

    void MakeText() // ����� �̵��ϸ� ������ �����ִ� �Լ�
    {
        OrbitalXAngle.text = "[STOVL's Angle]:" + Object_F35B.transform.eulerAngles.x.ToString();//(360.0f - Object_F35B.transform.eulerAngles.x).ToString();
    }

    float AngleAdjustFunc(float myAngle) // �Է� ���ڷ� ���� ������ �޶� �����ִ� �Լ�
    {
        #region ���� ����! (������ ���� = 360 - �� ����)
        // �μ����� ���� = ��ȣ���� ����
        // ���� 45�� -> �������� 315��
        // ���� 135�� -> �������� 225��(������ ���� = 360 - �� ����)
        #endregion

        float Result_Angle; 
        return Result_Angle = 360.0f - myAngle;
    }

    void NFPResultFunc() // NFP ���� �Լ�
    {
        // o(Obital) : �̵��Ϸ���, ��ǥ ����� / s(Statinary) : �������ִ�, ��ֹ� �����[Obstacle]
        o_Angle = AngleAdjustFunc(Object_F35B.transform.eulerAngles.x); // ��ǥ ������� X ���� : 45(�� ����)

        // ��ֹ� ����� x ����
        double STOVL_ObsAngle = AngleAdjustFunc(Obstacle_F35B.transform.eulerAngles.x); // (STOVL)��ֹ� ������� X ���� : 315(�� ����)
        double HELO_ObsAngle = 0.0f; // (HELO)��ֹ� ����� x ����

        // ��ֹ� ����� ��ġ ��ǥ
        VERTEX STOVL_Obslocation = new VERTEX(Obstacle_F35B.transform.position.x, Obstacle_F35B.transform.position.y); // (-25, -5)
        VERTEX HELO_Obslocation = new VERTEX(Obstacle_HELO.transform.position.x, Obstacle_HELO.transform.position.y); // (-60, -5)

        // MakeNFP�� �ʿ� �Լ� ���� : ��ֹ� ����� ����, ��ǥ ����� ����, ��ֹ� ����� ��ġ
        STOVL_NFPResult = MakeNFP.GetSTOVL(STOVL_ObsAngle, o_Angle, STOVL_Obslocation);
        HELO_NFPResult = MakeNFP.GetHELO(HELO_ObsAngle, o_Angle, HELO_Obslocation);

        DeleteNFPDesign();
        NFPDesign();
    }

    void DeleteNFPDesign() // ���� NFP �׸� ���� �Լ�
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

    void NFPDesign() // ���� NFP �׸� ����ȭ
    {
        if (STOVL_NFPResult != null) // STOVL ����� �������� ������ NFP
        {
            for (int i = 0; i < STOVL_NFPResult.Count-1; i++)
            {
                var ob = Instantiate(DrawdotS, new Vector3((float)STOVL_NFPResult[i].Item1.X, (float)STOVL_NFPResult[i].Item1.Y, -1.0f), Quaternion.identity); // Item1 = ������, Item2 = ����
                ob.AddComponent<LineRenderer>();

                lr_s = ob.GetComponent<LineRenderer>();

                lr_s.widthMultiplier = 0.1f; //�� �ʺ�
                lr_s.startColor = Color.yellow; //�� ������ ��
                lr_s.endColor = Color.yellow; //�� ���� ��

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
                var ob = Instantiate(DrawdotH, new Vector3((float)HELO_NFPResult[i].Item1.X, (float)HELO_NFPResult[i].Item1.Y, -1.0f), Quaternion.identity); // Item1 = ������, Item2 = ����

                ob.AddComponent<LineRenderer>();

                lr_h = ob.GetComponent<LineRenderer>();

                lr_h.widthMultiplier = 0.1f; //�� �ʺ�
                lr_h.startColor = Color.yellow; //�� ������ ��
                lr_h.endColor = Color.yellow; //�� ���� ��

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
        #region ������
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

    bool IsInside(VERTEX B, List<Tuple<VERTEX, VERTEX>> tuples) // ��ǥ ������� �������� NFP ������ ������ True
    {
        int crosses = 0; //crosses�� ��q�� ������ �������� �ٰ������� ������ ����
        for (int i = 0 ; i < tuples.Count() ; i++)
        {        
            int j = (i+1)%tuples.Count();
            if((tuples[i].Item1.Y > B.Y) != (tuples[j].Item1.Y > B.Y)) //�� B�� ���� (p[i], p[j])�� y��ǥ ���̿� ����
            {  
                //atX�� �� B�� ������ ���򼱰� ���� (p[i], p[j])�� ����   
                double atX = (tuples[j].Item1.X- tuples[i].Item1.X)*(B.Y-tuples[i].Item1.Y)/(tuples[j].Item1.Y-tuples[i].Item1.Y)+tuples[i].Item1.X;
                //atX�� ������ ���������� ������ ������ ������ ������ ������Ų��.    
                if (B.X < atX) crosses++;
                else if (B.X == atX) return false;  //B�� ������ Y���־ȿ� �����鼭 ����� �������� ������ �������� x���� B.x�� ���ٸ� b�� �������� ���̴�. -> �ȿ� �ִ� ���� �ƴ�
            }
        }
        return crosses % 2 > 0; // true : ������ polygon �ȿ� ����
    }
}