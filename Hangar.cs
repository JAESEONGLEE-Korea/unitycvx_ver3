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
using Pathgeneration_Aircraft_V1; // ����ȣ ��� ����(Taxiing)
using Pathfollowing_Aircraft_V1; // ����ȣ ��� ����(Taxiing)
using Pathgeneration_Towbarless_V1; // ����ȣ ��� ����(���񱸿�)
using Pathfollowing_Towbarless_V2; // ����ȣ ��� ����(���񱸿�)
using NFP;
using UnityEngine.UI;
using TMPro;
using System.Threading;
using System.IO;
using SaveDeckData; // ���񱸿����� ���� ��ȯ�ϱ� ����, ���ǿ����� ������

public class Hangar : MonoBehaviour
{
    #region Hangar Ŭ�������� ����� ���� ����
    FlightDeck_Alone HangarInformationScript;
    ArrayList HangarInformations; // ���񱸿� ����(���񱸿� �� ����� ���� ����, �ڿ�, ���峭 ����� ����)

    List<ArrayList> HangarAircrafts; // ����� ���� ����
    ArrayList HangarAircraftIndex; // HangarAircraftIndex[i]�� ��� ���� : ���񱸿� �� ��ġ / ����� �̸� / ����� ����
    GameObject BrokenAircraft; // ���峪�� ������ �ʿ��� �����
    GameObject UpToDeck; // ���峭 ����� ���, ���఩������ �ö� �����

    List<GameObject> SetUpAircraft = new List<GameObject>(); // ����ڷκ��� ���õ� ��¥ ������ 

    private IEnumerator MoveToD1EV; // ����� �̵���Ű�� �Լ� ���� �ڷ�ƾ

    bool forSelectOrbitalAircraft = false;
    GameObject NowMoveAircraft; // �����̴� �����
    GameObject TowbarlessTractor; // �����̴� Ʈ����

    List<Vector3> NowMoveAircraft_Transform = new List<Vector3>(); // ������ ������� �ʱ�(��ġ���ڸ���) ��ġ, ����

    GameObject CollisionTestResultAircraft; //GameObject CollisionTestResultAircraft = new GameObject(); -> �̷��� �ϸ� ���� �߻�

    List<GameObject> CollisionCheckList = new List<GameObject>(); // �浹 �˻簡 �ʿ���, �����̴� ����� �̿��� ������
    List<GameObject> StackAircraftList = new List<GameObject>(); // �̵��ϴٰ� �浹�� ������

    public GameObject DrawdotS; // ������ NFP �׸�����
    public TextMeshProUGUI OrbitalXAngle; // ��ǥ ����� �̵��ϸ� �ٲ�� ���� ����ȭ

    // List<Tuple<VERTEX, VERTEX>> => item1 : ������, item2 : ����
    List<Tuple<VERTEX, VERTEX>> NFPResult;
    List<Tuple<VERTEX, VERTEX>> PreNFPResult;

    LineRenderer lr_s; // STOVL ����Ⱑ ��ֹ��� ��, NFP ������ Line���� ǥ���ϱ� ����, �� �������� ���� ����

    int Index_NFP = 0; // ���� NFP �׸� ���� �Լ� ���� ����

    // ���� ���� ����
    public Socket HangarsocketConnection;
    List<int> Ports;

    public GameObject D1Elevator;
    public GameObject D2Elevator;

    Vector3 D1EVPosition; // D1 ����������(���񱸿�->����)
    Vector3 D2EVPosition; // D2 ����������(����->���񱸿�)

    public TextAsset Temporal_Text; // *221222 Towbarless Tractor�� �̵��� ���� �ӽ����� ��ǥ �ؽ�Ʈ ����(Coordinates.txt)
    #endregion

    void Start()
    {
        GameObject Information_Object = GameObject.Find("InfosDelivery").gameObject;
        HangarInformationScript = Information_Object.GetComponent<FlightDeck_Alone>(); // ()�� �ٿ��ִ� ���� : ������ ���, �Ƹ���..?

        HangarInformations = HangarInformationScript.DeckInformations; 
        HangarAircrafts = (List<ArrayList>)HangarInformations[3]; // ���񱸿� ����� ����(HangarInformations[3])

        Ports = (List<int>)HangarInformations[0]; // ���� ��Ʈ ��ȣ(Ports[1]) ����Ϸ���
        HangarsocketConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp); // SimPy�� ����ϱ� ���� ����

