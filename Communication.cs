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

public class Communication : MonoBehaviour // [�ܵ� + ���� ���] ���� �� ����� �Է� ���� ���� �� ���� ����� ���� ���� 
{
    // ������ �׾Ƴ��� ������(Deck ���� Hangar �� ���� ����Ϸ��� ������)
    GameObject DatasObject;
    DataManagement AllDatas;
    ServiceAreaManagement AircraftsLocationOnDeck; // ���఩�� �� ����� �������� ����
    HangarAreaManagement AircraftsLocationOnHangar; // ���񱸿� �� ����� �������� ����

    // ���� ���� ���� ���� ��Ƴ��� �������� ���� ���� ����
    public GameObject FinishedInformations; // ���� �ٲ㵵 ������ ����� ���������� DontDestroyOnLoad(GameObject)�� ����ؾ��ؼ� GameObject ����

    // "Informations"�� ��� ������
    List<int> Ports = new List<int>();
    List<ArrayList> Deck_Aircraft = new List<ArrayList>();
    List<int> Deck_Resource = new List<int>(); // �� TowbarTractor, Red, Brown, Purple, Green �ڿ� ���������� ���(int_List���� Dict���� �ٲٱ�)
    List<ArrayList> Hangar_Aircraft = new List<ArrayList>();
    List<int> Hangar_Resource = new List<int>(); // �� TowbarlessTractor, Green �ڿ� ���������� ���(int_List���� Dict���� �ٲٱ�)
    List<double> Elevator_Velos = new List<double>(); // (double_List���� Dict���� �ٲٱ�)
    ArrayList Informations = new ArrayList(); // ���� ������ �Ѱ��� ������

    // ���� ���� ������
    public Socket socketConnection;
    public GameObject SimulationButton; // Server���� ������ �޾Ҵٴ� ��ȣ�� ������ �ùķ��̼� ��ư�� �����ؾ� �ϱ� ������

