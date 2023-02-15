using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;
using System.Globalization;

public class OtherOptions : MonoBehaviour
{
    GameObject DatasObject;
    DataManagement AllDatas;
    ServiceAreaManagement AircraftsLocationOnDeck;
    HangarAreaManagement AircraftsLocationOnHangar;

    public Text HangarFleetsfromDeck;

    int AllFleets;
    int DeckFleets;
    int HangarFleets;

    public Dropdown Hangar_Fleets;
    public Dropdown Hangar_F35B;
    public Dropdown Hangar_Area;

    public GameObject[] HangarF35BButtons = new GameObject[4]; // Fleet ��ư, F35B ��ư, Select ��ư, Save F35B's Info ��ư ������
    List<int> HowmanyF35BinHangar = new List<int>(); // Fleet ��ư ��Ȱ��ȭ
    int HangarFleetValue; // �� �� Fleet���� Ȯ�� & Fleet ���ڿ� ���� F35B �̸� ����

    public InputField[] D1_ElevatorValues; // = new InputField[2];
    public InputField[] D2_ElevatorValues;

    public GameObject[] HangarAreaImages = new GameObject[10];
    public Text[] HangarAreaTexts = new Text[10];
    private List<string> HangarAreaList = new List<string> { "H1", "H2", "H3", "H4", "H5", "H6", "H7", "H8", "H9", "H10" };
    private Dictionary<int, string> HangarArea = new Dictionary<int, string>();
    public Text[] ElevatorTexts = new Text[2];


    #region ���񱸿� Resource ���� ����
    public InputField Hangar_TractorValue;
    public InputField Hangar_GreenValue;

    public GameObject Hangar_TractorImage;
    public GameObject Hangar_GreenImage;

    public GameObject EmptyResourceObject;

    List<GameObject> Hangar_TractorClones = new List<GameObject>(); // (������)Ŭ�� Ʈ���� �̹������� ���� �ִ� ����Ʈ
    List<GameObject> Hangar_GreenClones = new List<GameObject>(); // (������)Ŭ�� Green �׷� �̹������� ���� �ִ� ����Ʈ
    #endregion

    void Start()
    {
        DatasObject = GameObject.Find("DataManagement"); //.gameObject;
        AllDatas = DatasObject.GetComponent<DataManagement>();
        AircraftsLocationOnDeck = AllDatas.Area_management;

        HangarArea[0] = "H1"; HangarArea[1] = "H2"; HangarArea[2] = "H3"; HangarArea[3] = "H4"; HangarArea[4] = "H5";
        HangarArea[5] = "H6"; HangarArea[6] = "H7"; HangarArea[7] = "H8"; HangarArea[8] = "H9"; HangarArea[9] = "H10";

        /// �ּ� ī��
        AircraftsLocationOnHangar = DatasObject.GetComponent<DataManagement>().HangarArea_management; // DataManager ������Ʈ�� ���� �ִ� DataManagement ��ũ��Ʈ�� Area_management��� ���� ServiceAreaManagement ��ũ��Ʈ�� �����ðž�
        AircraftsLocationOnHangar.SetHangarArea(HangarArea.Count);
        /// �ּ� ī��

        AllFleets = AllDatas.AllFleets;
        DeckFleets = AllDatas.DeckFleets;
        HangarFleets = AllFleets - DeckFleets;

        HangarFleetsfromDeck.text = Convert.ToString(AllFleets - DeckFleets);

        MakeHangarFleets();
    }

