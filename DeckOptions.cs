using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeckOptions : MonoBehaviour
{
    ServiceAreaManagement AircraftsLocationOnDeck;
    DataManagement DeckData; // ��ſ��� ����� �� �ֵ���, ���఩���� ������ ����ִ� ����

    public InputField ExternalPort;
    public Text InternalPort; // 220903, InputField -> Text�� �ٲٱ�
    public GameObject ClearPortButton;

    public InputField All_Fleets;
    public InputField Deck_Fleet;

    public Dropdown Dropdown_Fleet;
    public Dropdown Dropdown_F35B;
    public Dropdown Dropdown_Posture;
    public Dropdown Dropdown_Area;

    public GameObject[] InitF35BButtons = new GameObject[5]; // Fleet ��ư, F35B ��ư, Posture ��ư, Select ��ư, Save F35B's Info ��ư ������
    List<int> HowmanyF35B = new List<int>(); // Fleet ��ư ��Ȱ��ȭ
    int FleetValue; // �� �� Fleet���� Ȯ�� & Fleet ���ڿ� ���� F35B �̸� ����

    private List<string> ServiceAreaList = new List<string> { "A1", "A2", "A3", "A4", "A5", "B1", "B2", "B3", "B4", "B5" };

    private Dictionary<int, string> ServiceArea = new Dictionary<int, string>();
    private List<string> Postures = new List<string> { "45 (deg)", "-45 (deg)" };

    public Sprite[] Aircrafts;
    public GameObject[] GameobjectsforArea = new GameObject[20];
    public Text[] TextsforArea = new Text[10];
    List<int> CheckingBrowns = new List<int>();

    #region ���఩�� Resource
    public InputField TractorValue;
    public InputField RedValue;
    public Text BrownValue; // Brown �ο� �� = ����� ����
    public InputField PurpleValue;
    public InputField GreenValue;
    public GameObject TractorImage;
    public GameObject RedImage;
    public GameObject BrownImage;
    public GameObject PurpleImage;
    public GameObject GreenImage;
    public GameObject EmptyResourceObject;

    // ���ҽ� �̹����� ���� �� ����
    List<GameObject> TractorClones = new List<GameObject>();
    List<GameObject> RedClones = new List<GameObject>();
    List<GameObject> BrownClones = new List<GameObject>();
    List<GameObject> PurpleClones = new List<GameObject>();
    List<GameObject> GreenClones = new List<GameObject>();
    #endregion

    /// <summary>
    /// 1. Dropdown.AddOptions(List<string>) : Dropdown.SetValueWithoutNotify(-1)�� ������� �ʾƵ�, ��Ӵٿ� �ɼ��� ���� ù��° ���� ��Ӵٿ� â�� �����ش�
    /// 2. Dropdown.options.Add(Dropdown.OptionData) : Dropdown.SetValueWithoutNotify(-1)�� ������� ������, ó�� ��Ӵٿ� â�� �ɼ��� ������ �ʴ´�. �׸��� ó�� ������ ��, ù��° �� ��뵵 ����
    /// => 2-1. Dropdown.SetValueWithoutNotify(-1)�� ���X = Dropdown.SetValueWithoutNotify(0);�� ����ϰų� or Dropdown.SetValueWithoutNotify()�� �ƿ� ������� ���� ��
    /// 3. F35B�̸��� ��Ӵٿ� ���� ����(MakeF35BDropdown()) : Dropdown_F35B.AddOptions(List<string>)������, Dropdown.options.Remove(Dropdown.OptionData) ������ Dropdown_F35B.SetValueWithoutNotify(-1) ���!
    /// ��, Dropdown.SetValueWithoutNotify(-1) : ��Ӵٿ��� ���� ������ Ǯ���� ����, ���? Dropdown.options.Add(Dropdown.OptionData)���� ������ ��Ӵٿ��� ���� <-> Dropdown.AddOptions(List<string>)
    /// </summary>

    private void Start()
    {
        ServiceArea[0] = "A1"; ServiceArea[1] = "A2"; ServiceArea[2] = "A3"; ServiceArea[3] = "A4"; ServiceArea[4] = "A5";
        ServiceArea[5] = "B1"; ServiceArea[6] = "B2"; ServiceArea[7] = "B3"; ServiceArea[8] = "B4"; ServiceArea[9] = "B5";

        /// �ּ� ī��
        GameObject DataManager = GameObject.Find("DataManagement").gameObject; // ����Ƽ ���̾��Ű â�� "DataManagement"��� �̸��� GameObject�� ã�� ����
        AircraftsLocationOnDeck = DataManager.GetComponent<DataManagement>().Area_management; // DataManager ������Ʈ�� ���� �ִ� DataManagement ��ũ��Ʈ�� Area_management��� ���� ServiceAreaManagement ��ũ��Ʈ�� �����ðž�
        AircraftsLocationOnDeck.SetServiceArea(ServiceArea.Count);
        /// �ּ� ī��

        DeckData = DataManager.GetComponent<DataManagement>(); //DeckData = new DataManagement(); -> �̷��� �ع����� ���� �������൵ ���� �ٲ�� ������ ���� �������� ����
        DeckData.Internal_PortNumber = int.Parse(InternalPort.text);
    }

    #region �ܺ� ��Ʈ ��ȣ ����
    public void ExPortNumber()
    {
        DeckData.External_PortNumber = int.Parse(ExternalPort.text);
        ExternalPort.readOnly = true; // ���� �Է� �Ұ���

        ClearPortButton.SetActive(true);
    }

    public void ClearPortValues()
    {
        ExternalPort.text = null;
        ExternalPort.readOnly = false;

        ClearPortButton.SetActive(false);
    }
    #endregion

    public void AllFleets()
    {
        DeckData.AllFleets = int.Parse(All_Fleets.text);

        if (DeckData.AllFleets == 0 || DeckData.AllFleets > 5)
        {
            Debug.Log(string.Format("{0}��� �̻��� ���� ����� �� �����ϴ�!", DeckData.AllFleets));
            All_Fleets.text = null;
        }

        else
        {
            All_Fleets.readOnly = true; // ����ڰ� 1~5 ������ ��븦 �����ϸ� Clear ��ư ������ ���� ���� X
        }
    }

    public void FleetsofDeck()
    {
        DeckData.DeckFleets = int.Parse(Deck_Fleet.text);
        int Hangar_Fleets_Numbers = DeckData.AllFleets - DeckData.DeckFleets;

        if (Hangar_Fleets_Numbers > 2)
        {
            Deck_Fleet.text = null;
            Debug.Log(string.Format("���񱸿��� {0}��� �̻��� ����⸦ ��ġ�� �� �����ϴ�!", Hangar_Fleets_Numbers));
        }

        else
        {
            if (DeckData.DeckFleets == 0 || DeckData.DeckFleets >= 4)
            {
                Debug.Log(string.Format("���఩�ǿ� {0}���� ����� �Ұ��մϴ�.", DeckData.DeckFleets));
                Deck_Fleet.text = null;
            }

            else if (DeckData.DeckFleets > DeckData.AllFleets)
            {
                Debug.Log(string.Format("������ ���({0}���)���� ���఩���� ���({1}���)�� �� �����ϴ�!", DeckData.AllFleets, DeckData.DeckFleets));
                Deck_Fleet.text = null;
            }

            else
            {
                Debug.Log(string.Format("���఩�� �� ������ ����� ��� ��� ���� : {0}���", DeckData.DeckFleets));
                Deck_Fleet.readOnly = true;

                MakeFleetDropdown();
            }
        }
    }

    public void MakeFleetDropdown() // ��� �ɼ� �����
    {
        Dropdown_Fleet.ClearOptions();

        List<string> fleetlist = new List<string>();

        for (int i = 0; i < DeckData.DeckFleets; i++)
        {
            fleetlist.Add(string.Format("Fleet #{0}", i + 1));
        }
        Dropdown_Fleet.AddOptions(fleetlist);
    }

    public void MakeF35BDropdown() // ���఩�� F35B �ɼ� �����
    {
        //InitF35BButtons[0].SetActive(false); // ��� ���� ��, �ٷ� ��ư ���ֱ� // �� ��� �츮�� �ȵ�
        Dropdown_Fleet.interactable = false; // ��� ���� ��, �� ���� �Ұ�

        Dropdown_F35B.ClearOptions();

        string GetFleetName = Dropdown_Fleet.options[Dropdown_Fleet.value].text;
        string[] GetValue = GetFleetName.Split('#');

        FleetValue = int.Parse(GetValue[1]);

        List<string> F35Bnamelist = new List<string>();

        if (FleetValue == 3)
        {
            for (int i = 1; i < 3; i++) // Fleet#3 : ���ǿ� 2��, ���񱸿��� 2��
            {
                F35Bnamelist.Add(string.Format("F35B#{0}", 4 * (FleetValue - 1) + i));
            }
            Dropdown_F35B.AddOptions(F35Bnamelist);
        }

        else
        {
            for (int i = 1; i < 5; i++) // 1 Fleet���� 4���� ������ ����� ���� -> i = 1,2,3,4
            {
                F35Bnamelist.Add(string.Format("F35B#{0}", 4 * (FleetValue - 1) + i));
            }
            Dropdown_F35B.AddOptions(F35Bnamelist);
        }
    }

    public void MakePostureDropdown()
    {
        Dropdown_Posture.options.Clear();

        for (int i = 0; i < Postures.Count; i++)
        {
            Dropdown.OptionData PostureOptions = new Dropdown.OptionData();
            PostureOptions.text = Postures[i];
            PostureOptions.image = Aircrafts[i];
            Dropdown_Posture.options.Add(PostureOptions); // Add(Dropdown.OptionData) �̹Ƿ� Dropdown_Posture.SetValueWithoutNotify(-1); ���~!
        }
        Dropdown_Posture.SetValueWithoutNotify(-1); // ��Ӵٿ� �ɼ� �� ���� ���� �ִ� �ɼ� ���� ��Ӵٿ� â�� ��
    }

    public void GetAngleImage() // �ڼ� ���� ��ư ������, ����� ���� �̹��� ���� �� ���� ���� �ɼ� �����
    {
        Dropdown_Posture.captionImage.transform.eulerAngles = new UnityEngine.Vector3(0.0f, 0.0f, 0.0f);

        if (Dropdown_Posture.value == 0)
        {
            Dropdown_Posture.captionImage.transform.eulerAngles = new UnityEngine.Vector3(0.0f, 0.0f, 45.0f);
        }

        else if (Dropdown_Posture.value == 1)
        {
            Dropdown_Posture.captionImage.transform.eulerAngles = new UnityEngine.Vector3(0.0f, 0.0f, -45.0f);
        }

        MakeAreaDropdown();
    }

    public void MakeAreaDropdown() // Service Area �ɼ� ����� 1
    {
        UpdateAreaDropdown();
    }

    public void UpdateAreaDropdown() // Service Area �ɼ� ����� 2
    {
        Dropdown_Area.options.Clear();

        List<int> Result = AircraftsLocationOnDeck.GetEmptyList();
        foreach (var item in Result)
        {
            Dropdown.OptionData AreaOptions = new Dropdown.OptionData();
            AreaOptions.text = ServiceArea[item];
            Dropdown_Area.options.Add(AreaOptions);
        }
        Dropdown_Area.SetValueWithoutNotify(-1); // ** ��Ӵٿ� �ɼ� �� ���� ���� �ִ� �ɼ� ���� ��Ӵٿ� â�� ��  //Dropdown_Area.SetValueWithoutNotify(0); // ��Ӵٿ� â�� �ƹ��͵� �� ��
    }

    public void GetAircraftImage() // Select Button ������ �� ����Ǵ� �Լ�
    {
        #region ���� ������ �ش��ϴ� ���� ���� ������ ����� �̸� �ؽ�Ʈ
        if (Dropdown_Area.options[Dropdown_Area.value].text == "A1" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[0].SetActive(true);
            TextsforArea[0].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A1����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "A1" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[1].SetActive(true);
            TextsforArea[0].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A1����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "A2" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[2].SetActive(true);
            TextsforArea[1].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A2����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "A2" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[3].SetActive(true);
            TextsforArea[1].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A2����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "A3" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[4].SetActive(true);
            TextsforArea[2].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A3����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "A3" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[5].SetActive(true);
            TextsforArea[2].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A3����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "A4" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[6].SetActive(true);
            TextsforArea[3].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A4����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "A4" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[7].SetActive(true);
            TextsforArea[3].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A4����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "A5" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[8].SetActive(true);
            TextsforArea[4].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A5����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "A5" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[9].SetActive(true);
            TextsforArea[4].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A5����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B1" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[10].SetActive(true);
            TextsforArea[5].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B1����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B1" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[11].SetActive(true);
            TextsforArea[5].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B1����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B2" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[12].SetActive(true);
            TextsforArea[6].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B2����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B2" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[13].SetActive(true);
            TextsforArea[6].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B2����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B3" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[14].SetActive(true);
            TextsforArea[7].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B3����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B3" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[15].SetActive(true);
            TextsforArea[7].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B3����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B4" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[16].SetActive(true);
            TextsforArea[8].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B4����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B4" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[17].SetActive(true);
            TextsforArea[8].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B4����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B5" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[18].SetActive(true);
            TextsforArea[9].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B5����
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B5" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[19].SetActive(true);
            TextsforArea[9].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B5����
        }
        #endregion

        int A_index = ServiceAreaList.IndexOf(Dropdown_Area.options[Dropdown_Area.value].text);
        AircraftsLocationOnDeck.AddInfo(A_index, Dropdown_Area.options[Dropdown_Area.value].text, Dropdown_F35B.options[Dropdown_F35B.value].text, Dropdown_Posture.captionImage.transform.eulerAngles.z);
        UpdateAreaDropdown();

        FleetOptionManage();

        Dropdown_F35B.options.Remove(Dropdown_F35B.options[Dropdown_F35B.value]);
        Dropdown_F35B.SetValueWithoutNotify(-1); // �̰� ���� �ȵȴ�
    }

    public void FleetOptionManage()
    {
        HowmanyF35B.Add(Dropdown_F35B.value); // HowmanyF35B ����Ʈ�� �Ȱ��� ���鵵 �ߺ����� ó������ �ʰ�, �� ���°� Ȯ�� �Ϸ�

        if (FleetValue == 3) // Fleet #3�̸� -> ���఩�ǿ� 2��, ���񱸿��� 2��
        {
            if (HowmanyF35B.Count == 2) // ���఩�ǿ� ����� 2���
            {
                Dropdown_Fleet.options.Remove(Dropdown_Fleet.options[Dropdown_Fleet.value]); // Fleet #3 ��� �����
                Dropdown_Fleet.SetValueWithoutNotify(-1);

                if (Dropdown_Fleet.options.Count == 0) // �갡 ������ ���̸�
                {
                    InitF35BButtons[0].SetActive(false);
                }

                else
                {
                    Dropdown_Fleet.interactable = true;
                    InitF35BButtons[0].SetActive(true); // �ٸ� ��� �ɼ� �����ؾ� �ϴϱ�!
                }

                HowmanyF35B.Clear();
            }

            else // (����) ������, �ش� ����� �������� ��� �������� ���� ���
            {
                Dropdown_Fleet.interactable = false;
                InitF35BButtons[0].SetActive(false);
            }
        }

        else // Fleet #1,2�� ���
        {
            if (HowmanyF35B.Count == 4)
            {
                Dropdown_Fleet.options.Remove(Dropdown_Fleet.options[Dropdown_Fleet.value]); // ������ ��뿡 �ִ� ������ �� ���������ϱ� ��������
                Dropdown_Fleet.SetValueWithoutNotify(-1);

                if (Dropdown_Fleet.options.Count == 0)
                {
                    InitF35BButtons[0].SetActive(false);
                }

                else
                {
                    Dropdown_Fleet.interactable = true;
                    InitF35BButtons[0].SetActive(true);
                }

                HowmanyF35B.Clear();
            }

            else // (����) ������, �ش� ����� �������� ��� �������� ���� ���
            {
                Dropdown_Fleet.interactable = false;
                InitF35BButtons[0].SetActive(false);
            }
        }
    }

    #region F35B ��ư�� ������� ��Ÿ���� ���� �Լ���
    //public GameObject[] InitF35BButtons = new GameObject[5]; // Fleet ��ư, F35B ��ư, Posture ��ư, Select ��ư, Save F35B's Info ��ư ������
    public void FleetButton()
    {
        List<int> Fleets = new List<int>(); // ���� ������ ���ö����� �ʱ�ȭ �Ǵϱ�

        Fleets.Add(Dropdown_Fleet.value);

        if (Fleets.Count == 1)
        {
            InitF35BButtons[0].SetActive(false);
        }
    }

    public void F35BButton() // F35B# ����, ��ư ������ ����� ���� ��ư �׾�
    {
        List<int> F35Bs = new List<int>(); // F35B ����Ⱑ �ϳ��� ���õ� ������

        F35Bs.Add(Dropdown_F35B.value);

        if (F35Bs.Count == 1)
        {
            InitF35BButtons[1].SetActive(false);
            InitF35BButtons[2].SetActive(true);
        }
    }

    public void PostureButton() // �ڼ� ����, ��ư ������ �ڼ� ���� ��ư �׾�
    {
        List<int> Postures = new List<int>(); 

        Postures.Add(Dropdown_F35B.value);

        if (Postures.Count == 1)
        {
            InitF35BButtons[2].SetActive(false);
        }
    }

    public void F35BSelectButton() // Select ��ư ������ �� ��Ƴ�
    {
        if (Dropdown_Fleet.options.Count == 0 && Dropdown_F35B.options.Count == 0)
        {
            InitF35BButtons[1].SetActive(false);
            InitF35BButtons[2].SetActive(false);
            InitF35BButtons[3].SetActive(false);
            InitF35BButtons[4].SetActive(true);
        }

        else
        {
            InitF35BButtons[1].SetActive(true);
        }
    }
    #endregion

    public void F35BInitialization() // F35B�� ���� ���� ��� Reset
    {
        All_Fleets.text = null;
        All_Fleets.readOnly = false; // ����ڰ� �Է� ����

        Deck_Fleet.text = null;
        Deck_Fleet.readOnly = false; // ����ڰ� �Է� ����

        Dropdown_Fleet.interactable = true;
        Dropdown_Fleet.ClearOptions(); // dropdown.ClearOptions() == dropdown.options.Clear() : ��Ӵٿ�� �ɼ� ��� ���� 
        Dropdown_F35B.ClearOptions();
        Dropdown_Posture.ClearOptions();
        Dropdown_Area.ClearOptions();

        BrownValue.text = null;
        CheckingBrowns.Clear();

        for (int i = 0; i < GameobjectsforArea.Length; i++)
        {
            GameobjectsforArea[i].SetActive(false);
        }

        for (int i = 0; i < TextsforArea.Length; i++)
        {
            TextsforArea[i].text = default;
        }
        Start(); // ���� ���� ������ �־����

        InitF35BButtons[0].SetActive(true);
        InitF35BButtons[1].SetActive(true);
        InitF35BButtons[2].SetActive(true);
        InitF35BButtons[3].SetActive(true);
        InitF35BButtons[4].SetActive(false);
    }

    //Resource ���� UGUI
    public void Resource_Tractor()
    {
        DeckData.Tractors = int.Parse(TractorValue.text);
        DestroyResourceImage(TractorClones);
        TractorClones.Clear();

        if (DeckData.Tractors == 0)
        {
            TractorImage.SetActive(false);
            Debug.Log("1�� �̻��� Ʈ���Ͱ� �ʿ��մϴ�.");
            TractorValue.text = null;
        }

        else if (DeckData.Tractors > 6)
        {
            TractorImage.SetActive(false);
            Debug.Log(string.Format("{0}�� �̻��� Ʈ���ʹ� ����� �� �����ϴ�.", DeckData.Tractors));
            TractorValue.text = null;
        }

        else
        {
            TractorValue.readOnly = true;
            TractorImage.SetActive(true);

            // �����Ǵ� Ʈ���� �̹����� ��ġ�� ���� ��ǥ��� �ؾ��ؼ�,,
            List<Vector2> TractorLocation = new List<Vector2>() { new Vector2(1150, 255), new Vector2(1120, 255), new Vector2(1090, 255), new Vector2(1060, 255), new Vector2(1030, 255) };

            for (int i = 0; i < DeckData.Tractors - 1; i++)
            {
                GameObject TCloneindex = Instantiate(TractorImage, new Vector3(TractorLocation[i].x, TractorLocation[i].y), Quaternion.identity);
                TCloneindex.transform.parent = EmptyResourceObject.transform;
                //TCloneindex.transform.parent = TractorImage.transform; -> �� �����ϸ� �������� ����� ����?
                TractorClones.Add(TCloneindex);
            }
        }
    }

    public void Resource_RedGroup()
    {
        DeckData.RedGroup = int.Parse(RedValue.text);
        DestroyResourceImage(RedClones);
        RedClones.Clear();

        if (DeckData.RedGroup == 0)
        {
            RedImage.SetActive(false);
            Debug.Log("1�� �̻��� Red �ο��� �ʿ��մϴ�.");
            RedValue.text = null;
        }

        else if (DeckData.RedGroup > 6)
        {
            RedImage.SetActive(false);
            Debug.Log(string.Format("{0}�� �̻��� Red ���� �ڿ��� ����� �� �����ϴ�.", DeckData.RedGroup));
            RedValue.text = null;
        }

        else
        {
            RedValue.readOnly = true;
            RedImage.SetActive(true);

            List<Vector2> RedLocation = new List<Vector2>() { new Vector2(940, 275), new Vector2(920, 275), new Vector2(900, 275), new Vector2(880, 275), new Vector2(860, 275) };

            for (int i = 0; i < DeckData.RedGroup - 1; i++)
            {
                GameObject RCloneindex = Instantiate(RedImage, new Vector3(RedLocation[i].x, RedLocation[i].y), Quaternion.identity);
                RCloneindex.transform.parent = EmptyResourceObject.transform;
                RedClones.Add(RCloneindex);
            }
        }
    }

    public void SaveF35BInfo() // Brown ���� ���Ϸ���. Brown = ����� ����
    {
        CheckingBrowns.Clear();

        for (int i = 0; i < TextsforArea.Length; i++)
        {
            if (TextsforArea[i].text != "") // TextsforArea[i].text�� ""�� �ƴϸ� -> ����Ⱑ ä�����ٴ� �ǹ�
            {
                CheckingBrowns.Add(i);
            }
        }

        BrownValue.text = Convert.ToString(CheckingBrowns.Count);
        Resource_BrownGroup();
    }

    public void Resource_BrownGroup()
    {
        DeckData.BrownGroup = int.Parse(BrownValue.text);
        BrownClones.Clear();

        BrownImage.SetActive(true);

        List<Vector2> BrownLocation = new List<Vector2>() { new Vector2(940, 260), new Vector2(920, 260), new Vector2(900, 260), new Vector2(880, 260), new Vector2(860, 260),
            new Vector2(840, 260), new Vector2(820, 260), new Vector2(800, 260), new Vector2(780, 260), new Vector2(760, 260) };

        for (int i = 0; i < DeckData.BrownGroup - 1; i++)
        {
            GameObject BCloneindex = Instantiate(BrownImage, new Vector3(BrownLocation[i].x, BrownLocation[i].y), Quaternion.identity);
            BCloneindex.transform.parent = EmptyResourceObject.transform;

            BrownClones.Add(BCloneindex);
        }
    }

    public void Resource_PurpleGroup()
    {
        DeckData.PurpleGroup = int.Parse(PurpleValue.text);
        DestroyResourceImage(PurpleClones);
        PurpleClones.Clear();

        if (DeckData.PurpleGroup == 0)
        {
            PurpleImage.SetActive(false);
            Debug.Log("1�� �̻��� Purple �ο��� �ʿ��մϴ�.");
            PurpleValue.text = null;
        }

        else if (DeckData.PurpleGroup > 6)
        {
            PurpleImage.SetActive(false);
            Debug.Log(string.Format("{0}�� �̻��� Purple ���� �ڿ��� ����� �� �����ϴ�.", DeckData.PurpleGroup));
            PurpleValue.text = null;
        }

        else
        {
            PurpleValue.readOnly = true;
            PurpleImage.SetActive(true);

            List<Vector2> PurpleLocation = new List<Vector2>() { new Vector2(940, 245), new Vector2(920, 245), new Vector2(900, 245), new Vector2(880, 245), new Vector2(860, 245) };

            for (int i = 0; i < DeckData.PurpleGroup - 1; i++)
            {
                GameObject PCloneindex = Instantiate(PurpleImage, new Vector3(PurpleLocation[i].x, PurpleLocation[i].y), Quaternion.identity);
                PCloneindex.transform.parent = EmptyResourceObject.transform;

                PurpleClones.Add(PCloneindex);
            }
        }
    }

    public void Resource_GreenGroup()
    {
        DeckData.GreenGroup = int.Parse(GreenValue.text);
        DestroyResourceImage(GreenClones);
        GreenClones.Clear();

        if (DeckData.GreenGroup == 0)
        {
            GreenImage.SetActive(false);
            Debug.Log("1�� �̻��� Green �ο��� �ʿ��մϴ�.");
            GreenValue.text = null;
        }

        else if (DeckData.GreenGroup > 6)
        {
            GreenImage.SetActive(false);
            Debug.Log(string.Format("{0}�� �̻��� Green ���� �ڿ��� ����� �� �����ϴ�.", DeckData.GreenGroup));
            GreenValue.text = null;
        }

        else
        {
            GreenValue.readOnly = true;
            GreenImage.SetActive(true);

            List<Vector2> GreenLocation = new List<Vector2>() { new Vector2(940, 230), new Vector2(920, 230), new Vector2(900, 230), new Vector2(880, 230), new Vector2(860, 230) };

            for (int i = 0; i < DeckData.GreenGroup - 1; i++)
            {
                GameObject GCloneindex = Instantiate(GreenImage, new Vector3(GreenLocation[i].x, GreenLocation[i].y), Quaternion.identity);
                GCloneindex.transform.parent = EmptyResourceObject.transform;

                GreenClones.Add(GCloneindex);
            }
        }
    }

    public void DestroyResourceImage(List<GameObject> Resources)
    {
        for (int i = 0; i < Resources.Count; i++)
        {
            Destroy(Resources[i]);
        }
    }

    public void ResourceInitialization() // Deck Resource�� ���� ���� ��� Reset
    {
        TractorValue.text = null;
        RedValue.text = null;
        //BrownValue.text = null; //Brown : ����� ������ �������� �� ���� ���� �������� �Ŷ� �ٲ�ų� �ٲ� �� ����
        PurpleValue.text = null;
        GreenValue.text = null;

        TractorValue.readOnly = false;
        RedValue.readOnly = false;
        PurpleValue.readOnly = false;
        GreenValue.readOnly = false;

        TractorImage.SetActive(false);
        DestroyResourceImage(TractorClones);

        RedImage.SetActive(false);
        DestroyResourceImage(RedClones);

        //BrownImage.SetActive(false); //Brown : ����� ������ �������� �� ���� ���� �������� �Ŷ� �ٲ�ų� �ٲ� �� ����
        //DestroyResourceImage(BrownClones);

        PurpleImage.SetActive(false);
        DestroyResourceImage(PurpleClones);

        GreenImage.SetActive(false);
        DestroyResourceImage(GreenClones);
    }

    public void Alone_ToOtherInfoScene() // �ܵ� ��忡���� "NextButton : Hangar & EV Info >>"
    {
        SceneManager.LoadScene("Alone_OtherInformation");
        DontDestroyOnLoad(DeckData.AllDataManager);
    }

    public void Inte_ToOtherInfoScene() // ���� ��忡���� "NextButton : Hangar & EV Info >>"
    {
        SceneManager.LoadScene("Inte_OtherInformation");
        DontDestroyOnLoad(DeckData.AllDataManager);
    }
}
