using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using System.Globalization;
using System.Net.Sockets;
using System.Net;
using Pathgeneration_Aircraft_V1; // ����ȣ ��� ����(Taxiing)
using Pathfollowing_Aircraft_V1; // ����ȣ ��� ����(Taxiing)
using Pathfollowing_Towbar_V5;
using Pathgeneration_Towbar_V2;
//using DG.Tweening; // DOTween ���� ���(�ٿ�) -> ProcessTime �ȿ� �����̴� �� �����ַ��� F35BObject.transform.DOMove(new Vector3(-300.0f, -9.5f, -60.4f), ProcessTime);

public class FlightDeck_Inte : MonoBehaviour // FlightDeck_Inte : ���� ��忡���� ���఩�� ���� ���� Ŭ����
{
    Communication DeckInformationNSocket;
    ArrayList DeckInformations; // DeckInformations -> ���఩�� ���� ����

    List<GameObject> SetUpAircraft = new List<GameObject>(); // ����ڷκ��� ���õ� ��¥ ������ 

    #region ���� ���� ����
    public Socket DecksocketConnection;
    List<int> Ports;
    #endregion

    #region ����� �̵� ���� ���� ����
    Vector2 TakeOffSpot; // �̷� ���
    Vector2 LandingSpot; // ���� ���

    List<string> TemporalAAreaList = new List<string>() { "A2", "A3", "A4", "A5" }; // ��ȣ dll������ �ӽ÷� ������ ����

    List<Vector2> ServiceA; // A ���� ����(A1,2,3,4,5)
    List<Vector2> ServiceB; // B ���� ����(B1,2,3,4,5)
    #endregion

    public GameObject Tractor; //** Ʈ����
    public GameObject Towbar; //** ����

    void Start()
    {
        GameObject Information_Object = GameObject.Find("InfoObject").gameObject; // ����Ƽ ���̾��Ű â�� "InfoObject"��� �̸��� GameObject�� ã�� ����
        DeckInformationNSocket = Information_Object.GetComponent<Communication>();

        DeckInformations = DeckInformationNSocket.MakeInfomations(); // ����ڰ� �Է��� ��� ������ ���� �ִ� ArrayList

        Ports = (List<int>)DeckInformations[0]; // ���� ��Ʈ ��ȣ(Ports[1]) ����Ϸ���
        DecksocketConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        #region ����� �̵� ���� ���� �Ҵ�
        TakeOffSpot = new Vector2(-80.0f, -9.5f); // ����� ���� : (180.0f, 90.0f, -90.0f)
        LandingSpot = new Vector2(-41.5f, -9.5f); // ����� ���� : (180.0f, 90.0f, -90.0f)

        ServiceA = new List<Vector2>()
        { new Vector2(-80.0f, 9.0f), new Vector2(-63.0f, 9.0f), new Vector2(-40.5f, 7.5f), new Vector2(-22.0f, 9.0f), new Vector2(-5.0f, 9.0f) }; // A1,2,3,4,5 ������

        ServiceB = new List<Vector2>()
        { new Vector2(-250.0f, 9.0f), new Vector2(-233.0f, 9.0f), new Vector2(-216.0f, 9.0f), new Vector2(-199.0f, 9.0f), new Vector2(-182.0f, 9.0f) }; // B1,2,3,4,5 ������
        #endregion

        SetAircrafts();

        Tractor.SetActive(false);
        Towbar.SetActive(false);

        //SetResources(); // ���఩�� �� �ڿ� ���� �Լ�, �ش�Ǵ� ��ġ? �ƴϸ� ������ŭ �ڿ� �α�
    }

