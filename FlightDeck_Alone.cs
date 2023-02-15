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
using DG.Tweening; // DOTween ���� ���(�ٿ�) -> ProcessTime �ȿ� �����̴� �� �����ַ��� F35BObject.transform.DOMove(new Vector3(-300.0f, -9.5f, -60.4f), ProcessTime);
using SaveDeckData; // 221101 ���񱸿� ������ ��ȯ �� ������ ������ ���� ��ũ��Ʈ

/* ��ü �ּ�ó�� �ϴ� ���
  */

public class MoveObject : MonoBehaviour
{
    private int targetIndex = 0;
    private Vector3[] targets = {
        new Vector3(-80.0f, 9.0f, -30.2f),
        new Vector3(-63.0f, 9.0f, -30.2f),
        new Vector3(-40.5f, 7.5f, -30.2f),
        new Vector3(-22.0f, 9.0f, -30.2f),
        new Vector3(-5.0f, 9.0f, -30.2f)
    };

    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, targets[targetIndex], Time.deltaTime);
        if (transform.position == targets[targetIndex])
        {
            targetIndex = (targetIndex + 1) % targets.Length;
        }
    }
}

public class FlightDeck_Alone : MonoBehaviour // FlightDeck_Alone : �ܵ� ��忡���� ���఩�� ���� ���� Ŭ����
{
    Communication DeckInformationNSocket;
    public ArrayList DeckInformations; // ��� ��ũ��Ʈ���� ����Ϸ��� public����

    public GameObject InfosDelivery; // ����->���񱸿� �� ��ȯ��, DeckInformations�� �����ϱ� ����

    List<GameObject> SetUpAircraft = new List<GameObject>();

    public GameObject Tractor; //** Ʈ����
    public GameObject Towbar; //** ����
    

    public GameObject D1Elevator;
    public GameObject D2Elevator;

    public Camera MainCamera;
    public Camera D1EVCamera;
    public Camera D2EVCamera;

    #region ���� ���� ����
    public Socket DecksocketConnection;
    List<int> Ports;
    #endregion

    #region ����� �̵� ���� ���� ����
    Vector3 TakeOffSpot; // �̷� ���
    Vector3 LandingSpot; // ���� ���

    Vector3 D1EV; // D1 ����������(���񱸿�->����)
    Vector3 D2EV; // D2 ����������(����->���񱸿�)

    List<Vector3> ServiceA; // A ���� ����(A1,2,3,4,5)
    List<Vector3> ServiceB; // B ���� ����(B1,2,3,4,5)
    #endregion

