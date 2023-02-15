using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Text;
using System.Globalization;
using System.Net.Sockets;
using System.Net;
using Pathgeneration_Aircraft_V1; // 송진호 경로 생성(Taxiing)
using Pathfollowing_Aircraft_V1; // 송진호 경로 추종(Taxiing)
using Pathgeneration_Towbarless_V1; // 송진호 경로 생성(정비구역)
using Pathfollowing_Towbarless_V2; // 송진호 경로 추종(정비구역)
using NFP;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using System.IO;
using SaveDeckData; // 정비구역으로 씬이 변환하기 직전, 갑판에서의 정보들

public class Hangar : MonoBehaviour
{
    #region Hangar 클래스에서 사용할 변수 선언
    FlightDeck_Alone HangarInformationScript;
    ArrayList HangarInformations; // 정비구역 정보(정비구역 내 함재기 상태 정보, 자원, 고장난 함재기 정보)

    List<ArrayList> HangarAircrafts; // 함재기 상태 정보
    ArrayList HangarAircraftIndex; // HangarAircraftIndex[i]에 담긴 정보 : 정비구역 내 위치 / 함재기 이름 / 함재기 각도
    GameObject BrokenAircraft; // 고장나서 수리가 필요한 함재기
    GameObject UpToDeck; // 고장난 함재기 대신, 비행갑판으로 올라갈 함재기

    List<GameObject> SetUpAircraft = new List<GameObject>(); // 사용자로부터 선택된 진짜 함재기들 

    private IEnumerator MoveToD1EV; // 함재기 이동시키는 함수 관련 코루틴

    bool forSelectOrbitalAircraft = false;
    GameObject NowMoveAircraft; // 움직이는 함재기
    GameObject TowbarlessTractor; // 움직이는 트랙터

    List<Vector3> NowMoveAircraft_Transform = new List<Vector3>(); // 움직일 함재기의 초기(배치되자마자) 위치, 각도

    GameObject CollisionTestResultAircraft; //GameObject CollisionTestResultAircraft = new GameObject(); -> 이렇게 하면 에러 발생

    List<GameObject> CollisionCheckList = new List<GameObject>(); // 충돌 검사가 필요한, 움직이는 함재기 이외의 함재기들
    List<GameObject> StackAircraftList = new List<GameObject>(); // 이동하다가 충돌한 함재기들

    public GameObject DrawdotS; // 생성된 NFP 그리려고
    public TextMeshProUGUI OrbitalXAngle; // 목표 함재기 이동하며 바뀌는 각도 가시화

    // List<Tuple<VERTEX, VERTEX>> => item1 : 시작점, item2 : 끝점
    List<Tuple<VERTEX, VERTEX>> NFPResult;
    List<Tuple<VERTEX, VERTEX>> PreNFPResult;

    LineRenderer lr_s; // STOVL 함재기가 장애물일 때, NFP 형상을 Line으로 표현하기 위해, 즉 렌더링을 위한 변수

    int Index_NFP = 0; // 이전 NFP 그림 삭제 함수 관련 변수

    // 소켓 관련 변수
    public Socket HangarsocketConnection;
    List<int> Ports;

    public GameObject D1Elevator;
    public GameObject D2Elevator;

    Vector3 D1EVPosition; // D1 엘리베이터(정비구역->갑판)
    Vector3 D2EVPosition; // D2 엘리베이터(갑판->정비구역)

    public TextAsset Temporal_Text; // *221222 Towbarless Tractor의 이동을 위한 임시적인 좌표 텍스트 파일(Coordinates.txt)
    #endregion

    void Start()
    {
        GameObject Information_Object = GameObject.Find("InfosDelivery").gameObject;
        HangarInformationScript = Information_Object.GetComponent<FlightDeck_Alone>(); // ()를 붙여주는 이유 : 생성자 사용, 아마두..?

        HangarInformations = HangarInformationScript.DeckInformations; 
        HangarAircrafts = (List<ArrayList>)HangarInformations[3]; // 정비구역 함재기 정보(HangarInformations[3])

        Ports = (List<int>)HangarInformations[0]; // 내부 포트 번호(Ports[1]) 사용하려고
        HangarsocketConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // SimPy와 통신하기 위한 소켓

        // 함재기 이동 위치 변수 할당
        D1EVPosition = D1Elevator.transform.position; // new Vector3(29.8f, 20.8f, -1.9f);
        D2EVPosition = D2Elevator.transform.position; // new Vector3(-20.2f, -20.8f, -1.9f);

        SetAircrafts();
        SetResources(); // 정비구역 내 자원 관련 함수, 해당되는 위치? 아니면 개수만큼 자원 두기
        
        CollisionTestResultAircraft = new GameObject(); // *221005 CollisionTestResultAircraft는 꼭 바로 아래 줄 처럼 선언해줘야한다 -> 에러나는 이유는 구현 좀 하고 찾아보기
    }