    public void SetAircrafts() // �ش�Ǵ� ���� ������ ����� �̸�, ����� �ڼ� ����
    {
        // 1-1. object DeckObject = Informations[1]; // List<int>�� ��Ҵ� int ���¸� ��� �ֵ�, ArrayList�� ��Ҵ� object ���¸� ��� �ִ�.

        // �� 1-2. (float or int or List<ArrayList> ��..)�ٸ� ���� => �̰��� ĳ��Ʈ? ĳ�����̶�� �Ѵ�! ��� �̰� ���� �˴ٴ� 
        // ���� ������ ��ټ��� ������ ��� ���� �ִ� ���°� X -> ���� �Ҹ��ĸ� ����ڰ� �Է��� �ؾ� ���� ����!!
        // �׷��� Informations �������� �ڽ��� ���¸� ��!! => Casting�� �������
        List<ArrayList> DeckAircrafts = (List<ArrayList>)DeckInformations[1]; //�갡 ����

        for (int i = 0; i < DeckAircrafts.Count; i++)
        {
            ArrayList DeckAircraftIndex = DeckAircrafts[i]; // DeckAircraftIndex : ���� ����[0], ����� �̸�[1], ����� ����[2] ������ ���� �ִ� ArrayList.

            SetUpAircraft.Add(GameObject.Find("GameObject").transform.Find((string)DeckAircraftIndex[0]).gameObject); // ���õ� ������ gameObject ���·� List�� �־���
            GameObject.Find("GameObject").transform.Find((string)DeckAircraftIndex[0]).gameObject.SetActive(true); // ���õ� ������ ���� ���̰� ����
            #region ����� ����, ���� ��ǥ�� Ȯ�� ��� -> ���� ��ǥ�� = ���� ��ǥ��
            //Debug.Log(String.Format("{0} ����� {1}�� ��ġ(���� ��ǥ�� {2})", SetUpAircraft[i].name, SetUpAircraft[i].transform.position, SetUpAircraft[i].transform.localPosition));
            #endregion

            SetUpAircraft[i].name = (string)DeckAircraftIndex[1]; // ����� �̸�
            if ((float)DeckAircraftIndex[2] == 315.0f) // == -45.0f
            {
                //SetUpAircraft[i].transform.rotation = Quaternion.Euler(135.0f, 90.0f, -90.0f); // ��õ ��� 2
                SetUpAircraft[i].transform.eulerAngles = new Vector3(135.0f, 90.0f, -90.0f); // ��õ ��� 1 : ����� ����(���̾��Ű â������)
                //Debug.Log(SetUpAircraft[i].transform.eulerAngles); // (45.0, 270.0, 90.0)
            }

            else // == 45.0f
            {
                SetUpAircraft[i].transform.eulerAngles = new Vector3((float)DeckAircraftIndex[2], 90.0f, -90.0f); // ��õ ��� 1 : ����� ����(���̾��Ű â������)
                //Debug.Log(SetUpAircraft[i].transform.eulerAngles); // (45.0, 90.0, 270.0)
            }
        }
        Invoke("MakeSocket", 0.5f); // 220901 11:24 ��� �ּ�ó��
        //MakeSocket();
    }

    #region SetResources() : ���఩�� �� �ʿ� �ڿ��� / ���߿� ��üȭ
    //public void SetResources() // ���఩�� �� �ڿ� ���� �Լ�, �ش�Ǵ� ��ġ? �ƴϸ� ������ŭ �ڿ� �α�
    //{
    //for (int y = 0; y < height; ++y)
    //{
    //    for (int x = 0; x < width; ++x)
    //    {
    //        Instantiate(block, new Vector3(x, y, 0), Quaternion.identity);
    //    }
    //}
    //}
    #endregion