    public void MakeHangarFleets() 
    {
        Hangar_Fleets.ClearOptions();

        List<string> hangarfleetlist = new List<string>(); // ���������� ���ö����� �ʱ�ȭ�Ǵ��� �ѹ� �� Ȯ��

        if (HangarFleets > 0) // HangarFleets = AllFleets - DeckFleets -> �ִ� HangarFleets = 2
        {
            if (DeckFleets == 3) // DeckFleets = ���఩�ǿ� �ִ� ��� �� -> DeckFleets = 3�̸� �� 10��
            {
                // HangarFleets = 0 or 1 or 2
                for (int i = 0; i < HangarFleets + 1; i++) 
                {
                    hangarfleetlist.Add(string.Format("Fleet #{0}", 3 + i)); // Fleet#3,4,5
                }
                Hangar_Fleets.AddOptions(hangarfleetlist);
            }

            else // DeckFleets : 1 or 2 (0�� X:���఩�� ���� ��� 1���� �־�� ��)
            {
                for (int i = 1; i < HangarFleets+1; i++)
                {
                    hangarfleetlist.Add(string.Format("Fleet #{0}", DeckFleets + i));
                }
                Hangar_Fleets.AddOptions(hangarfleetlist);
            }
        }

        else if (HangarFleets == 0) // HangarFleets == 0 : ���񱸿��� ����� �ʿ� ����!
        {
            if (DeckFleets == 3) // ������ ���఩���� Fleet#3�̶�� ���񱸿��� 2��� �ʿ��ϣp (���఩�� 2��, ���񱸿� 2��)
            {
                hangarfleetlist.Add("Fleet #3"); //F35B#9,10
                Hangar_Fleets.AddOptions(hangarfleetlist);
            }
        }
    }

    // 220902 22:11 �������� Ȯ��
    public void MakeHangarF35Bs() // ���񱸿� F35B �ɼ� �����
    {
        Hangar_Fleets.interactable = false; // [���񱸿�] ��� ���� ��, �� ���� �Ұ�

        Hangar_F35B.ClearOptions();

        string GetFleetName = Hangar_Fleets.options[Hangar_Fleets.value].text; // GetFleetName = "Fleet #numbers"
        string[] GetValue = GetFleetName.Split('#');

        HangarFleetValue = int.Parse(GetValue[1]); // HangarFleetValue = ��� ��ȣ

        List<string> F35Bnamelist = new List<string>();

        if (DeckFleets == 3 && HangarFleetValue == 3)
        {
            for (int i = 11; i < 13; i++)
            {
                F35Bnamelist.Add(string.Format("F35B#{0}", i));
            }
            Hangar_F35B.AddOptions(F35Bnamelist);
        }

        else
        {
            for (int i = 1; i < 5; i++)
            {
                F35Bnamelist.Add(string.Format("F35B#{0}", 4*(HangarFleetValue-1)+i));
            }
            Hangar_F35B.AddOptions(F35Bnamelist);
        }
    }

    public void MakeHangarArea() // ����� Ŭ���ϸ���~
    {
        Hangar_Area.options.Clear();

        List<int> Result = AircraftsLocationOnHangar.GetEmptyList();
        foreach (var item in Result)
        {
            Dropdown.OptionData AreaOptions = new Dropdown.OptionData();
            AreaOptions.text = HangarArea[item];
            Hangar_Area.options.Add(AreaOptions);
        }
        Hangar_Area.SetValueWithoutNotify(-1);
    }

    public void GetHangarAircraftImage()
    {
        #region ���� ���� �̹��� �� �ؽ�Ʈ ����
        if (Hangar_Area.options[Hangar_Area.value].text == "H1")
        {
            HangarAreaImages[0].SetActive(true);
            HangarAreaTexts[0].text = Hangar_F35B.options[Hangar_F35B.value].text;
        }

        else if (Hangar_Area.options[Hangar_Area.value].text == "H2")
        {
            HangarAreaImages[1].SetActive(true);
            HangarAreaTexts[1].text = Hangar_F35B.options[Hangar_F35B.value].text;
        }

        else if (Hangar_Area.options[Hangar_Area.value].text == "H3")
        {
            HangarAreaImages[2].SetActive(true);
            HangarAreaTexts[2].text = Hangar_F35B.options[Hangar_F35B.value].text;
        }

        else if (Hangar_Area.options[Hangar_Area.value].text == "H4")
        {
            HangarAreaImages[3].SetActive(true);
            HangarAreaTexts[3].text = Hangar_F35B.options[Hangar_F35B.value].text;
        }

        else if (Hangar_Area.options[Hangar_Area.value].text == "H5")
        {
            HangarAreaImages[4].SetActive(true);
            HangarAreaTexts[4].text = Hangar_F35B.options[Hangar_F35B.value].text;
        }

        else if (Hangar_Area.options[Hangar_Area.value].text == "H6")
        {
            HangarAreaImages[5].SetActive(true);
            HangarAreaTexts[5].text = Hangar_F35B.options[Hangar_F35B.value].text;
        }

        else if (Hangar_Area.options[Hangar_Area.value].text == "H7")
        {
            HangarAreaImages[6].SetActive(true);
            HangarAreaTexts[6].text = Hangar_F35B.options[Hangar_F35B.value].text;
        }

        else if (Hangar_Area.options[Hangar_Area.value].text == "H8")
        {
            HangarAreaImages[7].SetActive(true);
            HangarAreaTexts[7].text = Hangar_F35B.options[Hangar_F35B.value].text;
        }

        else if (Hangar_Area.options[Hangar_Area.value].text == "H9")
        {
            HangarAreaImages[8].SetActive(true);
            HangarAreaTexts[8].text = Hangar_F35B.options[Hangar_F35B.value].text;
        }

        else if (Hangar_Area.options[Hangar_Area.value].text == "H10")
        {
            HangarAreaImages[9].SetActive(true);
            HangarAreaTexts[9].text = Hangar_F35B.options[Hangar_F35B.value].text;
        }
        #endregion

        int H_index = HangarAreaList.IndexOf(Hangar_Area.options[Hangar_Area.value].text);
        AircraftsLocationOnHangar.AddInfo(H_index, Hangar_Area.options[Hangar_Area.value].text, Hangar_F35B.options[Hangar_F35B.value].text);
        MakeHangarArea();

        FleetOptionManage();// ��� ��ư�� ���õ� �Լ�

        Hangar_F35B.options.Remove(Hangar_F35B.options[Hangar_F35B.value]);
        Hangar_F35B.SetValueWithoutNotify(-1); // �̰� ���� �ȵȴ�
    }