    GameObject NewAircraft; // ���఩�ǿ����� ���峭 ������ ����, "���񱸿����� �ö�� ���ο� �����"
    void Start()
    {
        if (ReloadDeckScene.DeckServiceArea.Count>0) // ���񱸿����� ���ο� ����⸦ ���� �ö�°Ÿ�
        {
            #region ������ ���� �̸��� �� �����״�, �Ʒ� �ּ�ó���� �κ��� �ʿ� ������!
            /*
            int childAreasCount = GameObject.Find("GameObject").transform.childCount;
            for(int i=0; i< childAreasCount; i++)
            {
                Destroy(GameObject.Find("GameObject").transform.GetChild(i));
            }
            */
            #endregion
            Tractor.SetActive(false); // �缺 : SetActive �� ������Ʈ�� Ȱ��ȭ, ��Ȱ��ȭ �ϴ� �Լ��̴�.������Ʈ�� ��Ȱ��ȭ ��Ű�� ������Ʈ ��ü�� ��, ���Ӻ信�� ������� �۵����� �ʽ��ϴ�.
            Towbar.SetActive(false);

            D1Elevator.transform.position = new Vector3(0.0f, 0.0f, 10.3f);

            NewAircraft = GameObject.Find("D1Elevator").transform.Find("NewAircraft").gameObject;
            NewAircraft.SetActive(true);

            MainCamera.enabled = false;
            D1EVCamera.enabled = true;

            DecksocketConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            
            SetAircraftsPrevious();
            return;
        }
        GameObject Information_Object = GameObject.Find("InfoObject").gameObject; // ����Ƽ ���̾��Ű â�� "InfoObject"��� �̸��� GameObject�� ã�� ���� hierarchy : ����
        DeckInformationNSocket = Information_Object.GetComponent<Communication>();

        DeckInformations = DeckInformationNSocket.MakeInfomations();

        Ports = (List<int>)DeckInformations[0];
        ReloadDeckScene.InternalPort = Ports[1];
        DecksocketConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        #region ����� �̵� ��ġ ���� �Ҵ�
        TakeOffSpot = new Vector3(-80.0f, -9.5f, -30.2f); // ����� ���� : (180.0f, 90.0f, -90.0f)
        LandingSpot = new Vector3(-41.5f, -9.5f, -30.2f); // ����� ���� : (180.0f, 90.0f, -90.0f)

        D1EV = new Vector3(-40.5f, 25.0f, -30.2f); // (-40.5f, 26.0f, -30.2f);
        D2EV = new Vector3(-91.0f, -25.0f, -30.2f); // (-91.0f, -26.0f, -30.2f); -> A4 �������� �� �� ã��..

        ServiceA = new List<Vector3>()
        { new Vector3(-80.0f, 9.0f, -30.2f), new Vector3(-63.0f, 9.0f, -30.2f), new Vector3(-40.5f, 7.5f, -30.2f), new Vector3(-22.0f, 9.0f, -30.2f), new Vector3(-5.0f, 9.0f, -30.2f) }; // A1,2,3,4,5 ������

        ServiceB = new List<Vector3>()
        { new Vector3(-250.0f, 9.0f, -30.2f), new Vector3(-233.0f, 9.0f, -30.2f), new Vector3(-216.0f, 9.0f, -30.2f), new Vector3(-199.0f, 9.0f, -30.2f), new Vector3(-182.0f, 9.0f, -30.2f) }; // B1,2,3,4,5 ������
        #endregion

        SetAircrafts();
        SetResources();

        Tractor.SetActive(false);
        Towbar.SetActive(false);

        MainCamera.enabled = true;
        D1EVCamera.enabled = false;
        D2EVCamera.enabled = false;
    }

    public void SetAircrafts() // �ش�Ǵ� ���� ������ ����� �̸�, ����� �ڼ� ����
    {
        List<ArrayList> DeckAircrafts = (List<ArrayList>)DeckInformations[1]; //�갡 ����

        for (int i = 0; i < DeckAircrafts.Count; i++)
        {
            ArrayList DeckAircraftIndex = DeckAircrafts[i]; // DeckAircraftIndex : ���� ����[0], ����� �̸�[1], ����� ����[2] ������ ���� �ִ� ArrayList.

            SetUpAircraft.Add(GameObject.Find("GameObject").transform.Find((string)DeckAircraftIndex[0]).gameObject); // ���õ� ������ gameObject ���·� List�� �־���
            GameObject.Find("GameObject").transform.Find((string)DeckAircraftIndex[0]).gameObject.SetActive(true); // ���õ� ������ ���� ���̰� ����

            SetUpAircraft[i].name = (string)DeckAircraftIndex[1]; // ����� �̸�
            if ((float)DeckAircraftIndex[2] == 315.0f) // ����� �Է� : -45.0f(315.0f)
            {
                SetUpAircraft[i].transform.eulerAngles = new Vector3(135.0f, 90.0f, -90.0f);
            }

            else // ����� �Է� : 45.0f
            {
                SetUpAircraft[i].transform.eulerAngles = new Vector3((float)DeckAircraftIndex[2], 90.0f, -90.0f);
            }
        }
        Invoke("MakeSocket", 0.5f);
    }

    public void SetResources() // ���఩�� �� �ʿ� �ڿ���
    {

    }

