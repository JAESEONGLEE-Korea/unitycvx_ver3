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
//using DG.Tweening; // DOTween 에셋 사용(다운) -> ProcessTime 안에 움직이는 걸 보여주려고 F35BObject.transform.DOMove(new Vector3(-300.0f, -9.5f, -60.4f), ProcessTime);

public class FlightDeck_Inte : MonoBehaviour // FlightDeck_Inte : 통합 모드에서의 비행갑판 구역 관련 클래스
{
    Communication DeckInformationNSocket;
    ArrayList DeckInformations; // DeckInformations -> 비행갑판 구역 정보

    List<GameObject> SetUpAircraft = new List<GameObject>(); // 사용자로부터 선택된 진짜 함재기들 

    #region 소켓 관련 변수
    public Socket DecksocketConnection;
    List<int> Ports;
    #endregion

    #region 함재기 이동 관련 변수 선언
    Vector2 TakeOffSpot; // 이륙 장소
    Vector2 LandingSpot; // 착륙 장소

    List<string> TemporalAAreaList = new List<string>() { "A2", "A3", "A4", "A5" }; // 진호 dll때문에 임시로 만들어둔 변수

    List<Vector2> ServiceA; // A 서비스 구역(A1,2,3,4,5)
    List<Vector2> ServiceB; // B 서비스 구역(B1,2,3,4,5)
    #endregion

    public GameObject Tractor; //** 트랙터
    public GameObject Towbar; //** 토우바

    void Start()
    {
        GameObject Information_Object = GameObject.Find("InfoObject").gameObject; // 유니티 하이어라키 창의 "InfoObject"라는 이름의 GameObject를 찾기 위해
        DeckInformationNSocket = Information_Object.GetComponent<Communication>();

        DeckInformations = DeckInformationNSocket.MakeInfomations(); // 사용자가 입력한 모든 정보를 갖고 있는 ArrayList

        Ports = (List<int>)DeckInformations[0]; // 내부 포트 번호(Ports[1]) 사용하려고
        DecksocketConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        #region 함재기 이동 관련 변수 할당
        TakeOffSpot = new Vector2(-80.0f, -9.5f); // 함재기 각도 : (180.0f, 90.0f, -90.0f)
        LandingSpot = new Vector2(-41.5f, -9.5f); // 함재기 각도 : (180.0f, 90.0f, -90.0f)

        ServiceA = new List<Vector2>()
        { new Vector2(-80.0f, 9.0f), new Vector2(-63.0f, 9.0f), new Vector2(-40.5f, 7.5f), new Vector2(-22.0f, 9.0f), new Vector2(-5.0f, 9.0f) }; // A1,2,3,4,5 순으로

        ServiceB = new List<Vector2>()
        { new Vector2(-250.0f, 9.0f), new Vector2(-233.0f, 9.0f), new Vector2(-216.0f, 9.0f), new Vector2(-199.0f, 9.0f), new Vector2(-182.0f, 9.0f) }; // B1,2,3,4,5 순으로
        #endregion

        SetAircrafts();

        Tractor.SetActive(false);
        Towbar.SetActive(false);

        //SetResources(); // 비행갑판 위 자원 관련 함수, 해당되는 위치? 아니면 개수만큼 자원 두기
    }

    public void SetAircrafts() // 해당되는 서비스 구역에 함재기 이름, 함재기 자세 정렬
    {
        // 1-1. object DeckObject = Informations[1]; // List<int>의 요소는 int 형태를 띄고 있듯, ArrayList의 요소는 object 형태를 띄고 있다.

        // ★ 1-2. (float or int or List<ArrayList> 등..)다른 형식 => 이것을 캐스트? 캐스팅이라고 한다! 우왕 이걸 오늘 알다니 
        // 현재 선언한 대다수의 변수는 모두 값이 있는 상태가 X -> 무슨 소리냐면 사용자가 입력을 해야 값이 들어옴!!
        // 그래서 Informations 변수들이 자신의 형태를 모름!! => Casting을 해줘야함
        List<ArrayList> DeckAircrafts = (List<ArrayList>)DeckInformations[1]; //얘가 정답

        for (int i = 0; i < DeckAircrafts.Count; i++)
        {
            ArrayList DeckAircraftIndex = DeckAircrafts[i]; // DeckAircraftIndex : 서비스 구역[0], 함재기 이름[1], 함재기 각도[2] 정보를 갖고 있는 ArrayList.

            SetUpAircraft.Add(GameObject.Find("GameObject").transform.Find((string)DeckAircraftIndex[0]).gameObject); // 선택된 함재기들 gameObject 형태로 List에 넣어줘
            GameObject.Find("GameObject").transform.Find((string)DeckAircraftIndex[0]).gameObject.SetActive(true); // 선택된 함재기들 눈에 보이게 해줘
            #region 함재기 절대, 로컬 좌표계 확인 결과 -> 절대 좌표계 = 로컬 좌표계
            //Debug.Log(String.Format("{0} 함재기 {1}에 위치(로컬 좌표계 {2})", SetUpAircraft[i].name, SetUpAircraft[i].transform.position, SetUpAircraft[i].transform.localPosition));
            #endregion

            SetUpAircraft[i].name = (string)DeckAircraftIndex[1]; // 함재기 이름
            if ((float)DeckAircraftIndex[2] == 315.0f) // == -45.0f
            {
                //SetUpAircraft[i].transform.rotation = Quaternion.Euler(135.0f, 90.0f, -90.0f); // 추천 방법 2
                SetUpAircraft[i].transform.eulerAngles = new Vector3(135.0f, 90.0f, -90.0f); // 추천 방법 1 : 함재기 각도(하이어라키 창에서만)
                //Debug.Log(SetUpAircraft[i].transform.eulerAngles); // (45.0, 270.0, 90.0)
            }

            else // == 45.0f
            {
                SetUpAircraft[i].transform.eulerAngles = new Vector3((float)DeckAircraftIndex[2], 90.0f, -90.0f); // 추천 방법 1 : 함재기 각도(하이어라키 창에서만)
                //Debug.Log(SetUpAircraft[i].transform.eulerAngles); // (45.0, 90.0, 270.0)
            }
        }
        Invoke("MakeSocket", 0.5f); // 220901 11:24 잠시 주석처리
        //MakeSocket();
    }