    public void FleetOptionManage()
    {
        HowmanyF35BinHangar.Add(Hangar_F35B.value); // HowmanyF35BinHangar ����Ʈ�� �Ȱ��� ���鵵 �ߺ����� ó������ �ʰ�, �� ���°� Ȯ�� �Ϸ�

        if (DeckFleets == 3 && HangarFleetValue == 3) // ���఩�ǵ�, ���񱸿��� Fleet #3�̸� -> ���఩�ǿ� 2��, ���񱸿��� 2��
        {
            if (HowmanyF35BinHangar.Count == 2) // ���఩�ǿ� ����� 2���
            {
                Hangar_Fleets.options.Remove(Hangar_Fleets.options[Hangar_Fleets.value]); // Fleet #3 ��� �����
                Hangar_Fleets.SetValueWithoutNotify(-1);

                if (Hangar_Fleets.options.Count == 0) // �갡 ������ ���̸�
                {
                    HangarF35BButtons[0].SetActive(false);
                }

                else
                {
                    Hangar_Fleets.interactable = true;
                    HangarF35BButtons[0].SetActive(true); // �ٸ� ��� �ɼ� �����ؾ� �ϴϱ�!
                }

                HowmanyF35BinHangar.Clear();
            }

            else // (����) ������, �ش� ����� �������� ��� �������� ���� ���
            {
                Hangar_Fleets.interactable = false;
                HangarF35BButtons[0].SetActive(false);
            }
        }

        else // Fleet #1,2 or Fleet #3 or Fleet #4,5 �� ���
        {
            if (HowmanyF35BinHangar.Count == 4)
            {
                Hangar_Fleets.options.Remove(Hangar_Fleets.options[Hangar_Fleets.value]); // ������ ��뿡 �ִ� ������ �� ���������ϱ� ��������
                Hangar_Fleets.SetValueWithoutNotify(-1);

                if (Hangar_Fleets.options.Count == 0)
                {
                    HangarF35BButtons[0].SetActive(false);
                }

                else
                {
                    Hangar_Fleets.interactable = true;
                    HangarF35BButtons[0].SetActive(true);
                }

                HowmanyF35BinHangar.Clear();
            }

            else // (����) ������, �ش� ����� �������� ��� �������� ���� ���
            {
                Hangar_Fleets.interactable = false;
                HangarF35BButtons[0].SetActive(false);
            }
        }
    }

    #region F35B ���� ��ư�� ������� ��Ÿ���� ���� �Լ���
    //public GameObject[] HangarF35BButtons = new GameObject[4]; // Fleet ��ư, F35B ��ư, Select ��ư, Save F35B's Info ��ư ������
    public void FleetButton()
    {
        List<int> Fleets = new List<int>(); // ���� ������ ���ö����� �ʱ�ȭ �Ǵϱ�

        Fleets.Add(Hangar_Fleets.value);

        if (Fleets.Count == 1)
        {
            HangarF35BButtons[0].SetActive(false); // Fleet ���� ��ư ��Ȱ��ȭ
        }
    }

