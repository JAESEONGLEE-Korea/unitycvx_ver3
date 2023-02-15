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

    public GameObject[] HangarF35BButtons = new GameObject[4]; // Fleet 버튼, F35B 버튼, Select 버튼, Save F35B's Info 버튼 순으로
    List<int> HowmanyF35BinHangar = new List<int>(); // Fleet 버튼 비활성화
    int HangarFleetValue; // 몇 번 Fleet인지 확인 & Fleet 숫자에 따라 F35B 이름 생성

    public InputField[] D1_ElevatorValues; // = new InputField[2];
    public InputField[] D2_ElevatorValues;

    public GameObject[] HangarAreaImages = new GameObject[10];
    public Text[] HangarAreaTexts = new Text[10];
    private List<string> HangarAreaList = new List<string> { "H1", "H2", "H3", "H4", "H5", "H6", "H7", "H8", "H9", "H10" };
    private Dictionary<int, string> HangarArea = new Dictionary<int, string>();
    public Text[] ElevatorTexts = new Text[2];


    #region 정비구역 Resource 관련 변수
    public InputField Hangar_TractorValue;
    public InputField Hangar_GreenValue;

    public GameObject Hangar_TractorImage;
    public GameObject Hangar_GreenImage;

    public GameObject EmptyResourceObject;

    List<GameObject> Hangar_TractorClones = new List<GameObject>(); // (복제된)클론 트랙터 이미지들을 갖고 있는 리스트
    List<GameObject> Hangar_GreenClones = new List<GameObject>(); // (복제된)클론 Green 그룹 이미지들을 갖고 있는 리스트
    #endregion

    void Start()
    {
        DatasObject = GameObject.Find("DataManagement"); //.gameObject;
        AllDatas = DatasObject.GetComponent<DataManagement>();
        AircraftsLocationOnDeck = AllDatas.Area_management;

        HangarArea[0] = "H1"; HangarArea[1] = "H2"; HangarArea[2] = "H3"; HangarArea[3] = "H4"; HangarArea[4] = "H5";
        HangarArea[5] = "H6"; HangarArea[6] = "H7"; HangarArea[7] = "H8"; HangarArea[8] = "H9"; HangarArea[9] = "H10";

        /// 주소 카피
        AircraftsLocationOnHangar = DatasObject.GetComponent<DataManagement>().HangarArea_management; // DataManager 오브젝트가 갖고 있는 DataManagement 스크립트의 Area_management라고 명명된 ServiceAreaManagement 스크립트를 가져올거야
        AircraftsLocationOnHangar.SetHangarArea(HangarArea.Count);
        /// 주소 카피

        AllFleets = AllDatas.AllFleets;
        DeckFleets = AllDatas.DeckFleets;
        HangarFleets = AllFleets - DeckFleets;

        HangarFleetsfromDeck.text = Convert.ToString(AllFleets - DeckFleets);

        MakeHangarFleets();
    }

    public void MakeHangarFleets() 
    {
        Hangar_Fleets.ClearOptions();

        List<string> hangarfleetlist = new List<string>(); // 지역변수는 들어올때마다 초기화되는지 한번 더 확인

        if (HangarFleets > 0) // HangarFleets = AllFleets - DeckFleets -> 최대 HangarFleets = 2
        {
            if (DeckFleets == 3) // DeckFleets = 비행갑판에 있는 편대 수 -> DeckFleets = 3이면 총 10대
            {
                // HangarFleets = 0 or 1 or 2
                for (int i = 0; i < HangarFleets + 1; i++) 
                {
                    hangarfleetlist.Add(string.Format("Fleet #{0}", 3 + i)); // Fleet#3,4,5
                }
                Hangar_Fleets.AddOptions(hangarfleetlist);
            }

            else // DeckFleets : 1 or 2 (0대 X:비행갑판 위에 적어도 1편대는 있어야 함)
            {
                for (int i = 1; i < HangarFleets+1; i++)
                {
                    hangarfleetlist.Add(string.Format("Fleet #{0}", DeckFleets + i));
                }
                Hangar_Fleets.AddOptions(hangarfleetlist);
            }
        }

        else if (HangarFleets == 0) // HangarFleets == 0 : 정비구역엔 함재기 필요 없따!
        {
            if (DeckFleets == 3) // 하지만 비행갑판의 Fleet#3이라면 정비구역에 2대는 필요하즤 (비행갑판 2대, 정비구역 2대)
            {
                hangarfleetlist.Add("Fleet #3"); //F35B#9,10
                Hangar_Fleets.AddOptions(hangarfleetlist);
            }
        }
    }

    // 220902 22:11 문제없음 확인
    public void MakeHangarF35Bs() // 정비구역 F35B 옵션 만들기
    {
        Hangar_Fleets.interactable = false; // [정비구역] 편대 선택 후, 값 수정 불가

        Hangar_F35B.ClearOptions();

        string GetFleetName = Hangar_Fleets.options[Hangar_Fleets.value].text; // GetFleetName = "Fleet #numbers"
        string[] GetValue = GetFleetName.Split('#');

        HangarFleetValue = int.Parse(GetValue[1]); // HangarFleetValue = 편대 번호

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

    public void MakeHangarArea() // 함재기 클릭하면은~
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
        #region 정비 구역 이미지 및 텍스트 등장
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

        FleetOptionManage();// 편대 버튼과 관련된 함수

        Hangar_F35B.options.Remove(Hangar_F35B.options[Hangar_F35B.value]);
        Hangar_F35B.SetValueWithoutNotify(-1); // 이거 없음 안된다
    }

    public void FleetOptionManage()
    {
        HowmanyF35BinHangar.Add(Hangar_F35B.value); // HowmanyF35BinHangar 리스트는 똑같은 값들도 중복으로 처리되지 않고, 잘 담기는거 확인 완료

        if (DeckFleets == 3 && HangarFleetValue == 3) // 비행갑판도, 정비구역도 Fleet #3이면 -> 비행갑판에 2대, 정비구역에 2대
        {
            if (HowmanyF35BinHangar.Count == 2) // 비행갑판에 함재기 2대면
            {
                Hangar_Fleets.options.Remove(Hangar_Fleets.options[Hangar_Fleets.value]); // Fleet #3 편대 지우기
                Hangar_Fleets.SetValueWithoutNotify(-1);

                if (Hangar_Fleets.options.Count == 0) // 얘가 마지막 값이면
                {
                    HangarF35BButtons[0].SetActive(false);
                }

                else
                {
                    Hangar_Fleets.interactable = true;
                    HangarF35BButtons[0].SetActive(true); // 다른 편대 옵션 선택해야 하니까!
                }

                HowmanyF35BinHangar.Clear();
            }

            else // (아직) 선택한, 해당 편대의 함재기들을 모두 선택하지 않은 경우
            {
                Hangar_Fleets.interactable = false;
                HangarF35BButtons[0].SetActive(false);
            }
        }

        else // Fleet #1,2 or Fleet #3 or Fleet #4,5 인 경우
        {
            if (HowmanyF35BinHangar.Count == 4)
            {
                Hangar_Fleets.options.Remove(Hangar_Fleets.options[Hangar_Fleets.value]); // 선택한 편대에 있는 함재기들 다 선택했으니까 지워야지
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

            else // (아직) 선택한, 해당 편대의 함재기들을 모두 선택하지 않은 경우
            {
                Hangar_Fleets.interactable = false;
                HangarF35BButtons[0].SetActive(false);
            }
        }
    }

    #region F35B 관련 버튼들 사라졌다 나타났다 관리 함수들
    //public GameObject[] HangarF35BButtons = new GameObject[4]; // Fleet 버튼, F35B 버튼, Select 버튼, Save F35B's Info 버튼 순으로
    public void FleetButton()
    {
        List<int> Fleets = new List<int>(); // 지역 변수는 들어올때마다 초기화 되니까

        Fleets.Add(Hangar_Fleets.value);

        if (Fleets.Count == 1)
        {
            HangarF35BButtons[0].SetActive(false); // Fleet 선택 버튼 비활성화
        }
    }

    public void F35BButton() // F35B# 고르고, 버튼 누르면 함재기 선택 버튼 죽어
    {
        List<int> F35Bs = new List<int>(); // F35B 함재기가 하나씩 선택될 때마다

        F35Bs.Add(Hangar_F35B.value);

        if (F35Bs.Count == 1)
        {
            HangarF35BButtons[1].SetActive(false);
        }
    }

    //public GameObject[] HangarF35BButtons = new GameObject[4]; // Fleet 버튼, F35B 버튼, Select 버튼, Save F35B's Info 버튼 순으로
    public void F35BSelectButton() // Select 버튼 누르면 다 살아남
    {
        if (Hangar_Fleets.options.Count == 0 && Hangar_F35B.options.Count == 0)
        {
            HangarF35BButtons[1].SetActive(false); // F35B 버튼
            HangarF35BButtons[2].SetActive(false); // Select 버튼
            HangarF35BButtons[3].SetActive(true); // Save F35B's Info 버튼
        }

        else
        {
            HangarF35BButtons[1].SetActive(true); // Hangar_Fleets.options.Count != 0 이니까 자꾸 생기는거!!
            //HangarF35BButtons[2].SetActive(true);
        }
    }
    #endregion

    public void HangarF35BClear() // F35BInitialization() 찾기!!
    {
        Hangar_Fleets.ClearOptions();
        MakeHangarFleets(); // 편대의 드롭다운 항목들은 바뀌면 안되니까
        Hangar_Fleets.interactable = true; // 몇 번 편대기를 선택할지 사용자 선택이 가능해야하니까

        Hangar_F35B.ClearOptions();
        Hangar_Area.ClearOptions();

        for (int i = 0; i < HangarAreaImages.Length; i++)
        {
            HangarAreaImages[i].SetActive(false);
        }

        for (int i = 0; i < HangarAreaTexts.Length; i++)
        {
            HangarAreaTexts[i].text = default; // 원래의 상태로 두는가보다
            //HangarAreaTexts[i].text = null;
        }
        Start(); // 정비 구역 때문에 있어야함

        HangarF35BButtons[0].SetActive(true); // Fleet 선택 버튼
        HangarF35BButtons[1].SetActive(true); // F35B 선택 버튼
        HangarF35BButtons[2].SetActive(true); // Select 버튼
        HangarF35BButtons[3].SetActive(false); // Save F35B's Info 버튼
    }

    public void SaveF35BInfo() // 함재기 정보 관련한거 사용자 입력 불가능하게 만들려고(굳이 해야하나..?) & 근데 Clear 버튼은 남겨두자!
    {

    }

    #region 정비구역 Resource 관련
    public void Resource_Tractor()
    {
        AllDatas.Hangar_Trators = int.Parse(Hangar_TractorValue.text);
        DestroyResourceImage(Hangar_TractorClones);
        Hangar_TractorClones.Clear();

        if (AllDatas.Hangar_Trators == 0)
        {
            Hangar_TractorImage.SetActive(false);
            Debug.Log("1개 이상의 트랙터가 필요합니다.");
            Hangar_TractorValue.text = null;
        }

        else if (AllDatas.Hangar_Trators > 4)
        {
            Hangar_TractorImage.SetActive(false);
            Debug.Log(string.Format("{0}개 이상의 트랙터는 운용할 수 없습니다.", AllDatas.Hangar_Trators));
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
            Debug.Log("1명 이상의 Red 인원이 필요합니다.");
            Hangar_GreenValue.text = null;
        }

        else if (AllDatas.Hangar_GreenGroup > 5)
        {
            Hangar_GreenImage.SetActive(false);
            Debug.Log(string.Format("{0}명 이상의 Red 인적 자원은 운용할 수 없습니다.", AllDatas.Hangar_GreenGroup));
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

    public void ResourceInitialization() // Hangar Resource에 관한 정보 모두 Reset
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

    #region 엘리베이터 관련 함수
    public void D1Elevator()
    {
        D1_ElevatorValues[0].readOnly = true;
        D1_ElevatorValues[1].readOnly = true;

        string D1Velo = string.Format("{0}.{1}", D1_ElevatorValues[0].text, D1_ElevatorValues[1].text);
        AllDatas.D1_Velocity = double.Parse(D1Velo, CultureInfo.InvariantCulture);
        ElevatorTexts[0].text = string.Format("D1 속도 : {0} m/s", AllDatas.D1_Velocity);
    }

    public void D2Elevator()
    {
        D2_ElevatorValues[0].readOnly = true;
        D2_ElevatorValues[1].readOnly = true;

        string D2Velo = string.Format("{0}.{1}", D2_ElevatorValues[0].text, D2_ElevatorValues[1].text);
        AllDatas.D2_Velocity = double.Parse(D2Velo, CultureInfo.InvariantCulture);
        ElevatorTexts[1].text = string.Format("D2 속도 : {0} m/s", AllDatas.D2_Velocity);
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

    public void Alone_ToSocketScene() // 단독 모드에서의 소켓 씬으로 이동 "SimulationButton : Simulation Start >>"
    {
        SceneManager.LoadScene("Alone_Socket");
        DontDestroyOnLoad(AllDatas.AllDataManager);
    }

    public void Inte_ToSocketScene() // 통합 모드에서의 소켓 씬으로 이동 "SimulationButton : Simulation Start >>"
    {
        SceneManager.LoadScene("Inte_Socket");
        DontDestroyOnLoad(AllDatas.AllDataManager);
    }
}