    #region SetResources() : 비행갑판 위 필요 자원들 / 나중에 구체화
    //public void SetResources() // 비행갑판 위 자원 관련 함수, 해당되는 위치? 아니면 개수만큼 자원 두기
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

    void MakeSocket() // 소켓 연결은 한번이면 된다! (그러니까 Start()에 써도 될듯?)
    {
        try
        {
            DecksocketConnection.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Ports[1])); // 소켓 연결 끝!
            DecksocketConnection.Blocking = false; // 논-블로킹 구현
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e); // Server가 실행되지 않으면 나오는 에러
        }

        InvokeRepeating("ReceivedFromPython", 0.0f, 0.05f); //  DecksocketConnection.Blocking = false; 덕에 "응답 없음" 발생 X
    }

    void ReceivedFromPython() // Python으로부터 정보를 받는 함수
    {
        //Debug.Log(DecksocketConnection.Connected); // All True -> 문제 없는 듯
        var ReceiveData = new byte[4];
        DecksocketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None); // Updata()나, InvokeRepeating()이나 여기서 멈춤
        Array.Reverse(ReceiveData);
        ReceiveData = new byte[BitConverter.ToInt32(ReceiveData, 0)];
        DecksocketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);

        string ReceivedDataValue = Encoding.UTF8.GetString(ReceiveData);
        Debug.Log("From Python : " + ReceivedDataValue);
        //Debug.Log(string.Format("{0}초, From Python : {1}", Time.time, ReceivedDataValue));
        DataProcessing(ReceivedDataValue);
    }

    void DataProcessing(string fromSimPy) // SimPy에서 넘어온 정보(함재기 이름[0], 작업[1], 소요 시간[2]) 처리 함수
    {
        string[] AircraftName_Process_Time = fromSimPy.Split(',');

        #region 진호 함수 Call
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

        #region 파이썬에서, 작업 소요 시간 전달 -> 가시화 위주 (220830 Coroutine을 사용해야 함)
        else
        {
            float ProcessTime = float.Parse(AircraftName_Process_Time[2]); // 220901 없어도 될 듯..?
            GameObject F35BObject = GameObject.Find(AircraftName_Process_Time[0]);
            #region 게임오브젝트를 setactive(false) -> setactive(true)하는 과정에서 게임오브젝트를 못찾는다면
            if (F35BObject == null) // 내가 찾는게 이전 함수에서 꺼놨으면
            {
                F35BObject = GameObject.Find("GameObject").transform.Find(AircraftName_Process_Time[0]).gameObject;
            }
            #endregion

            if (AircraftName_Process_Time[1] == "TakeOff") // 함재기 이륙
            {
                StartCoroutine(forTakeOff());
                IEnumerator forTakeOff()
                {
                    for (int i = 0; i < 110; i++) // (-80.0f, -9.5f, -30.2f)[이륙 장소]에서 (-300.0f, -9.5f, -60.4f)[상공1]로
                    {
                        F35BObject.transform.position += new Vector3(-2.0f, 0.0f); // 함재기가 위로 떠야하니까(z방향) -30.2f -> -60.4f
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
                        ////yield return new WaitForSeconds(0.05f);
                    }
                    ProcessFinishSignalToPython();
                }
            }

            //220902 해야할 일 : 이륙 준비 및 이륙 직전 함재기가 있으면 착륙 안되게 하기(SimPy에서 해야하지 않나..?)
            else if (AircraftName_Process_Time[1] == "Land") // 함재기 착륙
            {
                F35BObject.SetActive(true);

                StartCoroutine(forLand());
                IEnumerator forLand()
                {
                    for (int i = 0; i < 10; i++) // (40.0f, -9.5f, -60.4f)[상공2]에서 (-41.5f, -9.5f, -30.2f)[착륙 장소]로
                    {
                        F35BObject.transform.position += new Vector3(-8.15f, 0.0f);
                        F35BObject.transform.position += new Vector3(0.0f, 0.0f, 3.02f);

                        yield return new WaitForSeconds(0.05f); // 숫자가 작다는건 그만큼 빨리 for문을 돈다는 것 => 움직임이 자연스러움
                        ////yield return new WaitForSeconds(0.025f); // 숫자가 작다는건 그만큼 빨리 for문을 돈다는 것 => 움직임이 자연스러움
                    }
                    ProcessFinishSignalToPython();
                }
            }

            // 함재기 점검
            // 함재기 급유
        }
        #endregion
    }

    public float InputXAngle(float YAngle) // 진호 함수 입력 값 각도 조정 함수
    {
        float ChangedAngle = 180.0f;

        if (YAngle == 90.0f) // 사용자 입력값 : 45도 -> 나한테 45도
        {
            ChangedAngle = 315.0f; // 진호오빠한테 315도
        }

        else if (YAngle == 270.0f) // 사용자 입력값 : -45도 -> 나한테 135도
        {
            ChangedAngle = 225.0f; // 진호오빠한테 225도
        }

        return ChangedAngle;
    }

    #region 이동 관련 함수들(진호 파일 추가)
    public void ToTakeOff(string AircraftName) // Taxiing
    {
        // 진호 dll -> 꼭! 지역 변수로 설정
        Pathgeneration_Aircraft_V1.pathgeneration MakePath = new Pathgeneration_Aircraft_V1.pathgeneration();
        Pathfollowing_Aircraft_V1.pathfollowing FollowPath = new Pathfollowing_Aircraft_V1.pathfollowing();

        double TransferTime;
        GameObject F35BObject = GameObject.Find(AircraftName);

        var WayPoints = MakePath.path(F35BObject.transform.position, new Vector3(TakeOffSpot.x, TakeOffSpot.y, F35BObject.transform.position.z), InputXAngle(F35BObject.transform.eulerAngles.y), InputXAngle(180.0f));
        var FollowValue = FollowPath.path_following(WayPoints); // FollowValue : 4행으로 이루어져 있음 => 함재기 x,y, x 방향 각도, 시간(마지막 값이 총 소요 시간)

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
            Debug.Log(string.Format("이동 소요 시간 : {0}", TransferTime));

            TransferTimeResultToPython(TransferTime);
        }
    }

    // ServiceA 구역이 5개인데 어디로 가야할지(5개의 구역 중 어디가 비어있는지) 파이썬 말고 유니티에서 조절? -> 최적화를 여기에 적용해야하지않을까
    public void ToAarea(string AircraftName) // TowbarTractor
    {
        // 진호 dll -> 꼭! 지역 변수로 설정
        Pathgeneration_Towbar_V2.pathgeneration MakePath = new Pathgeneration_Towbar_V2.pathgeneration();
        Pathfollowing_Towbar_V5.pathfollowing FollowPath = new Pathfollowing_Towbar_V5.pathfollowing();

        double TransferTime;
        GameObject F35BObject = GameObject.Find(AircraftName);

        //Debug.Log(string.Format("잘된당 히히 : {0}", F35BObject.transform.eulerAngles));

        System.Random random = new System.Random();
        int ChooseAreaIndex = random.Next(TemporalAAreaList.Count);
        string ChooseArea = TemporalAAreaList[ChooseAreaIndex];
        TemporalAAreaList.Remove(ChooseArea);

        var WayPoints = MakePath.path(F35BObject.transform.position, F35BObject.transform.position, 180.0f, 225.0f);
        var FollowValue = FollowPath.path_following(WayPoints); // FollowValue : 10행으로 이루어져 있음 => 트랙터의 x,y,angle / Towbar의 x,y,angle / 함재기의 x,y,angle / 이동 소요 시간

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
            UnityEngine.Debug.Log("On client connect exception " + e); // Server가 실행되지 않으면 나오는 에러
        }
    }

    void ProcessFinishSignalToPython() // 함재기 작업 수행 완료 상태(0.0) 송신
    {
        try
        {
            string ProcessFinishSignal = "0.0";

            // ! Send !
            DecksocketConnection.Send(BitConverter.GetBytes(ProcessFinishSignal.Length)); // Byte[]로 변환한 송신 데이터의 길이
            DecksocketConnection.Send(Encoding.UTF8.GetBytes(ProcessFinishSignal)); // Byte[]로 Encoding한 송신 데이터
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e); // Server가 실행되지 않으면 나오는 에러
        }
    }
}