    public void F35BButton() // F35B# ����, ��ư ������ ����� ���� ��ư �׾�
    {
        List<int> F35Bs = new List<int>(); // F35B ����Ⱑ �ϳ��� ���õ� ������

        F35Bs.Add(Hangar_F35B.value);

        if (F35Bs.Count == 1)
        {
            HangarF35BButtons[1].SetActive(false);
        }
    }

    //public GameObject[] HangarF35BButtons = new GameObject[4]; // Fleet ��ư, F35B ��ư, Select ��ư, Save F35B's Info ��ư ������
    public void F35BSelectButton() // Select ��ư ������ �� ��Ƴ�
    {
        if (Hangar_Fleets.options.Count == 0 && Hangar_F35B.options.Count == 0)
        {
            HangarF35BButtons[1].SetActive(false); // F35B ��ư
            HangarF35BButtons[2].SetActive(false); // Select ��ư
            HangarF35BButtons[3].SetActive(true); // Save F35B's Info ��ư
        }

        else
        {
            HangarF35BButtons[1].SetActive(true); // Hangar_Fleets.options.Count != 0 �̴ϱ� �ڲ� ����°�!!
            //HangarF35BButtons[2].SetActive(true);
        }
    }
    #endregion

    public void HangarF35BClear() // F35BInitialization() ã��!!
    {
        Hangar_Fleets.ClearOptions();
        MakeHangarFleets(); // ����� ��Ӵٿ� �׸���� �ٲ�� �ȵǴϱ�
        Hangar_Fleets.interactable = true; // �� �� ���⸦ �������� ����� ������ �����ؾ��ϴϱ�

        Hangar_F35B.ClearOptions();
        Hangar_Area.ClearOptions();

        for (int i = 0; i < HangarAreaImages.Length; i++)
        {
            HangarAreaImages[i].SetActive(false);
        }

        for (int i = 0; i < HangarAreaTexts.Length; i++)
        {
            HangarAreaTexts[i].text = default; // ������ ���·� �δ°�����
            //HangarAreaTexts[i].text = null;
        }
        Start(); // ���� ���� ������ �־����

        HangarF35BButtons[0].SetActive(true); // Fleet ���� ��ư
        HangarF35BButtons[1].SetActive(true); // F35B ���� ��ư
        HangarF35BButtons[2].SetActive(true); // Select ��ư
        HangarF35BButtons[3].SetActive(false); // Save F35B's Info ��ư
    }

    public void SaveF35BInfo() // ����� ���� �����Ѱ� ����� �Է� �Ұ����ϰ� �������(���� �ؾ��ϳ�..?) & �ٵ� Clear ��ư�� ���ܵ���!
    {

    }

    #region ���񱸿� Resource ����
    public void Resource_Tractor()
    {
        AllDatas.Hangar_Trators = int.Parse(Hangar_TractorValue.text);
        DestroyResourceImage(Hangar_TractorClones);
        Hangar_TractorClones.Clear();

        if (AllDatas.Hangar_Trators == 0)
        {
            Hangar_TractorImage.SetActive(false);
            Debug.Log("1�� �̻��� Ʈ���Ͱ� �ʿ��մϴ�.");
            Hangar_TractorValue.text = null;
        }

        else if (AllDatas.Hangar_Trators > 4)
        {
            Hangar_TractorImage.SetActive(false);
            Debug.Log(string.Format("{0}�� �̻��� Ʈ���ʹ� ����� �� �����ϴ�.", AllDatas.Hangar_Trators));
            Hangar_TractorValue.text = null;
        }

        else
        {
            Hangar_TractorValue.readOnly = true;
            Hangar_TractorImage.SetActive(true);

            List<Vector2> TractorLocation = new List<Vector2>() { new Vector2(1090, 350), new Vector2(1120, 350), new Vector2(1150, 350) };

            for (int i = 0; i < AllDatas.Hangar_Trators - 1; i++)
            {
                GameObject TCloneindex = Instantiate(Hangar_TractorImage, new Vector3(TractorLocation[i].x, TractorLocation[i].y), Quaternion.identity);
                TCloneindex.transform.parent = EmptyResourceObject.transform;

                Hangar_TractorClones.Add(TCloneindex);
            }
        }
    }