    void Start()
    {
        DatasObject = GameObject.Find("DataManagement");
        AllDatas = DatasObject.GetComponent<DataManagement>(); // "DataManagement"��� ���ӿ�����Ʈ�� �ִ� "DataManagement" ��ũ��Ʈ�� ����ҰŴ�!

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

    #region ����� �Է� ���� ����
    public void PortNumber()
    {
        Ports.Add(AllDatas.External_PortNumber); // �ܺ� ��Ʈ ��ȣ
        Ports.Add(AllDatas.Internal_PortNumber); // ���� ��Ʈ ��ȣ

        Informations.Add(Ports);
    }

    public void ServiceArea()
    {
        for (int i = 0; i < 10; i++) // ���఩���� ���� ������ 10��
        {
            if (AircraftsLocationOnDeck.ServiceArea_State[i] != null) // ��� ������ ���� ��ҵ鸸
            {
                ArrayList Deck_Aircraft_Index = new ArrayList(); // ���� �� ���� �ʱ�ȭ�ȱ�

                Deck_Aircraft_Index.Add(AircraftsLocationOnDeck.ServiceArea_State[i].AREA_NAME); // ���� ���� �̸�
                Deck_Aircraft_Index.Add(AircraftsLocationOnDeck.ServiceArea_State[i].AIRCRAFT_NAME); // ����� �̸�
                Deck_Aircraft_Index.Add(AircraftsLocationOnDeck.ServiceArea_State[i].AIRCRAFT_POSTURE); // ����Ⱑ ���� �ڼ�

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

        Informations.Add(Deck_Resource); // Informations�� Informations[1] �ϼ�
    }

    public void HangarArea()
    {
        for (int i = 0; i < 10; i++) // ���� ���� �� ���� ���� ������ 10��
        {
            if (AircraftsLocationOnHangar.HangarArea_State[i] != null) // ��� ������ ���� ��ҵ鸸
            {
                ArrayList Hangar_Aircraft_Index = new ArrayList();

                Hangar_Aircraft_Index.Add(AircraftsLocationOnHangar.HangarArea_State[i].AREA_NAME); // ���� ���� �̸�
                Hangar_Aircraft_Index.Add(AircraftsLocationOnHangar.HangarArea_State[i].AIRCRAFT_NAME); // ����� �̸�

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

    void SaveInfosText() // ����� �Է� ������ .txt ���Ϸ� ����
    {
        // ����Ǵ� ������ �̸��� ���� ��� �ٲ��ֱ�
        FileStream InfosText = new FileStream("C:\\master0\\navalized aircraft\\Unity\\CVX_ver3\\InfosText.txt", FileMode.Create);

        StreamWriter testStreamWriter = new StreamWriter(InfosText);

        // ��/���� ��Ʈ ��ȣ
        testStreamWriter.Write(string.Format("External_PortNumber {0}", Ports[0] + "\r\n"));
        testStreamWriter.Write(string.Format("Internal_PortNumber {0}", Ports[1] + "\r\n"));

        // ���఩�� ���� ����� ����
        for (int i = 0; i < Deck_Aircraft.Count; i++)
        {
            testStreamWriter.Write(string.Format("{0} {1} {2}", Deck_Aircraft[i][0], Deck_Aircraft[i][1], Deck_Aircraft[i][2] + "\r\n"));
            //testStreamWriter.Write('\n');
        }

        // ���఩�� �ڿ�
        testStreamWriter.Write(string.Format("TowbarTractor {0}", Deck_Resource[0] + "\r\n"));
        testStreamWriter.Write(string.Format("RedGroup {0}", Deck_Resource[1] + "\r\n"));
        testStreamWriter.Write(string.Format("BrownGroup {0}", Deck_Resource[2] + "\r\n"));
        testStreamWriter.Write(string.Format("PurpleGroup {0}", Deck_Resource[3] + "\r\n"));
        testStreamWriter.Write(string.Format("GreenGroup {0}", Deck_Resource[4] + "\r\n"));

        // ���񱸿� ����� ����
        for (int i = 0; i < Hangar_Aircraft.Count; i++)
        {
            testStreamWriter.Write(string.Format("{0} {1}", Hangar_Aircraft[i][0], Hangar_Aircraft[i][1] + "\r\n"));
        }

        // ���񱸿� �ڿ�
        testStreamWriter.Write(string.Format("TowbarlessTractor {0}", Hangar_Resource[0] + "\r\n"));
        testStreamWriter.Write(string.Format("Hangar_GreenGroup {0}", Hangar_Resource[1] + "\r\n"));

        // ���������͵� �ӵ�
        for (int i = 0; i < Elevator_Velos.Count; i++)
        {
            testStreamWriter.Write(string.Format("D{0}_EV {1}", i+1, Elevator_Velos[i] + "\r\n"));
        }

        #region �̰͵� ������ �ʳ�
        /*
        // ��Ʈ ��ȣ
        for (int i = 0; i < Ports.Count; i++)
        {
            testStreamWriter.Write(string.Format("External_PortNumber {0}", Ports[i])); //switch ���� ���� �ʹ� �����ε�
            testStreamWriter.Write('\n');
        }

        // Deck_Resource
        testStreamWriter.Write(string.Format("���఩�� �� Blue �� Towbar Tractor ���� : {0}��", Deck_Resource[0] + '\n'));
        testStreamWriter.Write(string.Format("���఩�� �� Red �ο� : {0}��", Deck_Resource[1] + '\n'));
        testStreamWriter.Write(string.Format("���఩�� �� Brown �ο� : {0}��", Deck_Resource[2] + '\n'));
        testStreamWriter.Write(string.Format("���఩�� �� Purple �ο� : {0}��", Deck_Resource[3] + '\n'));
        testStreamWriter.Write(string.Format("���఩�� �� Green �ο� : {0}��", Deck_Resource[4] + '\n'));

        // Hangar_Resource
        testStreamWriter.Write(string.Format("���񱸿� �� Blue �� Towbarless Tractor ���� : {0}��", Hangar_Resource[0] + '\n'));
        testStreamWriter.Write(string.Format("���񱸿� �� Green �ο� : {0}��", Hangar_Resource[1] + '\n'));
        */
        #endregion

        testStreamWriter.Close();
    }

    public ArrayList MakeInfomations()
    {
        return Informations; // ����ڰ� �Է���, '���' ������ ��� �ִ� List
    }
    #endregion

    #region �ܵ� ����
    public void Alone_SocketButton() // �ܵ� ���[Alone_Socket ��] ���఩�� ��ư ������~
    {
        // [221031] ���� Ȯ���ϸ�, SimPy ��� �����ϱ�
        var Path = @"C:\master0\navalized aircraft\Unity\CVX_ver3\SimPy_forCVX"; // ������ ������ ���
        var PythonFilename = "DabinReal.py"; // ���� �̸� var PythonFilename

        // *221013 �Ʒ� ���� �ּ� ó��
        //Thread RuntheServer = new Thread(() => RunPython(Path, PythonFilename));
        //RuntheServer.Start();

        Alone_ConnectTcpServer(); // ���� Server(Python)�� ������ �ְ� �ް� ���

        // ���߿�, ���� ����ڰ� �Է� ���� ���� ������ � �� �������� Ȯ��, ���� �߻���Ű��
    }

    void Alone_ConnectTcpServer()
    {
        try
        {
            using (socketConnection)
            {
                socketConnection.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Ports[1]));

                #region ! ����ڰ� �Է��� ��� ������ ���̽�(SimPy)���� Send !
                for (int i = 0; i < Deck_Aircraft.Count; i++) // ���఩�� �� ����� ����
                {
                    socketConnection.Send(BitConverter.GetBytes(InfosToByte(Deck_Aircraft[i][1], "@").Length));
                    socketConnection.Send(InfosToByte(Deck_Aircraft[i][1], "@")); // ����� �̸��� Send
                }

                for (int i = 0; i < Deck_Resource.Count; i++) // ���఩�� �� �ڿ� ����
                {
                    socketConnection.Send(BitConverter.GetBytes(InfosToByte(Deck_Resource[i], "$").Length));
                    socketConnection.Send(InfosToByte(Deck_Resource[i], "$"));
                }

                for (int i = 0; i < Hangar_Aircraft.Count; i++) // ���񱸿� �� ����� ����
                {
                    socketConnection.Send(BitConverter.GetBytes(InfosToByte(Hangar_Aircraft[i][1], "%").Length));
                    socketConnection.Send(InfosToByte(Hangar_Aircraft[i][1], "%")); // ����� �̸��� ������
                }

                for (int i = 0; i < Hangar_Resource.Count; i++) // ���񱸿� �� �ڿ� ����
                {
                    socketConnection.Send(BitConverter.GetBytes(InfosToByte(Hangar_Resource[i], "&").Length));
                    socketConnection.Send(InfosToByte(Hangar_Resource[i], "&"));
                }

                for (int i = 0; i < Elevator_Velos.Count; i++) // ���������� ����
                {
                    socketConnection.Send(BitConverter.GetBytes(InfosToByte(Elevator_Velos[i], "^").Length));
                    socketConnection.Send(InfosToByte(Elevator_Velos[i], "^"));
                }
                UnityEngine.Debug.Log("����� ���� ��� �۽� �Ϸ�!");
                #endregion ! ����ڰ� �Է��� ��� ������ ���̽�(SimPy)���� Send !

                #region ! ���̽����κ��� ��ȣ�� Receive !
                var ReceiveData = new byte[4]; // ũ�� �Ҵ��� ���� �׸��� ����� �شٴ� �ǹ�, �������� �����Ͱ� ������ �ش� ������ ũ�⸸ŭ ����
                socketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);
                Array.Reverse(ReceiveData);
                ReceiveData = new byte[BitConverter.ToInt32(ReceiveData, 0)];
                socketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);

                string ReceivedDataValue = Encoding.UTF8.GetString(ReceiveData); // Server�κ��� ���� ������ ��Ʈ������
                UnityEngine.Debug.Log("From Python First : " + ReceivedDataValue);

                if (ReceivedDataValue == "Success")
                {
                    SimulationButton.SetActive(true); // .SetActive() : Main Thread������ �θ��� �ִ�
                    Invoke("ToFlightDeck_Alone", 2); // 2�� �ڿ�, ���఩�� �ùķ��̼� ������ �̵��ϴ� �Լ�(ToSimulationScene()) �θ���
                }
                #endregion ! ���̽����κ��� ��ȣ�� Receive !
            }
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e); // Server�� ������� ������ ������ ����
        }
    }

    public void ToFlightDeck_Alone()
    {
        SceneManager.LoadScene("FlightDeck_Alone");
        DontDestroyOnLoad(FinishedInformations);

        socketConnection.Close(); // *221028 �߰�
    }
    #endregion

    #region ���� ����
    public void Inte_DeckSocketButton() // ���� ���[Inte_Socket ��] ���఩�� ��ư ������~
    {
        var Path = @"c:\master0\SimPy_forCVX"; // c:\master0\SimPy_forCVX
        var PythonFilename = "DabinReal.py"; // dabin �缺 ���� ��ġ 

        // 220808 ��� �ּ� ó��
        Thread RuntheServer = new Thread(() => RunPython(Path, PythonFilename)); // RunPython() : Server�� ���̽��� �����Ű�� �Լ�
        RuntheServer.Start();

        Inte_DeckConnectTcpServer(); // ���� Server(Python)�� ������ �ְ� �ް� ���
    }

    void Inte_DeckConnectTcpServer()
    {
        try
        {
            using (socketConnection)
            {
                socketConnection.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Ports[1]));

                // ! Send !
                for (int i = 0; i < Deck_Aircraft.Count; i++) // ���఩�� �� ����� ����
                {
                    socketConnection.Send(BitConverter.GetBytes(InfosToByte(Deck_Aircraft[i][1], "@").Length));
                    socketConnection.Send(InfosToByte(Deck_Aircraft[i][1], "@")); // ����� �̸��� Send
                }

                for (int i = 0; i < Deck_Resource.Count; i++) // ���఩�� �� �ڿ� ����
                {
                    socketConnection.Send(BitConverter.GetBytes(InfosToByte(Deck_Resource[i], "$").Length));
                    socketConnection.Send(InfosToByte(Deck_Resource[i], "$"));
                }

                UnityEngine.Debug.Log("����� ���� ��� �۽� �Ϸ�!");

                // ! Receive !
                var ReceiveData = new byte[4]; // ũ�� �Ҵ��� ���� �׸��� ����� �شٴ� �ǹ�, �������� �����Ͱ� ������ �ش� ������ ũ�⸸ŭ ����
                socketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);
                Array.Reverse(ReceiveData);
                ReceiveData = new byte[BitConverter.ToInt32(ReceiveData, 0)];
                socketConnection.Receive(ReceiveData, ReceiveData.Length, SocketFlags.None);

                string ReceivedDataValue = Encoding.UTF8.GetString(ReceiveData); // Server�κ��� ���� ������ ��Ʈ������
                UnityEngine.Debug.Log("From Python First : " + ReceivedDataValue);

                if (ReceivedDataValue == "Success")
                {
                    SimulationButton.SetActive(true); // .SetActive() : Main Thread������ �θ��� �ִ�
                    Invoke("ToFlightDeck_Inte", 2); // 2�� �ڿ�, ���఩�� �ùķ��̼� ������ �̵��ϴ� �Լ�(ToSimulationScene()) �θ���
                }
            }
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e); // Server�� ������� ������ ������ ����
        }
    }

    public void ToFlightDeck_Inte()
    {
        SceneManager.LoadScene("FlightDeck_Inte");
        DontDestroyOnLoad(FinishedInformations);
    }

    public void Inte_HangarSocketButton() // ���� ���[Inte_Socket ��] ���񱸿� ��ư ������~ 
    {
        // 220918 ����
        var Path = @"d:\CVX_ver1\Real\Integrate";
        var PythonFilename = "Hangar.py";

        //Thread RuntheServer = new Thread(() => RunPython(Path, PythonFilename)); // RunPython() : Server�� ���̽��� �����Ű�� �Լ�
        //RuntheServer.Start();

        #region 220920 �Ʒ� 3�� ��� ���� -> Ȯ�� �� �����ϼ���
        SimulationButton.SetActive(true);

        SceneManager.LoadScene("Hangar_Inte");
        DontDestroyOnLoad(FinishedInformations);
        #endregion

        //Inte_HangarConnectTcpServer(); // 220920 ��� �ּ� ó��
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
                    socketConnection.Send(InfosToByte(Hangar_Aircraft[i][1], "%")); // ����� �̸��� ������
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

                string ReceivedDataValue = Encoding.UTF8.GetString(ReceiveData); // Server�κ��� ���� ������ ��Ʈ������
                UnityEngine.Debug.Log("From Python First : " + ReceivedDataValue);

                if (ReceivedDataValue == "Success")
                {
                    SimulationButton.SetActive(true); // .SetActive() : Main Thread������ �θ��� �ִ�
                    Invoke("ToHangar_Inte", 2); // 2�� �ڿ�, ���఩�� �ùķ��̼� ������ �̵� * ���� ��
                }
            }
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e); // Server�� ������� ������ ������ ����
        }
    }

    public void ToHangar_Inte()
    {
        SceneManager.LoadScene("Hangar_Inte");
        DontDestroyOnLoad(FinishedInformations);
    }

    public void Inte_EVSocketButton() // ���� ���[Inte_Socket ��] ���������� ��ư ������~
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

                string Success = Encoding.UTF8.GetString(ReceiveData);// + " ��ȣ~! ";
                UnityEngine.Debug.Log(Success);

                if (Success.Length > 0)// == 15)
                {
                    SimulationButton.SetActive(true); // �����Ͱ� �� ���Դٴ� ��ȣ�� ������ Simulation ���� ��ư ���� �� �Ʒ��� �Լ� ����
                    Invoke("ToElevator_Inte", 2); // 2�� �ڿ�, ���఩�� �ùķ��̼� ������ �̵� * ���� ��
                }
            }
        }

        catch (Exception e)
        {
            UnityEngine.Debug.Log("On client connect exception " + e); // Server�� ������� ������ ������ ����
        }
    }

    public void ToElevator_Inte() // ���� ����� ���� ���̽����κ��� ��ȣ�� ������, ���఩�� �ùķ��̼� ������ �̵�
    {
        SceneManager.LoadScene("Elevator_Inte");
        DontDestroyOnLoad(FinishedInformations);
    }
    #endregion ���� ����

    public void RunPython(string path, string pythonname) // Server�� Python�� �����Ű�� �Լ�
    {
        ProcessStartInfo PythonStart = new ProcessStartInfo();
        PythonStart.FileName = @"C:/Users/rustj/.conda/envs/Simpy/python.exe"; // .exe ������ �ִ� ��� // ���缺�� ��ġ C:\Users\rustj\.conda\envs\Simpy\python.exe //C:/Users/rustj/.conda/envs/Simpy/python.exe // C:/Users/rustj/AppData/Local/Programs/Python/Python311/python.exe
        PythonStart.Arguments = $"\"{path}\\{pythonname}"; //C:/Users/rustj/.conda/envs/Simpy/python.exe
        PythonStart.UseShellExecute = false;
        PythonStart.CreateNoWindow = true;
        PythonStart.RedirectStandardOutput = true;
        PythonStart.RedirectStandardError = true;

        using (Process process = Process.Start(PythonStart))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                UnityEngine.Debug.Log("Python Server ���� �Ϸ�!");
            }
        }
    }
   
    #region (for ���� ���) ���� ����� ���� ������ �����͵��� Byte[]�� Encoding
    public Byte[] InfosToByte(System.Object AreaIndex, string Symbol) // ���� �� ���񱸿��� ����� ���� ������ byte[]��
    {
        // ���� Ư���� ���� Symbol�� �Ʒ��� ����.
        // ���఩�� ����� ���� "@" / ���఩�� �ڿ� ���� "$" / ���񱸿� ����� ���� "%" / ���񱸿� �ڿ� ���� "&" / ���������� ���� "^"
        Byte[] InfosEncoding = Encoding.UTF8.GetBytes(AreaIndex.ToString() + Symbol); 
        return InfosEncoding;
    }
    #endregion

    #region �� ���� Byte[]�� Encoding
    /*
    public Byte[] AreaInfosToByte(System.Object AreaIndex, string Symbol) // ���� �� ���񱸿��� ����� ���� ������ byte[]��
    {
        Byte[] AreaInfos = Encoding.UTF8.GetBytes(AreaIndex.ToString() + Symbol); //"$"
        return AreaInfos;
    }

    public Byte[] ResourcesToByte(int ResourcesIndex) // ���� �� ���񱸿��� �ڿ� ������ byte[]��
    {
        Byte[] ResourcesBytes = Encoding.UTF8.GetBytes(ResourcesIndex.ToString() + "&");
        return ResourcesBytes;
    }

    public Byte[] EVInfosToByte(double VelocityIndex) // ���������� ������ byte[]��
    {
        Byte[] VelocityBytes = Encoding.UTF8.GetBytes(VelocityIndex.ToString() + "+");
        return VelocityBytes;
    }
    */
    #endregion
}