using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Net.Sockets;
using System.Text;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Net;
using System.Diagnostics;
using System.Threading;

public class Communication : MonoBehaviour // [단독 + 통합 모드] 구역 별 사용자 입력 정보 저장 및 소켓 통신을 통한 전달 
{
    // 이전에 쌓아놨던 정보들(Deck 씬과 Hangar 씬 정보 사용하려고 가져옴)
    GameObject DatasObject;
    DataManagement AllDatas;
    ServiceAreaManagement AircraftsLocationOnDeck; // 비행갑판 위 함재기 정보들을 위해
    HangarAreaManagement AircraftsLocationOnHangar; // 정비구역 내 함재기 정보들을 위해

    // 이제 다음 씬을 위해 담아놓은 정보들을 위한 변수 생성
    public GameObject FinishedInformations; // 씬을 바꿔도 정보를 살려서 가져가려면 DontDestroyOnLoad(GameObject)를 사용해야해서 GameObject 형태

    // "Informations"에 담긴 정보들
    List<int> Ports = new List<int>();
    List<ArrayList> Deck_Aircraft = new List<ArrayList>();
    List<int> Deck_Resource = new List<int>(); // ★ TowbarTractor, Red, Brown, Purple, Green 자원 정보순으로 담기(int_List말고 Dict으로 바꾸까)
    List<ArrayList> Hangar_Aircraft = new List<ArrayList>();
    List<int> Hangar_Resource = new List<int>(); // ★ TowbarlessTractor, Green 자원 정보순으로 담기(int_List말고 Dict으로 바꾸까)
    List<double> Elevator_Velos = new List<double>(); // (double_List말고 Dict으로 바꾸까)
    ArrayList Informations = new ArrayList(); // 다음 씬으로 넘겨줄 정보들

    // 소켓 관련 변수들
    public Socket socketConnection;
    public GameObject SimulationButton; // Server에서 데이터 받았다는 신호가 들어오면 시뮬레이션 버튼이 등장해야 하기 때문에

    void Start()
    {
        DatasObject = GameObject.Find("DataManagement");
        AllDatas = DatasObject.GetComponent<DataManagement>(); // "DataManagement"라는 게임오브젝트에 있는 "DataManagement" 스크립트를 사용할거다!

        AircraftsLocationOnDeck = AllDatas.Area_management;
        AircraftsLocationOnHangar = AllDatas.HangarArea_management;

        PortNumber();
        ServiceArea();
        DeckResources();
        HangarArea();
        HangarResources();
        Elevator();

        socketConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    }

    #region 사용자 입력 정보 저장
    public void PortNumber()
    {
        Ports.Add(AllDatas.External_PortNumber); // 외부 포트 번호
        Ports.Add(AllDatas.Internal_PortNumber); // 내부 포트 번호

        Informations.Add(Ports);
    }

    public void ServiceArea()
    {
        for (int i = 0; i < 10; i++) // 비행갑판의 서비스 구역은 10개
        {
            if (AircraftsLocationOnDeck.ServiceArea_State[i] != null) // 어떠한 정보를 가진 요소들만
            {
                ArrayList Deck_Aircraft_Index = new ArrayList(); // 들어올 때 마다 초기화된까

                Deck_Aircraft_Index.Add(AircraftsLocationOnDeck.ServiceArea_State[i].AREA_NAME); // 서비스 구역 이름
                Deck_Aircraft_Index.Add(AircraftsLocationOnDeck.ServiceArea_State[i].AIRCRAFT_NAME); // 함재기 이름
                Deck_Aircraft_Index.Add(AircraftsLocationOnDeck.ServiceArea_State[i].AIRCRAFT_POSTURE); // 함재기가 놓인 자세

                Deck_Aircraft.Add(Deck_Aircraft_Index);
            }
        }
        Informations.Add(Deck_Aircraft);
    }