        // ����� �̵� ��ġ ���� �Ҵ�
        D1EVPosition = D1Elevator.transform.position; // new Vector3(29.8f, 20.8f, -1.9f);
        D2EVPosition = D2Elevator.transform.position; // new Vector3(-20.2f, -20.8f, -1.9f);

        SetAircrafts();
        SetResources(); // ���񱸿� �� �ڿ� ���� �Լ�, �ش�Ǵ� ��ġ? �ƴϸ� ������ŭ �ڿ� �α�
        
        CollisionTestResultAircraft = new GameObject(); // *221005 CollisionTestResultAircraft�� �� �ٷ� �Ʒ� �� ó�� ����������Ѵ� -> �������� ������ ���� �� �ϰ� ã�ƺ���
    }

    public void SetAircrafts()
    {
        for (int i = 0; i < HangarAircrafts.Count; i++)
        {
            HangarAircraftIndex = HangarAircrafts[i];

            SetUpAircraft.Add(GameObject.Find("GameObject").transform.Find((string)HangarAircraftIndex[0]).gameObject);
            GameObject.Find("GameObject").transform.Find((string)HangarAircraftIndex[0]).gameObject.SetActive(true); // ���õ� ������ ���� ���̰� ����
            SetUpAircraft[i].name = (string)HangarAircraftIndex[1]; // ����� �̸�
        }

        if (HangarInformations.Count == 7) // ���峭 ����Ⱑ ������
        {
            BrokenAircraft = GameObject.Find("GameObject").transform.Find("D2EV").gameObject;
            BrokenAircraft.SetActive(true);
            BrokenAircraft.name = (string)HangarInformations[6];

            SetUpAircraft.Add(BrokenAircraft);
        }
    }

    public void SetResources() // ���񱸿� �� �ڿ� ���� �Լ�
    {
        List<int> HangarResources = new List<int>(); // ���񱸿� �ڿ� ����([0]:Blue Group/Towbarless Tractor, [1]:Green Group)
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

        forSelectOrbitalAircraft = true; // ����ڰ� ������ ����� �����ϱ� ���� �Լ� Call
    }

    private void Update() // �̵��� ��ǥ ����� ���� �Լ�
    {
        if (forSelectOrbitalAircraft)
        {
            // 220920, ����⸦ Ŭ���ش޶�� UI �����

            if (Input.GetMouseButton(0)) // ����ڰ� ���콺�� Ŭ���ϸ�
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit HitObject;

                if (Physics.Raycast(ray, out HitObject)) // ���콺 Ŭ���� ���� ��ǥ ����⸦ ����(�浹)�ϸ�
                {
                    UpToDeck = HitObject.transform.gameObject; // ������ �긦 �������� �ø��ž�
                    NowMoveAircraft = HitObject.transform.gameObject;
                    Debug.Log(string.Format("�̵��� Orbital ����� �̸� : {0}", NowMoveAircraft.name));

                    forSelectOrbitalAircraft = false; // Update()�� �� �̻� �� ������ �ϱ� ����

                    CollisionCheckList = SetUpAircraft;
                    CollisionCheckList.Remove(NowMoveAircraft);

                    Invoke("MakeSocket", 0.5f); // ���� ���� �� ����
                    MoveTowbarless_usingTextfile(); // MoveTowbarless(); ��ſ�
                }
            }
        }
    }

    void MakeSocket() // ���� ������ �ѹ��̸� �ȴ�!
    {
        try
        {
            HangarsocketConnection.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Ports[1])); // ���� ���� ��!
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e); // Server�� ������� ������ ������ ����
        }
    }
    
    void MoveTowbarless_usingTextfile() // �ڱ��ʱ��������� ������ ������ �����ؾ��� �缺
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
    
    void MoveTowbarless() // ����⸦ �̵���Ű�� ���� Towbarless Tractor�� ȥ�� �����̴� �Լ�
    {
        // ��ȣ dll -> ��! ���� ������ ����
        Pathgeneration_Aircraft_V1.pathgeneration MakePath = new Pathgeneration_Aircraft_V1.pathgeneration();
        Pathfollowing_Aircraft_V1.pathfollowing FollowPath = new Pathfollowing_Aircraft_V1.pathfollowing();

        double TransferTime;

        var WayPoints = MakePath.path(TowbarlessTractor.transform.position, new Vector3(-1.04024f, -1.740236f, -0.5f), AngleAdjustFunc(180.0f), AngleAdjustFunc(225.0f));
        var FollowValue = FollowPath.path_following(WayPoints); // FollowValue : 4������ �̷���� ���� => ����� x,y, x ���� ����, �ð�(������ ���� �� �ҿ� �ð�)

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
            Debug.Log(string.Format("{0}�� �̵� �ҿ� �ð� : {1}", TowbarlessTractor.name, TransferTime));
            RecursiveFunc(NowMoveAircraft);
            //TransferTimeResultToPython(TransferTime); // 
        }
    }

    void RecursiveFunc(GameObject NextMoveAircraft) // NextAircraft : ���� ���� �浹�� ���������, ���� �������� �� �����
    {
        NowMoveAircraft = NextMoveAircraft;

        NowMoveAircraft_Transform.Clear();
        NowMoveAircraft_Transform.Add(NowMoveAircraft.transform.position);
        NowMoveAircraft_Transform.Add(NowMoveAircraft.transform.eulerAngles);

        MoveToD1EV = MoveToD1Elevator(NowMoveAircraft);
        StartCoroutine(MoveToD1EV);
    }

    float AngleAdjustFunc(float myAngle) // �Է� ���ڷ� ���� ������ �޶�(��ȣ, �μ� ����) �����ִ� �Լ�
    {
        return 360.0f - myAngle;
    }

    public IEnumerator MoveToD1Elevator(GameObject nowMovingAircraft)
    {
        // ��ȣ dll -> ��! ���� ������ ����
        Pathgeneration_Towbarless_V1.pathgeneration MakePath = new Pathgeneration_Towbarless_V1.pathgeneration();
        Pathfollowing_Towbarless_V2.pathfollowing FollowPath = new Pathfollowing_Towbarless_V2.pathfollowing();

        double TransferTime; // ����� �̵� �ҿ� �ð�

        var WayPoints = MakePath.path(nowMovingAircraft.transform.position, D1EVPosition, AngleAdjustFunc(nowMovingAircraft.transform.eulerAngles.x), AngleAdjustFunc(90.0f));
        var FollowValue = FollowPath.path_following(WayPoints); // FollowValue : 4������ �̷���� ���� => ����� x,y, x ���� ����, �ð�(������ ���� �� �ҿ� �ð�)

        for (int i = 0; i < FollowValue.Count; i += 300)// *220928 i+=100���� i+=300���� ��� ����
        {
            TowbarlessTractor.transform.position = new Vector3((float)FollowValue[i][0], (float)FollowValue[i][1], TowbarlessTractor.transform.position.z);
            TowbarlessTractor.transform.rotation = Quaternion.Euler((-(float)FollowValue[i][2] * 180.0f / Mathf.PI) - 90.0f, 90.0f, -90.0f);

            nowMovingAircraft.transform.position = new Vector3((float)FollowValue[i][3], (float)FollowValue[i][4], nowMovingAircraft.transform.position.z);
            nowMovingAircraft.transform.rotation = Quaternion.Euler(-(float)FollowValue[i][5] * 180.0f / Mathf.PI, 90.0f, -90.0f);

            CollisionTestResultAircraft = NFPResultFunc(nowMovingAircraft); // NFP ���� �Լ�
            if (CollisionTestResultAircraft.name != "New Game Object") // �浹�ϸ�!
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
        // �浹���� ���������ͱ��� �� �̵��� ���
        TransferTime = FollowValue[FollowValue.Count - 1][6];
        Debug.Log(string.Format("{0}�� �̵� �ҿ� �ð� : {1}", nowMovingAircraft.name, TransferTime));
        DeleteNFPDesign();

        if (StackAircraftList.Count > 0) // ��ֹ� ����� �̵� ����
        {
            TransferResultToPython(NowMoveAircraft.name, TransferTime);
            NowMoveAircraft.SetActive(false);

            CollisionCheckList.Remove(StackAircraftList.Last());

            RecursiveFunc(StackAircraftList.Last());
            StackAircraftList.Remove(StackAircraftList.Last());
        }

        else // StackAircraftList.Count == 0 -> ��ǥ ����� �̵� ����
        {
            UpToDeck = NowMoveAircraft;
            UptoDeckInfoToPython(NowMoveAircraft.name, TransferTime);

            ReceivedFromPython();
        }
    }

    GameObject NFPResultFunc(GameObject nowMovingAircraft)
    {
        GameObject EmptyResult = new GameObject(); // �浹�� ������ �� ������Ʈ�� return �ϱ� ����

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

            NFPDesign((List<Tuple<VERTEX, VERTEX>>)IndexofList_AboutNFPResults[1]); // (List<Tuple<VERTEX, VERTEX>>)a[1]); // ������ NFP �μ��� ������ ��

            float OrbitalposX = nowMovingAircraft.transform.position.x + 8.0f;
            float OrbitalposY = nowMovingAircraft.transform.position.y;
            bool CollisionCheck = IsInside(new VERTEX(OrbitalposX, OrbitalposY), (List<Tuple<VERTEX, VERTEX>>)IndexofList_AboutNFPResults[1]);
            if (CollisionCheck) // CollisionCheck = true -> �浹 �߻�
            {
                return (GameObject)IndexofList_AboutNFPResults[0];
            }
        }

        return EmptyResult; // �� ������Ʈ return, �̸��� "New Game Object"
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
                // Item1 = ������, Item2 = ����
                var ob = Instantiate(DrawdotS, new Vector3((float)CreatedNFP[i].Item1.X, (float)CreatedNFP[i].Item1.Y, -1.0f), Quaternion.identity);
                ob.AddComponent<LineRenderer>();

                lr_s = ob.GetComponent<LineRenderer>();
                lr_s.widthMultiplier = 0.5f; //�� �ʺ�

                lr_s.material = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply")); // Find("") "" �κ��� ������ Defualt�� ��ȫ���� �׷���
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

    bool IsInside(VERTEX B, List<Tuple<VERTEX, VERTEX>> tuples) // ��ǥ ������� �������� NFP ������ ������ True�� return
    {
        int crosses = 0; //crosses�� ��q�� ������ �������� �ٰ������� ������ ����
        for (int i = 0; i < tuples.Count(); i++)
        {
            int j = (i + 1) % tuples.Count();
            if ((tuples[i].Item1.Y > B.Y) != (tuples[j].Item1.Y > B.Y)) //�� B�� ���� (p[i], p[j])�� y��ǥ ���̿� ����
            {
                //atX�� �� B�� ������ ���򼱰� ���� (p[i], p[j])�� ����   
                double atX = (tuples[j].Item1.X - tuples[i].Item1.X) * (B.Y - tuples[i].Item1.Y) / (tuples[j].Item1.Y - tuples[i].Item1.Y) + tuples[i].Item1.X;
                //atX�� ������ ���������� ������ ������ ������ ������ ������Ų��.    
                if (B.X < atX) crosses++;
                else if (B.X == atX) return false;  //B�� ������ Y���־ȿ� �����鼭 ����� �������� ������ �������� x���� B.x�� ���ٸ� b�� �������� ���̴�. -> �ȿ� �ִ� ���� �ƴ�
            }
        }
        return crosses % 2 > 0; // true : ������ polygon �ȿ� ���� -> �浹�̴� �̰ſ���
    }

    void TransferResultToPython(string AircraftName, double TransferTime) // ��ֹ� ����� �̵� �ҿ� �ð� �۽�
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

    void UptoDeckInfoToPython(string AircraftName, double TransferTime) // ��ǥ ����� �̵� �ҿ� �ð� �۽�
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

    void ReceivedFromPython() // Python���κ��� ������ �޴� �Լ�
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