    void SetAircraftsPrevious() // *221101 ���񱸿����� �������� ���� �ٽ� ��ȯ���� ��, ���� �����ͷ��� ����� ��ġ �Լ�
    {
        // GetChildInfos : GameObject �ؿ� �ִ� A1, A2, A3..�� �����Ϸ���
        Transform GetChildInfos = GameObject.Find("GameObject").transform;
        List<GameObject> SetAliveObjects = new List<GameObject>();

        for (int i = 0; i < ReloadDeckScene.DeckServiceArea.Count; i++)
        {
            SetAliveObjects.Add(GetChildInfos.GetChild(i).gameObject);
            SetAliveObjects[i].SetActive(true);
        }

        int Indexnumber = 0;
        foreach(KeyValuePair<string, Vector3> value in ReloadDeckScene.DeckServiceArea)
        {
            SetAliveObjects[Indexnumber].name = value.Key;
            SetAliveObjects[Indexnumber].transform.position = value.Value;
            SetAliveObjects[Indexnumber].transform.eulerAngles = new Vector3(ReloadDeckScene.ParkingAngle, 90.0f, -90.0f);
            Indexnumber++;
        }

        Invoke("MakeSocket", 0.5f);
    }

    void MakeSocket() // ���� ������ �ѹ��̸� �ȴ�! (�׷��ϱ� Start()�� �ᵵ �ɵ�?)
    {
        try
        {
            DecksocketConnection.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"),  ReloadDeckScene.InternalPort)); // �ٽ� ���ƿ����� ���⼭ Connect�� ����(���� ���� ��!)
            DecksocketConnection.Blocking = false; // ��-���ŷ ����
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e); // Server�� ������� ������ ������ ����
        }

