using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using System.Globalization;
using System.Net.Sockets;
using System.Net;
using Pathgeneration_Aircraft_V1; // 송진호 경로 생성(Taxiing)
using Pathfollowing_Aircraft_V1; // 송진호 경로 추종(Taxiing)
using Pathfollowing_Towbar_V5;
using Pathgeneration_Towbar_V2;
using DG.Tweening; // DOTween 에셋 사용(다운) -> ProcessTime 안에 움직이는 걸 보여주려고 F35BObject.transform.DOMove(new Vector3(-300.0f, -9.5f, -60.4f), ProcessTime);
using SaveDeckData; // 221101 정비구역 씬으로 변환 전 데이터 저장을 위한 스크립트

/* 단체 주석처리 하는 방법
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

public class FlightDeck_Alone : MonoBehaviour // FlightDeck_Alone : 단독 모드에서의 비행갑판 구역 관련 클래스
{
    Communication DeckInformationNSocket;
    public ArrayList DeckInformations; // 행거 스크립트에서 사용하려고 public으로

    public GameObject InfosDelivery; // 갑판->정비구역 씬 변환시, DeckInformations에 접근하기 위함

    List<GameObject> SetUpAircraft = new List<GameObject>();

    public GameObject Tractor; //** 트랙터
    public GameObject Towbar; //** 토우바
    

    public GameObject D1Elevator;
    public GameObject D2Elevator;

    public Camera MainCamera;
    public Camera D1EVCamera;
    public Camera D2EVCamera;

    #region 소켓 관련 변수
    public Socket DecksocketConnection;
    List<int> Ports;
    #endregion

    #region 함재기 이동 관련 변수 선언
    Vector3 TakeOffSpot; // 이륙 장소
    Vector3 LandingSpot; // 착륙 장소

    Vector3 D1EV; // D1 엘리베이터(정비구역->갑판)
    Vector3 D2EV; // D2 엘리베이터(갑판->정비구역)

    List<Vector3> ServiceA; // A 서비스 구역(A1,2,3,4,5)
    List<Vector3> ServiceB; // B 서비스 구역(B1,2,3,4,5)
    #endregion

    GameObject NewAircraft; // 비행갑판에서의 고장난 함재기로 인해, "정비구역에서 올라온 새로운 함재기"
    void Start()
    {
        if (ReloadDeckScene.DeckServiceArea.Count>0) // 정비구역에서 새로운 함재기를 꺼내 올라온거면
        {
            #region 어차피 구역 이름은 안 켜질테니, 아래 주석처리한 부분은 필요 없을듯!
            /*
            int childAreasCount = GameObject.Find("GameObject").transform.childCount;
            for(int i=0; i< childAreasCount; i++)
            {
                Destroy(GameObject.Find("GameObject").transform.GetChild(i));
            }
            */
            #endregion
            Tractor.SetActive(false); // 재성 : SetActive 는 오브젝트를 활성화, 비활성화 하는 함수이다.오브젝트를 비활성화 시키면 오브젝트 자체가 씬, 게임뷰에서 사라지고 작동하지 않습니다.
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
        GameObject Information_Object = GameObject.Find("InfoObject").gameObject; // 유니티 하이어라키 창의 "InfoObject"라는 이름의 GameObject를 찾기 위해 hierarchy : 계층
        DeckInformationNSocket = Information_Object.GetComponent<Communication>();

        DeckInformations = DeckInformationNSocket.MakeInfomations();

        Ports = (List<int>)DeckInformations[0];
        ReloadDeckScene.InternalPort = Ports[1];
        DecksocketConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        #region 함재기 이동 위치 변수 할당
        TakeOffSpot = new Vector3(-80.0f, -9.5f, -30.2f); // 함재기 각도 : (180.0f, 90.0f, -90.0f)
        LandingSpot = new Vector3(-41.5f, -9.5f, -30.2f); // 함재기 각도 : (180.0f, 90.0f, -90.0f)

        D1EV = new Vector3(-40.5f, 25.0f, -30.2f); // (-40.5f, 26.0f, -30.2f);
        D2EV = new Vector3(-91.0f, -25.0f, -30.2f); // (-91.0f, -26.0f, -30.2f); -> A4 구역에서 길 못 찾네..

        ServiceA = new List<Vector3>()
        { new Vector3(-80.0f, 9.0f, -30.2f), new Vector3(-63.0f, 9.0f, -30.2f), new Vector3(-40.5f, 7.5f, -30.2f), new Vector3(-22.0f, 9.0f, -30.2f), new Vector3(-5.0f, 9.0f, -30.2f) }; // A1,2,3,4,5 순으로

        ServiceB = new List<Vector3>()
        { new Vector3(-250.0f, 9.0f, -30.2f), new Vector3(-233.0f, 9.0f, -30.2f), new Vector3(-216.0f, 9.0f, -30.2f), new Vector3(-199.0f, 9.0f, -30.2f), new Vector3(-182.0f, 9.0f, -30.2f) }; // B1,2,3,4,5 순으로
        #endregion

        SetAircrafts();
        SetResources();

        Tractor.SetActive(false);
        Towbar.SetActive(false);

        MainCamera.enabled = true;
        D1EVCamera.enabled = false;
        D2EVCamera.enabled = false;
    }

    public void SetAircrafts() // 해당되는 서비스 구역에 함재기 이름, 함재기 자세 정렬
    {
        List<ArrayList> DeckAircrafts = (List<ArrayList>)DeckInformations[1]; //얘가 정답

        for (int i = 0; i < DeckAircrafts.Count; i++)
        {
            ArrayList DeckAircraftIndex = DeckAircrafts[i]; // DeckAircraftIndex : 서비스 구역[0], 함재기 이름[1], 함재기 각도[2] 정보를 갖고 있는 ArrayList.

            SetUpAircraft.Add(GameObject.Find("GameObject").transform.Find((string)DeckAircraftIndex[0]).gameObject); // 선택된 함재기들 gameObject 형태로 List에 넣어줘
            GameObject.Find("GameObject").transform.Find((string)DeckAircraftIndex[0]).gameObject.SetActive(true); // 선택된 함재기들 눈에 보이게 해줘

            SetUpAircraft[i].name = (string)DeckAircraftIndex[1]; // 함재기 이름
            if ((float)DeckAircraftIndex[2] == 315.0f) // 사용자 입력 : -45.0f(315.0f)
            {
                SetUpAircraft[i].transform.eulerAngles = new Vector3(135.0f, 90.0f, -90.0f);
            }

            else // 사용자 입력 : 45.0f
            {
                SetUpAircraft[i].transform.eulerAngles = new Vector3((float)DeckAircraftIndex[2], 90.0f, -90.0f);
            }
        }
        Invoke("MakeSocket", 0.5f);
    }

    public void SetResources() // 비행갑판 위 필요 자원들
    {

    }

    void SetAircraftsPrevious() // *221101 정비구역에서 갑판으로 씬을 다시 전환했을 때, 이전 데이터로의 함재기 배치 함수
    {
        // GetChildInfos : GameObject 밑에 있는 A1, A2, A3..에 접근하려고
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

    void MakeSocket() // 소켓 연결은 한번이면 된다! (그러니까 Start()에 써도 될듯?)
    {
        try
        {
            DecksocketConnection.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"),  ReloadDeckScene.InternalPort)); // 다시 돌아왔을때 여기서 Connect를 못함(소켓 연결 끝!)
            DecksocketConnection.Blocking = false; // 논-블로킹 구현
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e); // Server가 실행되지 않으면 나오는 에러
        }

        if (DecksocketConnection.Connected) InvokeRepeating("ReceivedFromPython", 0.0f, 0.05f); //  DecksocketConnection.Blocking = false; 덕에 "응답 없음" 발생 X
    }



    void ReceivedFromPython() // Python으로부터 정보를 받는 함수 ★원본
    {
        var ReceiveData = new byte[4];
        DecksocketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None); // Updata()나, InvokeRepeating()이나 여기서 멈춤
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
    //    System.Threading.Thread.Sleep(100); // 100 밀리초 대기
    //    DecksocketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);

    //    string ReceivedDataValue = Encoding.UTF8.GetString(ReceiveData);
    //    Debug.Log("From Python : " + ReceivedDataValue);
    //    DataProcessing(ReceivedDataValue);
    //}

    void DataProcessing(string fromSimPy) // 함재기 이름[0], 작업[1], 소요 시간[2]
    {
        string[] AircraftName_Process_Time = fromSimPy.Split(',');

        #region 함재기 이동(진호 함수 Call)
        if (AircraftName_Process_Time[2] == "0.0")
        {
            if (AircraftName_Process_Time[1] == "ToTakeOff")
            {
                Taxiing(AircraftName_Process_Time[0], TakeOffSpot);
            }

            else if (AircraftName_Process_Time[1] == "ToAarea2") // 재성 토에어리어2 토우바트랙터
            {
                System.Random random = new System.Random();
                int ChooseAreaIndex = random.Next(ServiceA.Count); // random.Next : 0이상 ServiceA.Count미만 범위에 존재하는 정수 return
                Vector3 ChooseArea = ServiceA[ChooseAreaIndex];
                ServiceA.Remove(ChooseArea);

                RedGroup(AircraftName_Process_Time[0], ChooseArea, 135.0f); //TowbarTractor(AircraftName_Process_Time[0], ChooseArea, 135.0f);
            }

            else if (AircraftName_Process_Time[1] == "ToD2EV")
            {
                Debug.Log(string.Format("고장난 함재기 : {0}", AircraftName_Process_Time[0]));
                RedGroup(AircraftName_Process_Time[0], D2EV, 270.0f);
            }

            else if (AircraftName_Process_Time[1] == "ToAarea")
            {
                System.Random random = new System.Random();
                int ChooseAreaIndex = random.Next(ServiceA.Count); // random.Next : 0이상 ServiceA.Count미만 범위에 존재하는 정수 return
                Vector3 ChooseArea = ServiceA[ChooseAreaIndex];
                ServiceA.Remove(ChooseArea);

                TowbarTractor(AircraftName_Process_Time[0], ChooseArea, 135.0f);
            }

            else if (AircraftName_Process_Time[1] == "ToD2EV")
            {
                Debug.Log(string.Format("고장난 함재기 : {0}", AircraftName_Process_Time[0]));
                TowbarTractor(AircraftName_Process_Time[0], D2EV, 270.0f);
            }


        }
        #endregion

        #region 함재기의 이동을 제외한 운용 작업 ★ Coroutine 사용 ★
        #region 다른 방법 설명
        // 1. DG.Tweening이랑 ProcessFinishSignalToPython()을 사용하면, SimPy의 신호때문에 바로 다음 작업으로 넘어감
        // 2. DG.Tweening이랑 Invoke("ProcessFinishSignalToPython", ProcessTime)을 사용하면 여러대의 함재기를 동시에 가시화 못함
        // [221027] 후자의 방법을 사용하고 싶다면, SimPy에서 신호 받아서 처리하는 부분을 수정해야하지 않을까?
        #endregion
        else
        {
            float ProcessTime = float.Parse(AircraftName_Process_Time[2]);
            GameObject F35BObject = new GameObject();

            if (NewAircraft == null) // [221103] 정비구역에서 올라온 새로운 함재기때문에 해당 if문 생성 -> 근데 시뮬레이션을 한번 더 돌릴 경우, 수정필요
            {
                F35BObject = GameObject.Find(AircraftName_Process_Time[0]);

                #region 게임오브젝트를 setactive(false) -> setactive(true)하는 과정에서 게임오브젝트를 못찾는다면
                if (F35BObject == null) // 내가 찾는게 이전 함수에서 꺼놨으면
                {
                    F35BObject = GameObject.Find("GameObject").transform.Find(AircraftName_Process_Time[0]).gameObject;
                }
                #endregion
            }
            
            // 재성 이륙하기 전에 레드그룹이 정비 하는 코드를 넣을 위치

            if (AircraftName_Process_Time[1] == "TakeOff") // 함재기 이륙
            {
                StartCoroutine(forTakeOff());
                IEnumerator forTakeOff()
                {
                    for (int i = 0; i < 110; i++) // (-80.0f, -9.5f, -30.2f)[이륙 장소] -> (-300.0f, -9.5f, -60.4f)[상공1]
                    {
                        F35BObject.transform.position += new Vector3(-2.0f, 0.0f);
                        F35BObject.transform.position += new Vector3(0.0f, 0.0f, -0.27454545f); // 함재기가 위로 떠야하니까(z방향) -30.2f -> -60.4f

                        yield return new WaitForSeconds(0.005f);
                    }
                    ProcessFinishSignalToPython();
                }
            }

            else if (AircraftName_Process_Time[1] == "Mission") // 함재기 임무 수행
            {
                F35BObject.SetActive(false);

                StartCoroutine(forMission());
                IEnumerator forMission()
                {
                    for (int i = 0; i < 10; i++) // (40.0f, -9.5f, -60.4f)[상공2]
                    {
                        F35BObject.transform.position = new Vector3(40.0f, -9.5f, -60.4f); // 착륙 전, 상공에서의 대기 장소
                        yield return new WaitForSeconds(0.025f);
                    }
                    ProcessFinishSignalToPython();
                }
            }

            else if (AircraftName_Process_Time[1] == "Land") // 함재기 착륙
            {
                F35BObject.SetActive(true);

                StartCoroutine(forLand());
                IEnumerator forLand()
                {
                    for (int i = 0; i < 10; i++) // (40.0f, -9.5f, -60.4f)[상공2] -> (-41.5f, -9.5f, -30.2f)[착륙 장소]
                    {
                        F35BObject.transform.position += new Vector3(-8.15f, 0.0f);
                        F35BObject.transform.position += new Vector3(0.0f, 0.0f, 3.02f);

                        yield return new WaitForSeconds(0.03f); // *221024
                        //yield return new WaitForSeconds(0.05f); // 숫자가 크면 부자연스럽게 움직임, 딱 딱 딱 딱
                        ////yield return new WaitForSeconds(0.025f); // 숫자가 작다는건 그만큼 빨리 for문을 돈다는 것 => 움직임이 자연스러움
                    }
                    ProcessFinishSignalToPython();
                }
            }

            else if (AircraftName_Process_Time[1] == "Inspection") // 함재기 점검
            {
                Debug.Log(string.Format("{0} 함재기 점검 중!", F35BObject.name));
                ProcessFinishSignalToPython();
            }

            else if (AircraftName_Process_Time[1] == "Fueling") // 함재기 급유
            {
                Debug.Log(string.Format("{0} 함재기 급유 중!", F35BObject.name));
                ProcessFinishSignalToPython();
            }

            else if (AircraftName_Process_Time[1] == "ToHangar") // 함재기가 D2 엘리베이터와 같이 이동
            {
                //*221101 이 부분 바꿔줬으면 함
                DeckInformations.Add(F35BObject.name); // DeckInformations.Count == 7 -> 고장난 함재기가 생겼다는 의미
                                                          //DeckInformations[6] = null; // *221026 나중에 해당 값(DeckInformations[6])이 지워지는지 확인해보기!
                Tractor.SetActive(false);
                Towbar.SetActive(false);

                MainCamera.enabled = false;
                D2EVCamera.enabled = true;

                F35BObject.transform.parent = D2Elevator.transform;

                D2Elevator.transform.DOMove(new Vector3(0.0f, 0.0f, 10.3f), ProcessTime);
                Invoke("ChangeScene", ProcessTime); // 이제 행거로 씬변환
            }

            else if (AircraftName_Process_Time[1] == "ToDeck") // 새로 나온 함재기가 D1 엘리베이터로 이동
            {
                NewAircraft.name = AircraftName_Process_Time[0];
                
                D1Elevator.transform.DOMove(new Vector3(0.0f, 0.0f, 0.0f), ProcessTime);
                ProcessFinishSignalToPython();
                CancelInvoke("ReceivedFromPython");
            }
        }
        #endregion
    }

    public void ChangeScene() // 비행갑판 -> 정비구역 씬으로
    {
        DecksocketConnection.Close();
        CancelInvoke("ReceivedFromPython"); // ReceivedFromPython 함수 중단

        SavenowInfos(); // 정비구역에서 비행갑판 씬을 부를때 사용하려고

        SceneManager.LoadScene("Hangar_Alone"); // *221103 폭이 넓어진 정비구역 확인때문에 잠시만 주석 처리 좀 할게~
        //SceneManager.LoadScene("HangarWide_Alone"); // 221103 폭이 6m 넓어진 정비구역에서의 확인 필요 + bulid setting 확인
        DontDestroyOnLoad(InfosDelivery);
    }

    void SavenowInfos() // 정비구역으로 가기 전, 데이터 저장하는 함수
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

    #region 이동 관련 함수들(진호 파일 추가)
    public void Taxiing(string AircraftName, Vector3 Destination)
    {
        // 진호 dll -> 꼭! 지역 변수로 설정
        Pathgeneration_Aircraft_V1.pathgeneration MakePath = new Pathgeneration_Aircraft_V1.pathgeneration();
        Pathfollowing_Aircraft_V1.pathfollowing FollowPath = new Pathfollowing_Aircraft_V1.pathfollowing();

        double TransferTime;
        GameObject F35BObject = GameObject.Find(AircraftName);

        //Debug.Log(string.Format("이름:{0}, inspectoreuler각도:{1}", F35BObject.name, UnityEditor.TransformUtils.GetInspectorRotation(F35BObject.transform)));

        var WayPoints = MakePath.path(F35BObject.transform.position, Destination, DeckAngleAdjustFunc(UnityEditor.TransformUtils.GetInspectorRotation(F35BObject.transform).x), 180.0f);
        var FollowValue = FollowPath.path_following(WayPoints); // FollowValue : 4행으로 이루어져 있음 => 함재기 x,y, x 방향 각도, 시간(마지막 값이 총 소요 시간)

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
            Debug.Log(string.Format("이동 소요 시간 : {0}", TransferTime));

            TransferTimeResultToPython(TransferTime);
        }
    }

    //red 자원
    public void RedGroup(string AircraftName, Vector3 Destination, float DestiAngle) //DestiAngle : 내 기준 각도 // 재성 이 파일이 코루틴으로 트랙터토우바의 이동을 하는 함수
    {
        // 진호 dll -> 꼭! 지역 변수로 설정
        Pathgeneration_Towbar_V2.pathgeneration MakePath = new Pathgeneration_Towbar_V2.pathgeneration();
        Pathfollowing_Towbar_V5.pathfollowing FollowPath = new Pathfollowing_Towbar_V5.pathfollowing();

        double TransferTime;
        GameObject F35BObject = GameObject.Find(AircraftName);

        var WayPoints = MakePath.path(F35BObject.transform.position, Destination, DeckAngleAdjustFunc(UnityEditor.TransformUtils.GetInspectorRotation(F35BObject.transform).x), DeckAngleAdjustFunc(DestiAngle));
        var FollowValue = FollowPath.path_following(WayPoints); // FollowValue : 10행으로 이루어져 있음 => 트랙터의 x,y,angle / Towbar의 x,y,angle / 함재기의 x,y,angle / 이동 소요 시간

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
            Debug.Log(string.Format("이동 소요 시간 : {0}", TransferTime));

            TransferTimeResultToPython(TransferTime);
        }
    }
    public void TowbarTractor(string AircraftName, Vector3 Destination, float DestiAngle) //DestiAngle : 내 기준 각도 // 재성 이 파일이 코루틴으로 트랙터토우바의 이동을 하는 함수
    {
        // 진호 dll -> 꼭! 지역 변수로 설정
        Pathgeneration_Towbar_V2.pathgeneration MakePath = new Pathgeneration_Towbar_V2.pathgeneration();
        Pathfollowing_Towbar_V5.pathfollowing FollowPath = new Pathfollowing_Towbar_V5.pathfollowing();

        double TransferTime;
        GameObject F35BObject = GameObject.Find(AircraftName);

        var WayPoints = MakePath.path(F35BObject.transform.position, Destination, DeckAngleAdjustFunc(UnityEditor.TransformUtils.GetInspectorRotation(F35BObject.transform).x), DeckAngleAdjustFunc(DestiAngle));
        var FollowValue = FollowPath.path_following(WayPoints); // FollowValue : 10행으로 이루어져 있음 => 트랙터의 x,y,angle / Towbar의 x,y,angle / 함재기의 x,y,angle / 이동 소요 시간

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
            Debug.Log(string.Format("이동 소요 시간 : {0}", TransferTime));

            TransferTimeResultToPython(TransferTime);
        }
    }
    #endregion


    void TransferTimeResultToPython(double TransferTime) // 함재기 이동 소요 시간 송신
    {
        try
        {
            // ! Send !
            DecksocketConnection.Send(BitConverter.GetBytes(TransferTime.ToString().Length)); // Byte[]로 변환한 송신 데이터의 길이
            DecksocketConnection.Send(Encoding.UTF8.GetBytes(TransferTime.ToString())); // Byte[]로 Encoding한 송신 데이터
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e);
        }
    }

    void ProcessFinishSignalToPython() // 함재기 작업 수행 완료 상태(0.0) 송신
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