    void MakeSocket() // ���� ������ �ѹ��̸� �ȴ�! (�׷��ϱ� Start()�� �ᵵ �ɵ�?)
    {
        try
        {
            DecksocketConnection.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Ports[1])); // ���� ���� ��!
            DecksocketConnection.Blocking = false; // ��-���ŷ ����
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e); // Server�� ������� ������ ������ ����
        }

        InvokeRepeating("ReceivedFromPython", 0.0f, 0.05f); //  DecksocketConnection.Blocking = false; ���� "���� ����" �߻� X
    }

    void ReceivedFromPython() // Python���κ��� ������ �޴� �Լ�
    {
        //Debug.Log(DecksocketConnection.Connected); // All True -> ���� ���� ��
        var ReceiveData = new byte[4];
        DecksocketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None); // Updata()��, InvokeRepeating()�̳� ���⼭ ����
        Array.Reverse(ReceiveData);
        ReceiveData = new byte[BitConverter.ToInt32(ReceiveData, 0)];
        DecksocketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);

        string ReceivedDataValue = Encoding.UTF8.GetString(ReceiveData);
        Debug.Log("From Python : " + ReceivedDataValue);
        //Debug.Log(string.Format("{0}��, From Python : {1}", Time.time, ReceivedDataValue));
        DataProcessing(ReceivedDataValue);
    }

    void DataProcessing(string fromSimPy) // SimPy���� �Ѿ�� ����(����� �̸�[0], �۾�[1], �ҿ� �ð�[2]) ó�� �Լ�
    {
        string[] AircraftName_Process_Time = fromSimPy.Split(',');

        #region ��ȣ �Լ� Call
        if (AircraftName_Process_Time[2] == "0.0")
        {
            if (AircraftName_Process_Time[1] == "ToTakeOff")
            {
                ToTakeOff(AircraftName_Process_Time[0]);
            }

            else if (AircraftName_Process_Time[1] == "ToAarea")
            {
                ToAarea(AircraftName_Process_Time[0]);
            }
        }
        #endregion

        #region ���̽㿡��, �۾� �ҿ� �ð� ���� -> ����ȭ ���� (220830 Coroutine�� ����ؾ� ��)
        else
        {
            float ProcessTime = float.Parse(AircraftName_Process_Time[2]); // 220901 ��� �� ��..?
            GameObject F35BObject = GameObject.Find(AircraftName_Process_Time[0]);
            #region ���ӿ�����Ʈ�� setactive(false) -> setactive(true)�ϴ� �������� ���ӿ�����Ʈ�� ��ã�´ٸ�
            if (F35BObject == null) // ���� ã�°� ���� �Լ����� ��������
            {
                F35BObject = GameObject.Find("GameObject").transform.Find(AircraftName_Process_Time[0]).gameObject;
            }
            #endregion

            if (AircraftName_Process_Time[1] == "TakeOff") // ����� �̷�
            {
                StartCoroutine(forTakeOff());
                IEnumerator forTakeOff()
                {
                    for (int i = 0; i < 110; i++) // (-80.0f, -9.5f, -30.2f)[�̷� ���]���� (-300.0f, -9.5f, -60.4f)[���1]��
                    {
                        F35BObject.transform.position += new Vector3(-2.0f, 0.0f); // ����Ⱑ ���� �����ϴϱ�(z����) -30.2f -> -60.4f
                        F35BObject.transform.position += new Vector3(0.0f, 0.0f, -0.27454545f); // ����Ⱑ ���� �����ϴϱ�(z����) -30.2f -> -60.4f

                        yield return new WaitForSeconds(0.005f);
                    }
                    ProcessFinishSignalToPython();
                }
            }

            else if (AircraftName_Process_Time[1] == "Mission") // ����� �ӹ� ����
            {
                F35BObject.SetActive(false);

                StartCoroutine(forMission());
                IEnumerator forMission()
                {
                    for (int i = 0; i < 10; i++) // (40.0f, -9.5f, -60.4f)[���2]
                    {
                        F35BObject.transform.position = new Vector3(40.0f, -9.5f, -60.4f); // ���� ��, ��������� ��� ���
                        yield return new WaitForSeconds(0.025f);
                        ////yield return new WaitForSeconds(0.05f);
                    }
                    ProcessFinishSignalToPython();
                }
            }

            //220902 �ؾ��� �� : �̷� �غ� �� �̷� ���� ����Ⱑ ������ ���� �ȵǰ� �ϱ�(SimPy���� �ؾ����� �ʳ�..?)
            else if (AircraftName_Process_Time[1] == "Land") // ����� ����
            {
                F35BObject.SetActive(true);

                StartCoroutine(forLand());
                IEnumerator forLand()
                {
                    for (int i = 0; i < 10; i++) // (40.0f, -9.5f, -60.4f)[���2]���� (-41.5f, -9.5f, -30.2f)[���� ���]��
                    {
                        F35BObject.transform.position += new Vector3(-8.15f, 0.0f);
                        F35BObject.transform.position += new Vector3(0.0f, 0.0f, 3.02f);

                        yield return new WaitForSeconds(0.05f); // ���ڰ� �۴ٴ°� �׸�ŭ ���� for���� ���ٴ� �� => �������� �ڿ�������
                        ////yield return new WaitForSeconds(0.025f); // ���ڰ� �۴ٴ°� �׸�ŭ ���� for���� ���ٴ� �� => �������� �ڿ�������
                    }
                    ProcessFinishSignalToPython();
                }
            }

            // ����� ����
            // ����� ����
        }
        #endregion
    }

    public float InputXAngle(float YAngle) // ��ȣ �Լ� �Է� �� ���� ���� �Լ�
    {
        float ChangedAngle = 180.0f;

        if (YAngle == 90.0f) // ����� �Է°� : 45�� -> ������ 45��
        {
            ChangedAngle = 315.0f; // ��ȣ�������� 315��
        }

        else if (YAngle == 270.0f) // ����� �Է°� : -45�� -> ������ 135��
        {
            ChangedAngle = 225.0f; // ��ȣ�������� 225��
        }

        return ChangedAngle;
    }

    #region �̵� ���� �Լ���(��ȣ ���� �߰�)
    public void ToTakeOff(string AircraftName) // Taxiing
    {
        // ��ȣ dll -> ��! ���� ������ ����
        Pathgeneration_Aircraft_V1.pathgeneration MakePath = new Pathgeneration_Aircraft_V1.pathgeneration();
        Pathfollowing_Aircraft_V1.pathfollowing FollowPath = new Pathfollowing_Aircraft_V1.pathfollowing();

        double TransferTime;
        GameObject F35BObject = GameObject.Find(AircraftName);

        var WayPoints = MakePath.path(F35BObject.transform.position, new Vector3(TakeOffSpot.x, TakeOffSpot.y, F35BObject.transform.position.z), InputXAngle(F35BObject.transform.eulerAngles.y), InputXAngle(180.0f));
        var FollowValue = FollowPath.path_following(WayPoints); // FollowValue : 4������ �̷���� ���� => ����� x,y, x ���� ����, �ð�(������ ���� �� �ҿ� �ð�)

        StartCoroutine(MoveToTakeOff());
        IEnumerator MoveToTakeOff()
        {
            for (int i = 0; i < FollowValue.GetLength(1); i++) // FollowValue : [4, ????] ->  FollowValue.GetLength(0) = 4 / FollowValue.GetLength(1) = ????
            {
                F35BObject.transform.position = new Vector3((float)FollowValue[0, i], (float)FollowValue[1, i], F35BObject.transform.position.z);
                F35BObject.transform.rotation = Quaternion.Euler(-(float)FollowValue[2, i] * 180.0f / Mathf.PI, 90.0f, -90.0f);

                yield return new WaitForSeconds(0.001f);
            }
            TransferTime = FollowValue[3, FollowValue.GetLength(1) - 1];
            Debug.Log(string.Format("�̵� �ҿ� �ð� : {0}", TransferTime));

            TransferTimeResultToPython(TransferTime);
        }
    }

    // ServiceA ������ 5���ε� ���� ��������(5���� ���� �� ��� ����ִ���) ���̽� ���� ����Ƽ���� ����? -> ����ȭ�� ���⿡ �����ؾ�����������
    public void ToAarea(string AircraftName) // TowbarTractor
    {
        // ��ȣ dll -> ��! ���� ������ ����
        Pathgeneration_Towbar_V2.pathgeneration MakePath = new Pathgeneration_Towbar_V2.pathgeneration();
        Pathfollowing_Towbar_V5.pathfollowing FollowPath = new Pathfollowing_Towbar_V5.pathfollowing();

        double TransferTime;
        GameObject F35BObject = GameObject.Find(AircraftName);

        //Debug.Log(string.Format("�ߵȴ� ���� : {0}", F35BObject.transform.eulerAngles));

        System.Random random = new System.Random();
        int ChooseAreaIndex = random.Next(TemporalAAreaList.Count);
        string ChooseArea = TemporalAAreaList[ChooseAreaIndex];
        TemporalAAreaList.Remove(ChooseArea);

        var WayPoints = MakePath.path(F35BObject.transform.position, F35BObject.transform.position, 180.0f, 225.0f);
        var FollowValue = FollowPath.path_following(WayPoints); // FollowValue : 10������ �̷���� ���� => Ʈ������ x,y,angle / Towbar�� x,y,angle / ������� x,y,angle / �̵� �ҿ� �ð�

        StartCoroutine(MoveToAarea());
        IEnumerator MoveToAarea()
        {
            for (int i = 0; i < FollowValue.Count; i++)
            {
                Tractor.SetActive(true);
                Towbar.SetActive(true);

                Tractor.transform.position = new Vector3((float)FollowValue[i][0], (float)FollowValue[i][1], Tractor.transform.position.z);
                Tractor.transform.rotation = Quaternion.Euler(-(float)FollowValue[i][2] * 180.0f / Mathf.PI - 90.0f, 90.0f, -90.0f);

                Towbar.transform.position = new Vector3((float)FollowValue[i][3], (float)FollowValue[i][4], Tractor.transform.position.z);
                Towbar.transform.rotation = Quaternion.Euler(-(float)FollowValue[i][5] * 180.0f / Mathf.PI - 90.0f, 90.0f, -90.0f);

                F35BObject.transform.position = new Vector3((float)FollowValue[i][6], (float)FollowValue[i][7], F35BObject.transform.position.z);
                F35BObject.transform.rotation = Quaternion.Euler(-(float)FollowValue[i][8] * 180.0f / Mathf.PI, 90.0f, -90.0f);

                yield return new WaitForSeconds(0.001f);
            }
            TransferTime = FollowValue[FollowValue.Count - 1][9];
            Debug.Log(string.Format("�̵� �ҿ� �ð� : {0}", TransferTime));

            TransferTimeResultToPython(TransferTime);
        }
    }
    #endregion

    void TransferTimeResultToPython(double TransferTime) // ����� �̵� �ҿ� �ð� �۽�
    {
        try
        {
            // ! Send !
            DecksocketConnection.Send(BitConverter.GetBytes(TransferTime.ToString().Length)); // Byte[]�� ��ȯ�� �۽� �������� ����
            DecksocketConnection.Send(Encoding.UTF8.GetBytes(TransferTime.ToString())); // Byte[]�� Encoding�� �۽� ������
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e); // Server�� ������� ������ ������ ����
        }
    }

    void ProcessFinishSignalToPython() // ����� �۾� ���� �Ϸ� ����(0.0) �۽�
    {
        try
        {
            string ProcessFinishSignal = "0.0";

            // ! Send !
            DecksocketConnection.Send(BitConverter.GetBytes(ProcessFinishSignal.Length)); // Byte[]�� ��ȯ�� �۽� �������� ����
            DecksocketConnection.Send(Encoding.UTF8.GetBytes(ProcessFinishSignal)); // Byte[]�� Encoding�� �۽� ������
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e); // Server�� ������� ������ ������ ����
        }
    }
}