    public void Resource_GreenGroup()
    {
        AllDatas.Hangar_GreenGroup = int.Parse(Hangar_GreenValue.text);
        DestroyResourceImage(Hangar_GreenClones);
        Hangar_GreenClones.Clear();

        if (AllDatas.Hangar_GreenGroup == 0)
        {
            Hangar_GreenImage.SetActive(false);
            Debug.Log("1�� �̻��� Red �ο��� �ʿ��մϴ�.");
            Hangar_GreenValue.text = null;
        }

        else if (AllDatas.Hangar_GreenGroup > 5)
        {
            Hangar_GreenImage.SetActive(false);
            Debug.Log(string.Format("{0}�� �̻��� Red ���� �ڿ��� ����� �� �����ϴ�.", AllDatas.Hangar_GreenGroup));
            Hangar_GreenValue.text = null;
        }

        else
        {
            Hangar_GreenValue.readOnly = true;
            Hangar_GreenImage.SetActive(true);

            List<Vector2> GreenLocation = new List<Vector2>() { new Vector2(1355, 370), new Vector2(1375, 370), new Vector2(1395, 370), new Vector2(1415, 370) };

            for (int i = 0; i < AllDatas.Hangar_GreenGroup - 1; i++)
            {
                GameObject GCloneindex = Instantiate(Hangar_GreenImage, new Vector3(GreenLocation[i].x, GreenLocation[i].y), Quaternion.identity);
                GCloneindex.transform.parent = EmptyResourceObject.transform;

                Hangar_GreenClones.Add(GCloneindex);
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

    public void ResourceInitialization() // Hangar Resource�� ���� ���� ��� Reset
    {
        Hangar_TractorValue.text = null;
        Hangar_GreenValue.text = null;

        Hangar_TractorValue.readOnly = false;
        Hangar_GreenValue.readOnly = false;

        Hangar_TractorImage.SetActive(false);
        DestroyResourceImage(Hangar_TractorClones);

        Hangar_GreenImage.SetActive(false);
        DestroyResourceImage(Hangar_GreenClones);
    }
    #endregion

    #region ���������� ���� �Լ�
    public void D1Elevator()
    {
        D1_ElevatorValues[0].readOnly = true;
        D1_ElevatorValues[1].readOnly = true;

        string D1Velo = string.Format("{0}.{1}", D1_ElevatorValues[0].text, D1_ElevatorValues[1].text);
        AllDatas.D1_Velocity = double.Parse(D1Velo, CultureInfo.InvariantCulture);
        ElevatorTexts[0].text = string.Format("D1 �ӵ� : {0} m/s", AllDatas.D1_Velocity);
    }

    public void D2Elevator()
    {
        D2_ElevatorValues[0].readOnly = true;
        D2_ElevatorValues[1].readOnly = true;

        string D2Velo = string.Format("{0}.{1}", D2_ElevatorValues[0].text, D2_ElevatorValues[1].text);
        AllDatas.D2_Velocity = double.Parse(D2Velo, CultureInfo.InvariantCulture);
        ElevatorTexts[1].text = string.Format("D2 �ӵ� : {0} m/s", AllDatas.D2_Velocity);
    }

    public void ElevatorInitialization()
    {
        D1_ElevatorValues[0].text = null;
        D1_ElevatorValues[1].text = null;
        D1_ElevatorValues[0].readOnly = false;
        D1_ElevatorValues[1].readOnly = false;

        D2_ElevatorValues[0].text = null;
        D2_ElevatorValues[1].text = null;
        D2_ElevatorValues[0].readOnly = false;
        D2_ElevatorValues[1].readOnly = false;

        ElevatorTexts[0].text = null;
        ElevatorTexts[1].text = null;
    }
    #endregion

    public void Alone_ToSocketScene() // �ܵ� ��忡���� ���� ������ �̵� "SimulationButton : Simulation Start >>"
    {
        SceneManager.LoadScene("Alone_Socket");
        DontDestroyOnLoad(AllDatas.AllDataManager);
    }

    public void Inte_ToSocketScene() // ���� ��忡���� ���� ������ �̵� "SimulationButton : Simulation Start >>"
    {
        SceneManager.LoadScene("Inte_Socket");
        DontDestroyOnLoad(AllDatas.AllDataManager);
    }
}