    public void DeckResources()
    {
        Deck_Resource.Add(AllDatas.Tractors);
        Deck_Resource.Add(AllDatas.RedGroup);
        Deck_Resource.Add(AllDatas.BrownGroup);
        Deck_Resource.Add(AllDatas.PurpleGroup);
        Deck_Resource.Add(AllDatas.GreenGroup);

        Informations.Add(Deck_Resource); // Informations의 Informations[1] 완성
    }

    public void HangarArea()
    {
        for (int i = 0; i < 10; i++) // 정비 구역 내 주차 가능 구역은 10개
        {
            if (AircraftsLocationOnHangar.HangarArea_State[i] != null) // 어떠한 정보를 가진 요소들만
            {
                ArrayList Hangar_Aircraft_Index = new ArrayList();

                Hangar_Aircraft_Index.Add(AircraftsLocationOnHangar.HangarArea_State[i].AREA_NAME); // 정비 구역 이름
                Hangar_Aircraft_Index.Add(AircraftsLocationOnHangar.HangarArea_State[i].AIRCRAFT_NAME); // 함재기 이름

                Hangar_Aircraft.Add(Hangar_Aircraft_Index);
            }
        }
        Informations.Add(Hangar_Aircraft);
    }

    public void HangarResources()
    {
        Hangar_Resource.Add(AllDatas.Hangar_Trators);
        Hangar_Resource.Add(AllDatas.Hangar_GreenGroup);

        Informations.Add(Hangar_Resource);
    }

    public void Elevator()
    {
        Elevator_Velos.Add(AllDatas.D1_Velocity);
        Elevator_Velos.Add(AllDatas.D2_Velocity);

        Informations.Add(Elevator_Velos);

        SaveInfosText();
    }

    void SaveInfosText() // 사용자 입력 정보를 .txt 파일로 저장
    {
        // 저장되는 파일의 이름과 저장 장소 바꿔주기
        FileStream InfosText = new FileStream("C:\\master0\\navalized aircraft\\Unity\\CVX_ver3\\InfosText.txt", FileMode.Create);

        StreamWriter testStreamWriter = new StreamWriter(InfosText);

        // 외/내부 포트 번호
        testStreamWriter.Write(string.Format("External_PortNumber {0}", Ports[0] + "\r\n"));
        testStreamWriter.Write(string.Format("Internal_PortNumber {0}", Ports[1] + "\r\n"));

        // 비행갑판 구역 함재기 정보
        for (int i = 0; i < Deck_Aircraft.Count; i++)
        {
            testStreamWriter.Write(string.Format("{0} {1} {2}", Deck_Aircraft[i][0], Deck_Aircraft[i][1], Deck_Aircraft[i][2] + "\r\n"));
            //testStreamWriter.Write('\n');
        }

        // 비행갑판 자원
        testStreamWriter.Write(string.Format("TowbarTractor {0}", Deck_Resource[0] + "\r\n"));
        testStreamWriter.Write(string.Format("RedGroup {0}", Deck_Resource[1] + "\r\n"));
        testStreamWriter.Write(string.Format("BrownGroup {0}", Deck_Resource[2] + "\r\n"));
        testStreamWriter.Write(string.Format("PurpleGroup {0}", Deck_Resource[3] + "\r\n"));
        testStreamWriter.Write(string.Format("GreenGroup {0}", Deck_Resource[4] + "\r\n"));

        // 정비구역 함재기 정보
        for (int i = 0; i < Hangar_Aircraft.Count; i++)
        {
            testStreamWriter.Write(string.Format("{0} {1}", Hangar_Aircraft[i][0], Hangar_Aircraft[i][1] + "\r\n"));
        }

        // 정비구역 자원
        testStreamWriter.Write(string.Format("TowbarlessTractor {0}", Hangar_Resource[0] + "\r\n"));
        testStreamWriter.Write(string.Format("Hangar_GreenGroup {0}", Hangar_Resource[1] + "\r\n"));

        // 엘리베이터들 속도
        for (int i = 0; i < Elevator_Velos.Count; i++)
        {
            testStreamWriter.Write(string.Format("D{0}_EV {1}", i+1, Elevator_Velos[i] + "\r\n"));
        }

        #region 이것도 나쁘지 않네
        /*
        // 포트 번호
        for (int i = 0; i < Ports.Count; i++)
        {
            testStreamWriter.Write(string.Format("External_PortNumber {0}", Ports[i])); //switch 문을 쓸까 너무 낭비인디
            testStreamWriter.Write('\n');
        }

        // Deck_Resource
        testStreamWriter.Write(string.Format("비행갑판 위 Blue 및 Towbar Tractor 개수 : {0}개", Deck_Resource[0] + '\n'));
        testStreamWriter.Write(string.Format("비행갑판 위 Red 인원 : {0}명", Deck_Resource[1] + '\n'));
        testStreamWriter.Write(string.Format("비행갑판 위 Brown 인원 : {0}명", Deck_Resource[2] + '\n'));
        testStreamWriter.Write(string.Format("비행갑판 위 Purple 인원 : {0}명", Deck_Resource[3] + '\n'));
        testStreamWriter.Write(string.Format("비행갑판 위 Green 인원 : {0}명", Deck_Resource[4] + '\n'));

        // Hangar_Resource
        testStreamWriter.Write(string.Format("정비구역 내 Blue 및 Towbarless Tractor 개수 : {0}개", Hangar_Resource[0] + '\n'));
        testStreamWriter.Write(string.Format("정비구역 내 Green 인원 : {0}명", Hangar_Resource[1] + '\n'));
        */
        #endregion

        testStreamWriter.Close();
    }