    public void SetAircrafts()
    {
        for (int i = 0; i < HangarAircrafts.Count; i++)
        {
            HangarAircraftIndex = HangarAircrafts[i];

            SetUpAircraft.Add(GameObject.Find("GameObject").transform.Find((string)HangarAircraftIndex[0]).gameObject);
            GameObject.Find("GameObject").transform.Find((string)HangarAircraftIndex[0]).gameObject.SetActive(true); // 선택된 함재기들 눈에 보이게 해줘
            SetUpAircraft[i].name = (string)HangarAircraftIndex[1]; // 함재기 이름
        }

        if (HangarInformations.Count == 7) // 고장난 함재기가 있으면
        {
            BrokenAircraft = GameObject.Find("GameObject").transform.Find("D2EV").gameObject;
            BrokenAircraft.SetActive(true);
            BrokenAircraft.name = (string)HangarInformations[6];

            SetUpAircraft.Add(BrokenAircraft);
        }
    }

    public void SetResources() // 정비구역 내 자원 관련 함수
    {
        List<int> HangarResources = new List<int>(); // 정비구역 자원 정보([0]:Blue Group/Towbarless Tractor, [1]:Green Group)
        HangarResources = (List<int>)HangarInformations[4];

        int Blue_Towbarless = HangarResources[0];
        for(int i=Blue_Towbarless; i>0; i--)
        {
            GameObject.Find("Resources").transform.Find(string.Format("Towbarless#{0}", i)).gameObject.SetActive(true);
            TowbarlessTractor = GameObject.Find("Resources").transform.Find(string.Format("Towbarless#{0}", i)).gameObject;
        }

        int Green = HangarResources[1];
        for(int i=Green; i>0; i--)
        {
            GameObject.Find("Resources").transform.Find(string.Format("Green#{0}", i)).gameObject.SetActive(true);
        }

        forSelectOrbitalAircraft = true; // 사용자가 움직일 함재기 선택하기 위한 함수 Call
    }