        if (DecksocketConnection.Connected) InvokeRepeating("ReceivedFromPython", 0.0f, 0.05f); //  DecksocketConnection.Blocking = false; ���� "���� ����" �߻� X
    }



    void ReceivedFromPython() // Python���κ��� ������ �޴� �Լ� �ڿ���
    {
        var ReceiveData = new byte[4];
        DecksocketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None); // Updata()��, InvokeRepeating()�̳� ���⼭ ����
        Array.Reverse(ReceiveData);
        ReceiveData = new byte[BitConverter.ToInt32(ReceiveData, 0)];
        DecksocketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);

        string ReceivedDataValue = Encoding.UTF8.GetString(ReceiveData);
        Debug.Log("From Python : " + ReceivedDataValue);
        DataProcessing(ReceivedDataValue);
    }

    //void ReceivedFromPython()
    //{
    //    var ReceiveData = new byte[4];
    //    DecksocketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);
    //    Array.Reverse(ReceiveData);
    //    ReceiveData = new byte[BitConverter.ToInt32(ReceiveData, 0)];
    //    System.Threading.Thread.Sleep(100); // 100 �и��� ���
    //    DecksocketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);

    //    string ReceivedDataValue = Encoding.UTF8.GetString(ReceiveData);
    //    Debug.Log("From Python : " + ReceivedDataValue);
    //    DataProcessing(ReceivedDataValue);
    //}

    void DataProcessing(string fromSimPy) // ����� �̸�[0], �۾�[1], �ҿ� �ð�[2]
    {
        string[] AircraftName_Process_Time = fromSimPy.Split(',');

        #region ����� �̵�(��ȣ �Լ� Call)
        if (AircraftName_Process_Time[2] == "0.0")
        {
            if (AircraftName_Process_Time[1] == "ToTakeOff")
            {
                Taxiing(AircraftName_Process_Time[0], TakeOffSpot);
            }

            else if (AircraftName_Process_Time[1] == "ToAarea2") // �缺 �信���2 ����Ʈ����
            {
                System.Random random = new System.Random();
                int ChooseAreaIndex = random.Next(ServiceA.Count); // random.Next : 0�̻� ServiceA.Count�̸� ������ �����ϴ� ���� return
                Vector3 ChooseArea = ServiceA[ChooseAreaIndex];
                ServiceA.Remove(ChooseArea);

                RedGroup(AircraftName_Process_Time[0], ChooseArea, 135.0f); //TowbarTractor(AircraftName_Process_Time[0], ChooseArea, 135.0f);
            }

            else if (AircraftName_Process_Time[1] == "ToD2EV")
            {
                Debug.Log(string.Format("���峭 ����� : {0}", AircraftName_Process_Time[0]));
                RedGroup(AircraftName_Process_Time[0], D2EV, 270.0f);
            }

            else if (AircraftName_Process_Time[1] == "ToAarea")
            {
                System.Random random = new System.Random();
                int ChooseAreaIndex = random.Next(ServiceA.Count); // random.Next : 0�̻� ServiceA.Count�̸� ������ �����ϴ� ���� return
                Vector3 ChooseArea = ServiceA[ChooseAreaIndex];
                ServiceA.Remove(ChooseArea);

                TowbarTractor(AircraftName_Process_Time[0], ChooseArea, 135.0f);
            }

            else if (AircraftName_Process_Time[1] == "ToD2EV")
            {
                Debug.Log(string.Format("���峭 ����� : {0}", AircraftName_Process_Time[0]));
                TowbarTractor(AircraftName_Process_Time[0], D2EV, 270.0f);
            }


        }
        #endregion

        #region ������� �̵��� ������ ��� �۾� �� Coroutine ��� ��
        #region �ٸ� ��� ����
        // 1. DG.Tweening�̶� ProcessFinishSignalToPython()�� ����ϸ�, SimPy�� ��ȣ������ �ٷ� ���� �۾����� �Ѿ
        // 2. DG.Tweening�̶� Invoke("ProcessFinishSignalToPython", ProcessTime)�� ����ϸ� �������� ����⸦ ���ÿ� ����ȭ ����
        // [221027] ������ ����� ����ϰ� �ʹٸ�, SimPy���� ��ȣ �޾Ƽ� ó���ϴ� �κ��� �����ؾ����� ������?
        #endregion
        else
        {
            float ProcessTime = float.Parse(AircraftName_Process_Time[2]);
            GameObject F35BObject = new GameObject();

            if (NewAircraft == null) // [221103] ���񱸿����� �ö�� ���ο� ����⶧���� �ش� if�� ���� -> �ٵ� �ùķ��̼��� �ѹ� �� ���� ���, �����ʿ�
            {
                F35BObject = GameObject.Find(AircraftName_Process_Time[0]);

                #region ���ӿ�����Ʈ�� setactive(false) -> setactive(true)�ϴ� �������� ���ӿ�����Ʈ�� ��ã�´ٸ�
                if (F35BObject == null) // ���� ã�°� ���� �Լ����� ��������
                {
                    F35BObject = GameObject.Find("GameObject").transform.Find(AircraftName_Process_Time[0]).gameObject;
                }
                #endregion
            }
            
            // �缺 �̷��ϱ� ���� ����׷��� ���� �ϴ� �ڵ带 ���� ��ġ

            if (AircraftName_Process_Time[1] == "TakeOff") // ����� �̷�
            {
                StartCoroutine(forTakeOff());
                IEnumerator forTakeOff()
                {
                    for (int i = 0; i < 110; i++) // (-80.0f, -9.5f, -30.2f)[�̷� ���] -> (-300.0f, -9.5f, -60.4f)[���1]
                    {
                        F35BObject.transform.position += new Vector3(-2.0f, 0.0f);
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
                    }
                    ProcessFinishSignalToPython();
                }
            }

            else if (AircraftName_Process_Time[1] == "Land") // ����� ����
            {
                F35BObject.SetActive(true);

                StartCoroutine(forLand());
                IEnumerator forLand()
                {
                    for (int i = 0; i < 10; i++) // (40.0f, -9.5f, -60.4f)[���2] -> (-41.5f, -9.5f, -30.2f)[���� ���]
                    {
                        F35BObject.transform.position += new Vector3(-8.15f, 0.0f);
                        F35BObject.transform.position += new Vector3(0.0f, 0.0f, 3.02f);

                        yield return new WaitForSeconds(0.03f); // *221024
                        //yield return new WaitForSeconds(0.05f); // ���ڰ� ũ�� ���ڿ������� ������, �� �� �� ��
                        ////yield return new WaitForSeconds(0.025f); // ���ڰ� �۴ٴ°� �׸�ŭ ���� for���� ���ٴ� �� => �������� �ڿ�������
                    }
                    ProcessFinishSignalToPython();
                }
            }

            else if (AircraftName_Process_Time[1] == "Inspection") // ����� ����
            {
                Debug.Log(string.Format("{0} ����� ���� ��!", F35BObject.name));
                ProcessFinishSignalToPython();
            }

            else if (AircraftName_Process_Time[1] == "Fueling") // ����� ����
            {
                Debug.Log(string.Format("{0} ����� ���� ��!", F35BObject.name));
                ProcessFinishSignalToPython();
            }

            else if (AircraftName_Process_Time[1] == "ToHangar") // ����Ⱑ D2 ���������Ϳ� ���� �̵�
            {
                //*221101 �� �κ� �ٲ������� ��
                DeckInformations.Add(F35BObject.name); // DeckInformations.Count == 7 -> ���峭 ����Ⱑ ����ٴ� �ǹ�
                                                          //DeckInformations[6] = null; // *221026 ���߿� �ش� ��(DeckInformations[6])�� ���������� Ȯ���غ���!
                Tractor.SetActive(false);
                Towbar.SetActive(false);

                MainCamera.enabled = false;
                D2EVCamera.enabled = true;

                F35BObject.transform.parent = D2Elevator.transform;

                D2Elevator.transform.DOMove(new Vector3(0.0f, 0.0f, 10.3f), ProcessTime);
                Invoke("ChangeScene", ProcessTime); // ���� ��ŷ� ����ȯ
            }

            else if (AircraftName_Process_Time[1] == "ToDeck") // ���� ���� ����Ⱑ D1 ���������ͷ� �̵�
            {
                NewAircraft.name = AircraftName_Process_Time[0];
                
                D1Elevator.transform.DOMove(new Vector3(0.0f, 0.0f, 0.0f), ProcessTime);
                ProcessFinishSignalToPython();
                CancelInvoke("ReceivedFromPython");
            }
        }
        #endregion
    }

    public void ChangeScene() // ���఩�� -> ���񱸿� ������
    {
        DecksocketConnection.Close();
        CancelInvoke("ReceivedFromPython"); // ReceivedFromPython �Լ� �ߴ�

        SavenowInfos(); // ���񱸿����� ���఩�� ���� �θ��� ����Ϸ���

        SceneManager.LoadScene("Hangar_Alone"); // *221103 ���� �о��� ���񱸿� Ȯ�ζ����� ��ø� �ּ� ó�� �� �Ұ�~
        //SceneManager.LoadScene("HangarWide_Alone"); // 221103 ���� 6m �о��� ���񱸿������� Ȯ�� �ʿ� + bulid setting Ȯ��
        DontDestroyOnLoad(InfosDelivery);
    }

    void SavenowInfos() // ���񱸿����� ���� ��, ������ �����ϴ� �Լ�
    {
        Transform GetChildInfos = GameObject.Find("GameObject").transform;
        List<GameObject> AliveObjects = new List<GameObject>();

        for (int i = 0; i < GetChildInfos.childCount; i++)
        {
            if (GetChildInfos.GetChild(i).gameObject.activeInHierarchy)
            {
                AliveObjects.Add(GetChildInfos.GetChild(i).gameObject);
            }
        }
        ReloadDeckScene.ParkingAngle = UnityEditor.TransformUtils.GetInspectorRotation(AliveObjects[0].transform).x;

        for (int i = 0; i < AliveObjects.Count; i++)
        {
            ReloadDeckScene.DeckServiceArea[AliveObjects[i].name] = AliveObjects[i].transform.position;
        }
    }

    float DeckAngleAdjustFunc(float myAngle)
    {
        return 360.0f - myAngle;
    }

    #region �̵� ���� �Լ���(��ȣ ���� �߰�)
    public void Taxiing(string AircraftName, Vector3 Destination)
    {
        // ��ȣ dll -> ��! ���� ������ ����
        Pathgeneration_Aircraft_V1.pathgeneration MakePath = new Pathgeneration_Aircraft_V1.pathgeneration();
        Pathfollowing_Aircraft_V1.pathfollowing FollowPath = new Pathfollowing_Aircraft_V1.pathfollowing();

        double TransferTime;
        GameObject F35BObject = GameObject.Find(AircraftName);

        //Debug.Log(string.Format("�̸�:{0}, inspectoreuler����:{1}", F35BObject.name, UnityEditor.TransformUtils.GetInspectorRotation(F35BObject.transform)));

        var WayPoints = MakePath.path(F35BObject.transform.position, Destination, DeckAngleAdjustFunc(UnityEditor.TransformUtils.GetInspectorRotation(F35BObject.transform).x), 180.0f);
        var FollowValue = FollowPath.path_following(WayPoints); // FollowValue : 4������ �̷���� ���� => ����� x,y, x ���� ����, �ð�(������ ���� �� �ҿ� �ð�)

        StartCoroutine(Move_Taxiing());
        IEnumerator Move_Taxiing()
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

    //red �ڿ�
    public void RedGroup(string AircraftName, Vector3 Destination, float DestiAngle) //DestiAngle : �� ���� ���� // �缺 �� ������ �ڷ�ƾ���� Ʈ���������� �̵��� �ϴ� �Լ�
    {
        // ��ȣ dll -> ��! ���� ������ ����
        Pathgeneration_Towbar_V2.pathgeneration MakePath = new Pathgeneration_Towbar_V2.pathgeneration();
        Pathfollowing_Towbar_V5.pathfollowing FollowPath = new Pathfollowing_Towbar_V5.pathfollowing();

        double TransferTime;
        GameObject F35BObject = GameObject.Find(AircraftName);

        var WayPoints = MakePath.path(F35BObject.transform.position, Destination, DeckAngleAdjustFunc(UnityEditor.TransformUtils.GetInspectorRotation(F35BObject.transform).x), DeckAngleAdjustFunc(DestiAngle));
        var FollowValue = FollowPath.path_following(WayPoints); // FollowValue : 10������ �̷���� ���� => Ʈ������ x,y,angle / Towbar�� x,y,angle / ������� x,y,angle / �̵� �ҿ� �ð�

        StartCoroutine(Move_RedGroup());
        IEnumerator Move_RedGroup()
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
    public void TowbarTractor(string AircraftName, Vector3 Destination, float DestiAngle) //DestiAngle : �� ���� ���� // �缺 �� ������ �ڷ�ƾ���� Ʈ���������� �̵��� �ϴ� �Լ�
    {
        // ��ȣ dll -> ��! ���� ������ ����
        Pathgeneration_Towbar_V2.pathgeneration MakePath = new Pathgeneration_Towbar_V2.pathgeneration();
        Pathfollowing_Towbar_V5.pathfollowing FollowPath = new Pathfollowing_Towbar_V5.pathfollowing();

        double TransferTime;
        GameObject F35BObject = GameObject.Find(AircraftName);

        var WayPoints = MakePath.path(F35BObject.transform.position, Destination, DeckAngleAdjustFunc(UnityEditor.TransformUtils.GetInspectorRotation(F35BObject.transform).x), DeckAngleAdjustFunc(DestiAngle));
        var FollowValue = FollowPath.path_following(WayPoints); // FollowValue : 10������ �̷���� ���� => Ʈ������ x,y,angle / Towbar�� x,y,angle / ������� x,y,angle / �̵� �ҿ� �ð�

        StartCoroutine(Move_Towbar());
        IEnumerator Move_Towbar()
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
            UnityEngine.Debug.Log("On client connect exception " + e);
        }
    }

    void ProcessFinishSignalToPython() // ����� �۾� ���� �Ϸ� ����(0.0) �۽�
    {
        try
        {
            string ProcessFinishSignal = "0.0";

            // ! Send !
            DecksocketConnection.Send(BitConverter.GetBytes(ProcessFinishSignal.Length));
            DecksocketConnection.Send(Encoding.UTF8.GetBytes(ProcessFinishSignal));
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e);
        }
    }
}