    public ArrayList MakeInfomations()
    {
        return Informations; // 사용자가 입력한, '모든' 정보를 담고 있는 List
    }
    #endregion

    #region 단독 모드용
    public void Alone_SocketButton() // 단독 모드[Alone_Socket 씬] 비행갑판 버튼 누르면~
    {
        // [221031] 연동 확인하면, SimPy 경로 정리하기
        var Path = @"C:\master0\navalized aircraft\Unity\CVX_ver3\SimPy_forCVX"; // 파일이 놓여진 경로
        var PythonFilename = "DabinReal.py"; // 파일 이름 var PythonFilename

        // *221013 아래 두줄 주석 처리
        //Thread RuntheServer = new Thread(() => RunPython(Path, PythonFilename));
        //RuntheServer.Start();

        Alone_ConnectTcpServer(); // 켜진 Server(Python)에 정보를 주고 받고 기능

        // 나중에, 만약 사용자가 입력 값을 넣지 않으면 어떤 값 들어오는지 확인, 에러 발생시키기
    }

    void Alone_ConnectTcpServer()
    {
        try
        {
            using (socketConnection)
            {
                socketConnection.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Ports[1]));

                #region ! 사용자가 입력한 모든 정보를 파이썬(SimPy)으로 Send !
                for (int i = 0; i < Deck_Aircraft.Count; i++) // 비행갑판 위 함재기 정보
                {
                    socketConnection.Send(BitConverter.GetBytes(InfosToByte(Deck_Aircraft[i][1], "@").Length));
                    socketConnection.Send(InfosToByte(Deck_Aircraft[i][1], "@")); // 함재기 이름만 Send
                }

                for (int i = 0; i < Deck_Resource.Count; i++) // 비행갑판 위 자원 정보
                {
                    socketConnection.Send(BitConverter.GetBytes(InfosToByte(Deck_Resource[i], "$").Length));
                    socketConnection.Send(InfosToByte(Deck_Resource[i], "$"));
                }

                for (int i = 0; i < Hangar_Aircraft.Count; i++) // 정비구역 위 함재기 정보
                {
                    socketConnection.Send(BitConverter.GetBytes(InfosToByte(Hangar_Aircraft[i][1], "%").Length));
                    socketConnection.Send(InfosToByte(Hangar_Aircraft[i][1], "%")); // 함재기 이름만 보낼거
                }

                for (int i = 0; i < Hangar_Resource.Count; i++) // 정비구역 위 자원 정보
                {
                    socketConnection.Send(BitConverter.GetBytes(InfosToByte(Hangar_Resource[i], "&").Length));
                    socketConnection.Send(InfosToByte(Hangar_Resource[i], "&"));
                }

                for (int i = 0; i < Elevator_Velos.Count; i++) // 엘리베이터 정보
                {
                    socketConnection.Send(BitConverter.GetBytes(InfosToByte(Elevator_Velos[i], "^").Length));
                    socketConnection.Send(InfosToByte(Elevator_Velos[i], "^"));
                }
                UnityEngine.Debug.Log("함재기 정보 모두 송신 완료!");
                #endregion ! 사용자가 입력한 모든 정보를 파이썬(SimPy)으로 Send !

                #region ! 파이썬으로부터 신호를 Receive !
                var ReceiveData = new byte[4]; // 크기 할당은 그저 그릇을 만들어 준다는 의미, 서버에서 데이터가 들어오면 해당 데이터 크기만큼 생김
                socketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);
                Array.Reverse(ReceiveData);
                ReceiveData = new byte[BitConverter.ToInt32(ReceiveData, 0)];
                socketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);

                string ReceivedDataValue = Encoding.UTF8.GetString(ReceiveData); // Server로부터 받은 데이터 스트링으로
                UnityEngine.Debug.Log("From Python First : " + ReceivedDataValue);

                if (ReceivedDataValue == "Success")
                {
                    SimulationButton.SetActive(true); // .SetActive() : Main Thread에서만 부를수 있대
                    Invoke("ToFlightDeck_Alone", 2); // 2초 뒤에, 비행갑판 시뮬레이션 씬으로 이동하는 함수(ToSimulationScene()) 부르기
                }
                #endregion ! 파이썬으로부터 신호를 Receive !
            }
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e); // Server가 실행되지 않으면 나오는 에러
        }
    }

    public void ToFlightDeck_Alone()
    {
        SceneManager.LoadScene("FlightDeck_Alone");
        DontDestroyOnLoad(FinishedInformations);

        socketConnection.Close(); // *221028 추가
    }
    #endregion

    #region 통합 모드용
    public void Inte_DeckSocketButton() // 통합 모드[Inte_Socket 씬] 비행갑판 버튼 누르면~
    {
        var Path = @"c:\master0\SimPy_forCVX"; // c:\master0\SimPy_forCVX
        var PythonFilename = "DabinReal.py"; // dabin 재성 파일 위치 

        // 220808 잠시 주석 처리
        Thread RuntheServer = new Thread(() => RunPython(Path, PythonFilename)); // RunPython() : Server인 파이썬을 실행시키는 함수
        RuntheServer.Start();

        Inte_DeckConnectTcpServer(); // 켜진 Server(Python)에 정보를 주고 받고 기능
    }

    void Inte_DeckConnectTcpServer()
    {
        try
        {
            using (socketConnection)
            {
                socketConnection.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Ports[1]));

                // ! Send !
                for (int i = 0; i < Deck_Aircraft.Count; i++) // 비행갑판 위 함재기 정보
                {
                    socketConnection.Send(BitConverter.GetBytes(InfosToByte(Deck_Aircraft[i][1], "@").Length));
                    socketConnection.Send(InfosToByte(Deck_Aircraft[i][1], "@")); // 함재기 이름만 Send
                }

                for (int i = 0; i < Deck_Resource.Count; i++) // 비행갑판 위 자원 정보
                {
                    socketConnection.Send(BitConverter.GetBytes(InfosToByte(Deck_Resource[i], "$").Length));
                    socketConnection.Send(InfosToByte(Deck_Resource[i], "$"));
                }

                UnityEngine.Debug.Log("함재기 정보 모두 송신 완료!");

                // ! Receive !
                var ReceiveData = new byte[4]; // 크기 할당은 그저 그릇을 만들어 준다는 의미, 서버에서 데이터가 들어오면 해당 데이터 크기만큼 생김
                socketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);
                Array.Reverse(ReceiveData);
                ReceiveData = new byte[BitConverter.ToInt32(ReceiveData, 0)];
                socketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);

                string ReceivedDataValue = Encoding.UTF8.GetString(ReceiveData); // Server로부터 받은 데이터 스트링으로
                UnityEngine.Debug.Log("From Python First : " + ReceivedDataValue);

                if (ReceivedDataValue == "Success")
                {
                    SimulationButton.SetActive(true); // .SetActive() : Main Thread에서만 부를수 있대
                    Invoke("ToFlightDeck_Inte", 2); // 2초 뒤에, 비행갑판 시뮬레이션 씬으로 이동하는 함수(ToSimulationScene()) 부르기
                }
            }
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e); // Server가 실행되지 않으면 나오는 에러
        }
    }

    public void ToFlightDeck_Inte()
    {
        SceneManager.LoadScene("FlightDeck_Inte");
        DontDestroyOnLoad(FinishedInformations);
    }

    public void Inte_HangarSocketButton() // 통합 모드[Inte_Socket 씬] 정비구역 버튼 누르면~ 
    {
        // 220918 수정
        var Path = @"d:\CVX_ver1\Real\Integrate";
        var PythonFilename = "Hangar.py";

        //Thread RuntheServer = new Thread(() => RunPython(Path, PythonFilename)); // RunPython() : Server인 파이썬을 실행시키는 함수
        //RuntheServer.Start();

        #region 220920 아래 3줄 잠시 생성 -> 확인 후 삭제하세용
        SimulationButton.SetActive(true);

        SceneManager.LoadScene("Hangar_Inte");
        DontDestroyOnLoad(FinishedInformations);
        #endregion

        //Inte_HangarConnectTcpServer(); // 220920 잠시 주석 처리
    }

    void Inte_HangarConnectTcpServer()
    {
        try
        {
            using (socketConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socketConnection.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Ports[1]));

                for (int i = 0; i < Hangar_Aircraft.Count; i++)
                {
                    socketConnection.Send(BitConverter.GetBytes(InfosToByte(Hangar_Aircraft[i][1], "%").Length));
                    socketConnection.Send(InfosToByte(Hangar_Aircraft[i][1], "%")); // 함재기 이름만 보낼거
                }

                for (int i = 0; i < Hangar_Resource.Count; i++)
                {
                    socketConnection.Send(BitConverter.GetBytes(InfosToByte(Hangar_Resource[i], "&").Length));
                    socketConnection.Send(InfosToByte(Hangar_Resource[i], "&"));
                }

                //Receive
                var ReceiveData = new byte[4];
                socketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);
                Array.Reverse(ReceiveData);
                ReceiveData = new byte[BitConverter.ToInt32(ReceiveData, 0)];
                socketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);

                string ReceivedDataValue = Encoding.UTF8.GetString(ReceiveData); // Server로부터 받은 데이터 스트링으로
                UnityEngine.Debug.Log("From Python First : " + ReceivedDataValue);

                if (ReceivedDataValue == "Success")
                {
                    SimulationButton.SetActive(true); // .SetActive() : Main Thread에서만 부를수 있대
                    Invoke("ToHangar_Inte", 2); // 2초 뒤에, 비행갑판 시뮬레이션 씬으로 이동 * 수정 필
                }
            }
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e); // Server가 실행되지 않으면 나오는 에러
        }
    }

    public void ToHangar_Inte()
    {
        SceneManager.LoadScene("Hangar_Inte");
        DontDestroyOnLoad(FinishedInformations);
    }

    public void Inte_EVSocketButton() // 통합 모드[Inte_Socket 씬] 엘리베이터 버튼 누르면~
    {
        Inte_EVConnectTcpServer();
    }

    void Inte_EVConnectTcpServer()
    {
        try
        {
            using (socketConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                socketConnection.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Ports[1]));

                for (int i = 0; i < Elevator_Velos.Count; i++)
                {
                    socketConnection.Send(BitConverter.GetBytes(InfosToByte(Elevator_Velos[i], "^").Length));
                    socketConnection.Send(InfosToByte(Elevator_Velos[i], "^"));
                }

                //Receive
                var ReceiveData = new byte[4];
                socketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);
                Array.Reverse(ReceiveData);
                ReceiveData = new byte[BitConverter.ToInt32(ReceiveData, 0)];
                socketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);

                string Success = Encoding.UTF8.GetString(ReceiveData);// + " 야호~! ";
                UnityEngine.Debug.Log(Success);

                if (Success.Length > 0)// == 15)
                {
                    SimulationButton.SetActive(true); // 데이터가 잘 들어왔다는 신호가 들어오면 Simulation 시작 버튼 등장 및 아래의 함수 실행
                    Invoke("ToElevator_Inte", 2); // 2초 뒤에, 비행갑판 시뮬레이션 씬으로 이동 * 수정 필
                }
            }
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e); // Server가 실행되지 않으면 나오는 에러
        }
    }

    public void ToElevator_Inte() // 소켓 통신을 통해 파이썬으로부터 신호를 받으면, 비행갑판 시뮬레이션 씬으로 이동
    {
        SceneManager.LoadScene("Elevator_Inte");
        DontDestroyOnLoad(FinishedInformations);
    }
    #endregion 통합 모드용

    public void RunPython(string path, string pythonname) // Server인 Python을 실행시키는 함수
    {
        ProcessStartInfo PythonStart = new ProcessStartInfo();
        PythonStart.FileName = @"C:/Users/rustj/.conda/envs/Simpy/python.exe"; // .exe 파일이 있는 경로 // 이재성컴 위치 C:\Users\rustj\.conda\envs\Simpy\python.exe //C:/Users/rustj/.conda/envs/Simpy/python.exe // C:/Users/rustj/AppData/Local/Programs/Python/Python311/python.exe
        PythonStart.Arguments = $"\"{path}\\{pythonname}"; //C:/Users/rustj/.conda/envs/Simpy/python.exe
        PythonStart.UseShellExecute = false;
        PythonStart.CreateNoWindow = true;
        PythonStart.RedirectStandardOutput = true;
        PythonStart.RedirectStandardError = true;

        using (Process process = Process.Start(PythonStart))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                UnityEngine.Debug.Log("Python Server 실행 완료!");
            }
        }
    }
   
    #region (for 소켓 통신) 소켓 통신을 위해 전송할 데이터들을 Byte[]로 Encoding
    public Byte[] InfosToByte(System.Object AreaIndex, string Symbol) // 갑판 및 정비구역의 함재기 상태 정보를 byte[]로
    {
        // 정보 특성에 따른 Symbol은 아래와 같다.
        // 비행갑판 함재기 정보 "@" / 비행갑판 자원 정보 "$" / 정비구역 함재기 정보 "%" / 정비구역 자원 정보 "&" / 엘리베이터 정보 "^"
        Byte[] InfosEncoding = Encoding.UTF8.GetBytes(AreaIndex.ToString() + Symbol); 
        return InfosEncoding;
    }
    #endregion

    #region 이 전의 Byte[]로 Encoding
    /*
    public Byte[] AreaInfosToByte(System.Object AreaIndex, string Symbol) // 갑판 및 정비구역의 함재기 상태 정보를 byte[]로
    {
        Byte[] AreaInfos = Encoding.UTF8.GetBytes(AreaIndex.ToString() + Symbol); //"$"
        return AreaInfos;
    }

    public Byte[] ResourcesToByte(int ResourcesIndex) // 갑판 및 정비구역의 자원 정보를 byte[]로
    {
        Byte[] ResourcesBytes = Encoding.UTF8.GetBytes(ResourcesIndex.ToString() + "&");
        return ResourcesBytes;
    }

    public Byte[] EVInfosToByte(double VelocityIndex) // 엘리베이터 정보를 byte[]로
    {
        Byte[] VelocityBytes = Encoding.UTF8.GetBytes(VelocityIndex.ToString() + "+");
        return VelocityBytes;
    }
    */
    #endregion
}