    private void Update() // 이동할 목표 함재기 선택 함수
    {
        if (forSelectOrbitalAircraft)
        {
            // 220920, 함재기를 클릭해달라는 UI 만들기

            if (Input.GetMouseButton(0)) // 사용자가 마우스로 클릭하면
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit HitObject;

                if (Physics.Raycast(ray, out HitObject)) // 마우스 클릭을 통해 목표 함재기를 선택(충돌)하면
                {
                    UpToDeck = HitObject.transform.gameObject; // 선택한 얘를 갑판으로 올릴거야
                    NowMoveAircraft = HitObject.transform.gameObject;
                    Debug.Log(string.Format("이동할 Orbital 함재기 이름 : {0}", NowMoveAircraft.name));

                    forSelectOrbitalAircraft = false; // Update()에 더 이상 못 들어오게 하기 위함

                    CollisionCheckList = SetUpAircraft;
                    CollisionCheckList.Remove(NowMoveAircraft);

                    Invoke("MakeSocket", 0.5f); // 소켓 생성 및 연결
                    MoveTowbarless_usingTextfile(); // MoveTowbarless(); 대신에
                }
            }
        }
    }

    void MakeSocket() // 소켓 연결은 한번이면 된다!
    {
        try
        {
            HangarsocketConnection.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Ports[1])); // 소켓 연결 끝!
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e); // Server가 실행되지 않으면 나오는 에러
        }
    }
    
    void MoveTowbarless_usingTextfile() // 박광필교수님한테 데이터 받으면 수정해야함 재성
    {
        string coordinatesText = Temporal_Text.text.Substring(0, Temporal_Text.text.Length - 1);
        string[] line = coordinatesText.Split('\n');

        StartCoroutine(MoveTowbarlessTractor());
        IEnumerator MoveTowbarlessTractor()
        {
            for (int i = 0; i < line.Length; i++)
            {
                string[] index = line[i].Split('\t');
                TowbarlessTractor.transform.position = new Vector3(float.Parse(index[0]), float.Parse(index[1]), TowbarlessTractor.transform.position.z);
                TowbarlessTractor.transform.rotation = Quaternion.Euler(float.Parse(index[2]), 90.0f, -90.0f);

                yield return new WaitForSeconds(0.1f);
            }
            RecursiveFunc(NowMoveAircraft);
        }
    }
    
    void MoveTowbarless() // 함재기를 이동시키기 위해 Towbarless Tractor가 혼자 움직이는 함수
    {
        // 진호 dll -> 꼭! 지역 변수로 설정
        Pathgeneration_Aircraft_V1.pathgeneration MakePath = new Pathgeneration_Aircraft_V1.pathgeneration();
        Pathfollowing_Aircraft_V1.pathfollowing FollowPath = new Pathfollowing_Aircraft_V1.pathfollowing();

        double TransferTime;

        var WayPoints = MakePath.path(TowbarlessTractor.transform.position, new Vector3(-1.04024f, -1.740236f, -0.5f), AngleAdjustFunc(180.0f), AngleAdjustFunc(225.0f));
        var FollowValue = FollowPath.path_following(WayPoints); // FollowValue : 4행으로 이루어져 있음 => 함재기 x,y, x 방향 각도, 시간(마지막 값이 총 소요 시간)

        StartCoroutine(MoveTowbarlessTractor());
        IEnumerator MoveTowbarlessTractor()
        {
            for (int i = 0; i < FollowValue.GetLength(1); i++) // FollowValue : [4, ????] ->  FollowValue.GetLength(0) = 4 / FollowValue.GetLength(1) = ????
            {
                TowbarlessTractor.transform.position = new Vector3((float)FollowValue[0, i], (float)FollowValue[1, i], TowbarlessTractor.transform.position.z);
                TowbarlessTractor.transform.rotation = Quaternion.Euler(-(float)FollowValue[2, i] * 180.0f / Mathf.PI, 90.0f, -90.0f);

                yield return new WaitForSeconds(0.001f);
            }
            TransferTime = FollowValue[3, FollowValue.GetLength(1) - 1];
            Debug.Log(string.Format("{0}의 이동 소요 시간 : {1}", TowbarlessTractor.name, TransferTime));
            RecursiveFunc(NowMoveAircraft);
            //TransferTimeResultToPython(TransferTime); // 
        }
    }

    void RecursiveFunc(GameObject NextMoveAircraft) // NextAircraft : 지금 당장 충돌한 함재기이자, 이제 움직여야 할 함재기
    {
        NowMoveAircraft = NextMoveAircraft;

        NowMoveAircraft_Transform.Clear();
        NowMoveAircraft_Transform.Add(NowMoveAircraft.transform.position);
        NowMoveAircraft_Transform.Add(NowMoveAircraft.transform.eulerAngles);

        MoveToD1EV = MoveToD1Elevator(NowMoveAircraft);
        StartCoroutine(MoveToD1EV);
    }

    float AngleAdjustFunc(float myAngle) // 입력 인자로 들어가는 각도가 달라서(진호, 민수 오빠) 맞춰주는 함수
    {
        return 360.0f - myAngle;
    }

    public IEnumerator MoveToD1Elevator(GameObject nowMovingAircraft)
    {
        // 진호 dll -> 꼭! 지역 변수로 설정
        Pathgeneration_Towbarless_V1.pathgeneration MakePath = new Pathgeneration_Towbarless_V1.pathgeneration();
        Pathfollowing_Towbarless_V2.pathfollowing FollowPath = new Pathfollowing_Towbarless_V2.pathfollowing();

        double TransferTime; // 함재기 이동 소요 시간

        var WayPoints = MakePath.path(nowMovingAircraft.transform.position, D1EVPosition, AngleAdjustFunc(nowMovingAircraft.transform.eulerAngles.x), AngleAdjustFunc(90.0f));
        var FollowValue = FollowPath.path_following(WayPoints); // FollowValue : 4행으로 이루어져 있음 => 함재기 x,y, x 방향 각도, 시간(마지막 값이 총 소요 시간)

        for (int i = 0; i < FollowValue.Count; i += 300)// *220928 i+=100에서 i+=300으로 잠시 수정
        {
            TowbarlessTractor.transform.position = new Vector3((float)FollowValue[i][0], (float)FollowValue[i][1], TowbarlessTractor.transform.position.z);
            TowbarlessTractor.transform.rotation = Quaternion.Euler((-(float)FollowValue[i][2] * 180.0f / Mathf.PI) - 90.0f, 90.0f, -90.0f);

            nowMovingAircraft.transform.position = new Vector3((float)FollowValue[i][3], (float)FollowValue[i][4], nowMovingAircraft.transform.position.z);
            nowMovingAircraft.transform.rotation = Quaternion.Euler(-(float)FollowValue[i][5] * 180.0f / Mathf.PI, 90.0f, -90.0f);

            CollisionTestResultAircraft = NFPResultFunc(nowMovingAircraft); // NFP 생성 함수
            if (CollisionTestResultAircraft.name != "New Game Object") // 충돌하면!
            {
                StackAircraftList.Add(NowMoveAircraft);

                CollisionCheckList.Add(NowMoveAircraft); 
                CollisionCheckList.Remove(CollisionTestResultAircraft); 

                StopCoroutine(MoveToD1EV);

                NowMoveAircraft.transform.position = NowMoveAircraft_Transform[0];
                NowMoveAircraft.transform.eulerAngles = NowMoveAircraft_Transform[1];
                
                StackAircraftList.Add(CollisionTestResultAircraft);

                if (StackAircraftList.Count > 0)
                {
                    RecursiveFunc(StackAircraftList.Last());
                    StackAircraftList.Remove(StackAircraftList.Last());
                }
            }
            yield return new WaitForSeconds(0.001f);
        }
        // 충돌없이 엘리베이터까지 잘 이동한 경우
        TransferTime = FollowValue[FollowValue.Count - 1][6];
        Debug.Log(string.Format("{0}의 이동 소요 시간 : {1}", nowMovingAircraft.name, TransferTime));
        DeleteNFPDesign();

        if (StackAircraftList.Count > 0) // 장애물 함재기 이동 성공
        {
            TransferResultToPython(NowMoveAircraft.name, TransferTime);
            NowMoveAircraft.SetActive(false);

            CollisionCheckList.Remove(StackAircraftList.Last());

            RecursiveFunc(StackAircraftList.Last());
            StackAircraftList.Remove(StackAircraftList.Last());
        }

        else // StackAircraftList.Count == 0 -> 목표 함재기 이동 성공
        {
            UpToDeck = NowMoveAircraft;
            UptoDeckInfoToPython(NowMoveAircraft.name, TransferTime);

            ReceivedFromPython();
        }
    }

    GameObject NFPResultFunc(GameObject nowMovingAircraft)
    {
        GameObject EmptyResult = new GameObject(); // 충돌이 없으면 빈 오브젝트를 return 하기 위해

        float Orbital_Angle = AngleAdjustFunc(nowMovingAircraft.transform.eulerAngles.x);
        OrbitalXAngle.text = string.Format("[{0}'s Angle]:{1:0.000}", nowMovingAircraft.name, nowMovingAircraft.transform.eulerAngles.x);

        List<ArrayList> List_AboutNFPResults = new List<ArrayList>();

        for (int i = 0; i < CollisionCheckList.Count; i++)
        {
            ArrayList CollisionObjects_NFPResults = new ArrayList();

            CollisionObjects_NFPResults.Add(CollisionCheckList[i]);
            CollisionObjects_NFPResults.Add(MakeNFP.GetSTOVL(AngleAdjustFunc(CollisionCheckList[i].transform.eulerAngles.x), Orbital_Angle, new VERTEX(CollisionCheckList[i].transform.position.x, CollisionCheckList[i].transform.position.y)));

            List_AboutNFPResults.Add(CollisionObjects_NFPResults);
        }

        DeleteNFPDesign();

        for (int i = 0; i < List_AboutNFPResults.Count; i++)
        {
            ArrayList IndexofList_AboutNFPResults = List_AboutNFPResults[i]; // {GameObject, List<Tuple<VERTEX, VERTEX>>}

            NFPDesign((List<Tuple<VERTEX, VERTEX>>)IndexofList_AboutNFPResults[1]); // (List<Tuple<VERTEX, VERTEX>>)a[1]); // 생성된 NFP 인수로 보내야 함

            float OrbitalposX = nowMovingAircraft.transform.position.x + 8.0f;
            float OrbitalposY = nowMovingAircraft.transform.position.y;
            bool CollisionCheck = IsInside(new VERTEX(OrbitalposX, OrbitalposY), (List<Tuple<VERTEX, VERTEX>>)IndexofList_AboutNFPResults[1]);
            if (CollisionCheck) // CollisionCheck = true -> 충돌 발생
            {
                return (GameObject)IndexofList_AboutNFPResults[0];
            }
        }

        return EmptyResult; // 빈 오브젝트 return, 이름은 "New Game Object"
    }

    void DeleteNFPDesign()
    {
        GameObject.Find("DrawingSphereS").tag = "Untagged";
        if (Index_NFP > 0)
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
    }

    void NFPDesign(List<Tuple<VERTEX, VERTEX>> CreatedNFP)
    {
        if (CreatedNFP != null)
        {
            for (int i = 0; i < CreatedNFP.Count - 1; i++)
            {
                // Item1 = 시작점, Item2 = 끝점
                var ob = Instantiate(DrawdotS, new Vector3((float)CreatedNFP[i].Item1.X, (float)CreatedNFP[i].Item1.Y, -1.0f), Quaternion.identity);
                ob.AddComponent<LineRenderer>();

                lr_s = ob.GetComponent<LineRenderer>();
                lr_s.widthMultiplier = 0.5f; //선 너비

                lr_s.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply")); // Find("") "" 부분이 없으면 Defualt인 분홍색이 그려짐
                lr_s.startColor = Color.red;
                lr_s.endColor = Color.red;

                var cube1Pos = new Vector3((float)CreatedNFP[i].Item1.X, (float)CreatedNFP[i].Item1.Y, -1.0f);
                var cube2Pos = new Vector3((float)CreatedNFP[i].Item2.X, (float)CreatedNFP[i].Item2.Y, -1.0f);
                lr_s.SetPosition(0, cube1Pos);
                lr_s.SetPosition(1, cube2Pos);
            }
        }
        Index_NFP++;
    }

    bool IsInside(VERTEX B, List<Tuple<VERTEX, VERTEX>> tuples) // 목표 함재기의 기준점이 NFP 안으로 들어오면 True를 return
    {
        int crosses = 0; //crosses는 점q와 오른쪽 반직선과 다각형과의 교점의 개수
        for (int i = 0; i < tuples.Count(); i++)
        {
            int j = (i + 1) % tuples.Count();
            if ((tuples[i].Item1.Y > B.Y) != (tuples[j].Item1.Y > B.Y)) //점 B가 선분 (p[i], p[j])의 y좌표 사이에 있음
            {
                //atX는 점 B를 지나는 수평선과 선분 (p[i], p[j])의 교점   
                double atX = (tuples[j].Item1.X - tuples[i].Item1.X) * (B.Y - tuples[i].Item1.Y) / (tuples[j].Item1.Y - tuples[i].Item1.Y) + tuples[i].Item1.X;
                //atX가 오른쪽 반직선과의 교점이 맞으면 교점의 개수를 증가시킨다.    
                if (B.X < atX) crosses++;
                else if (B.X == atX) return false;  //B가 직선의 Y범주안에 있으면서 우방향 반직선과 직선의 교차점의 x값과 B.x가 같다면 b는 직선위의 점이다. -> 안에 있는 점이 아님
            }
        }
        return crosses % 2 > 0; // true : 정점이 polygon 안에 있음 -> 충돌이다 이거에요
    }

    void TransferResultToPython(string AircraftName, double TransferTime) // 장애물 함재기 이동 소요 시간 송신
    {
        string ToData = AircraftName + ',' + TransferTime;

        try
        {
            // ! Send !
            HangarsocketConnection.Send(Encoding.UTF8.GetBytes(ToData));
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e);
        }
    }

    void UptoDeckInfoToPython(string AircraftName, double TransferTime) // 목표 함재기 이동 소요 시간 송신
    {
        string ToData = AircraftName + ',' + TransferTime + ',' + "Finished";

        try
        {
            // ! Send !
            HangarsocketConnection.Send(Encoding.UTF8.GetBytes(ToData));
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e);
        }
    }

    void ReceivedFromPython() // Python으로부터 정보를 받는 함수
    {
        var ReceiveData = new byte[4];
        HangarsocketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);
        Array.Reverse(ReceiveData);
        ReceiveData = new byte[BitConverter.ToInt32(ReceiveData, 0)];
        HangarsocketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);

        string ReceivedDataValue = Encoding.UTF8.GetString(ReceiveData);
        Debug.Log("[Hangar Scene] From Python : " + ReceivedDataValue);

        if (ReceivedDataValue == "GotoDeck")
        {
            HangarsocketConnection.Close();
            SceneManager.LoadScene("FlightDeck_Alone");
        }
    }
}