using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeckOptions : MonoBehaviour
{
    ServiceAreaManagement AircraftsLocationOnDeck;
    DataManagement DeckData; // 행거에서 사용할 수 있도록, 비행갑판의 정보를 담아주는 역할

    public InputField ExternalPort;
    public Text InternalPort; // 220903, InputField -> Text로 바꾸기
    public GameObject ClearPortButton;

    public InputField All_Fleets;
    public InputField Deck_Fleet;

    public Dropdown Dropdown_Fleet;
    public Dropdown Dropdown_F35B;
    public Dropdown Dropdown_Posture;
    public Dropdown Dropdown_Area;

    public GameObject[] InitF35BButtons = new GameObject[5]; // Fleet 버튼, F35B 버튼, Posture 버튼, Select 버튼, Save F35B's Info 버튼 순으로
    List<int> HowmanyF35B = new List<int>(); // Fleet 버튼 비활성화
    int FleetValue; // 몇 번 Fleet인지 확인 & Fleet 숫자에 따라 F35B 이름 생성

    private List<string> ServiceAreaList = new List<string> { "A1", "A2", "A3", "A4", "A5", "B1", "B2", "B3", "B4", "B5" };

    private Dictionary<int, string> ServiceArea = new Dictionary<int, string>();
    private List<string> Postures = new List<string> { "45 (deg)", "-45 (deg)" };

    public Sprite[] Aircrafts;
    public GameObject[] GameobjectsforArea = new GameObject[20];
    public Text[] TextsforArea = new Text[10];
    List<int> CheckingBrowns = new List<int>();

    #region 비행갑판 Resource
    public InputField TractorValue;
    public InputField RedValue;
    public Text BrownValue; // Brown 인원 수 = 함재기 개수
    public InputField PurpleValue;
    public InputField GreenValue;
    public GameObject TractorImage;
    public GameObject RedImage;
    public GameObject BrownImage;
    public GameObject PurpleImage;
    public GameObject GreenImage;
    public GameObject EmptyResourceObject;

    // 리소스 이미지들 복제 및 삭제
    List<GameObject> TractorClones = new List<GameObject>();
    List<GameObject> RedClones = new List<GameObject>();
    List<GameObject> BrownClones = new List<GameObject>();
    List<GameObject> PurpleClones = new List<GameObject>();
    List<GameObject> GreenClones = new List<GameObject>();
    #endregion

    /// <summary>
    /// 1. Dropdown.AddOptions(List<string>) : Dropdown.SetValueWithoutNotify(-1)를 사용하지 않아도, 드롭다운 옵션의 가장 첫번째 값을 드롭다운 창에 보여준다
    /// 2. Dropdown.options.Add(Dropdown.OptionData) : Dropdown.SetValueWithoutNotify(-1)를 사용하지 않으면, 처음 드롭다운 창에 옵션이 보이지 않는다. 그리고 처음 시작할 때, 첫번째 값 사용도 못함
    /// => 2-1. Dropdown.SetValueWithoutNotify(-1)를 사용X = Dropdown.SetValueWithoutNotify(0);을 사용하거나 or Dropdown.SetValueWithoutNotify()를 아예 사용하지 않은 것
    /// 3. F35B이름의 드롭다운 생성 과정(MakeF35BDropdown()) : Dropdown_F35B.AddOptions(List<string>)이지만, Dropdown.options.Remove(Dropdown.OptionData) 때문에 Dropdown_F35B.SetValueWithoutNotify(-1) 사용!
    /// 즉, Dropdown.SetValueWithoutNotify(-1) : 드롭다운의 선택 제한을 풀어줌 ㅇㅇ, 어디서? Dropdown.options.Add(Dropdown.OptionData)으로 구성된 드롭다운을 ㅇㅇ <-> Dropdown.AddOptions(List<string>)
    /// </summary>

    private void Start()
    {
        ServiceArea[0] = "A1"; ServiceArea[1] = "A2"; ServiceArea[2] = "A3"; ServiceArea[3] = "A4"; ServiceArea[4] = "A5";
        ServiceArea[5] = "B1"; ServiceArea[6] = "B2"; ServiceArea[7] = "B3"; ServiceArea[8] = "B4"; ServiceArea[9] = "B5";

        /// 주소 카피
        GameObject DataManager = GameObject.Find("DataManagement").gameObject; // 유니티 하이어라키 창의 "DataManagement"라는 이름의 GameObject를 찾기 위해
        AircraftsLocationOnDeck = DataManager.GetComponent<DataManagement>().Area_management; // DataManager 오브젝트가 갖고 있는 DataManagement 스크립트의 Area_management라고 명명된 ServiceAreaManagement 스크립트를 가져올거야
        AircraftsLocationOnDeck.SetServiceArea(ServiceArea.Count);
        /// 주소 카피

        DeckData = DataManager.GetComponent<DataManagement>(); //DeckData = new DataManagement(); -> 이렇게 해버리면 값을 지정해줘도 씬이 바뀌면 지정한 값을 가져가지 못함
        DeckData.Internal_PortNumber = int.Parse(InternalPort.text);
    }

    #region 외부 포트 번호 저장
    public void ExPortNumber()
    {
        DeckData.External_PortNumber = int.Parse(ExternalPort.text);
        ExternalPort.readOnly = true; // 이제 입력 불가능

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
            Debug.Log(string.Format("{0}편대 이상의 편대는 운용할 수 없습니다!", DeckData.AllFleets));
            All_Fleets.text = null;
        }

        else
        {
            All_Fleets.readOnly = true; // 사용자가 1~5 사이의 편대를 선택하면 Clear 버튼 누르기 전엔 수정 X
        }
    }

    public void FleetsofDeck()
    {
        DeckData.DeckFleets = int.Parse(Deck_Fleet.text);
        int Hangar_Fleets_Numbers = DeckData.AllFleets - DeckData.DeckFleets;

        if (Hangar_Fleets_Numbers > 2)
        {
            Deck_Fleet.text = null;
            Debug.Log(string.Format("정비구역에 {0}편대 이상의 함재기를 배치할 수 없습니다!", Hangar_Fleets_Numbers));
        }

        else
        {
            if (DeckData.DeckFleets == 0 || DeckData.DeckFleets >= 4)
            {
                Debug.Log(string.Format("비행갑판에 {0}편대는 운용이 불가합니다.", DeckData.DeckFleets));
                Deck_Fleet.text = null;
            }

            else if (DeckData.DeckFleets > DeckData.AllFleets)
            {
                Debug.Log(string.Format("지정한 편대({0}편대)보다 비행갑판의 편대({1}편대)가 더 많습니다!", DeckData.AllFleets, DeckData.DeckFleets));
                Deck_Fleet.text = null;
            }

            else
            {
                Debug.Log(string.Format("비행갑판 위 고정익 함재기 운용 편대 갯수 : {0}편대", DeckData.DeckFleets));
                Deck_Fleet.readOnly = true;

                MakeFleetDropdown();
            }
        }
    }

    public void MakeFleetDropdown() // 편대 옵션 만들기
    {
        Dropdown_Fleet.ClearOptions();

        List<string> fleetlist = new List<string>();

        for (int i = 0; i < DeckData.DeckFleets; i++)
        {
            fleetlist.Add(string.Format("Fleet #{0}", i + 1));
        }
        Dropdown_Fleet.AddOptions(fleetlist);
    }

    public void MakeF35BDropdown() // 비행갑판 F35B 옵션 만들기
    {
        //InitF35BButtons[0].SetActive(false); // 편대 선택 후, 바로 버튼 없애기 // 음 얘는 살리면 안돼
        Dropdown_Fleet.interactable = false; // 편대 선택 후, 값 수정 불가

        Dropdown_F35B.ClearOptions();

        string GetFleetName = Dropdown_Fleet.options[Dropdown_Fleet.value].text;
        string[] GetValue = GetFleetName.Split('#');

        FleetValue = int.Parse(GetValue[1]);

        List<string> F35Bnamelist = new List<string>();

        if (FleetValue == 3)
        {
            for (int i = 1; i < 3; i++) // Fleet#3 : 갑판에 2대, 정비구역에 2대
            {
                F35Bnamelist.Add(string.Format("F35B#{0}", 4 * (FleetValue - 1) + i));
            }
            Dropdown_F35B.AddOptions(F35Bnamelist);
        }

        else
        {
            for (int i = 1; i < 5; i++) // 1 Fleet에는 4대의 고정익 함재기 존재 -> i = 1,2,3,4
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
            Dropdown_Posture.options.Add(PostureOptions); // Add(Dropdown.OptionData) 이므로 Dropdown_Posture.SetValueWithoutNotify(-1); 사용~!
        }
        Dropdown_Posture.SetValueWithoutNotify(-1); // 드롭다운 옵션 중 제일 위에 있는 옵션 값이 드롭다운 창에 뜸
    }

    public void GetAngleImage() // 자세 선택 버튼 누르면, 함재기 각도 이미지 생성 및 서비스 구역 옵션 만들기
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

    public void MakeAreaDropdown() // Service Area 옵션 만들기 1
    {
        UpdateAreaDropdown();
    }

    public void UpdateAreaDropdown() // Service Area 옵션 만들기 2
    {
        Dropdown_Area.options.Clear();

        List<int> Result = AircraftsLocationOnDeck.GetEmptyList();
        foreach (var item in Result)
        {
            Dropdown.OptionData AreaOptions = new Dropdown.OptionData();
            AreaOptions.text = ServiceArea[item];
            Dropdown_Area.options.Add(AreaOptions);
        }
        Dropdown_Area.SetValueWithoutNotify(-1); // ** 드롭다운 옵션 중 제일 위에 있는 옵션 값이 드롭다운 창에 뜸  //Dropdown_Area.SetValueWithoutNotify(0); // 드롭다운 창에 아무것도 안 뜸
    }

    public void GetAircraftImage() // Select Button 눌렀을 때 실행되는 함수
    {
        #region 서비스 구역에 해당하는 서비스 구역 사진과 함재기 이름 텍스트
        if (Dropdown_Area.options[Dropdown_Area.value].text == "A1" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[0].SetActive(true);
            TextsforArea[0].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A1구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "A1" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[1].SetActive(true);
            TextsforArea[0].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A1구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "A2" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[2].SetActive(true);
            TextsforArea[1].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A2구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "A2" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[3].SetActive(true);
            TextsforArea[1].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A2구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "A3" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[4].SetActive(true);
            TextsforArea[2].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A3구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "A3" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[5].SetActive(true);
            TextsforArea[2].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A3구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "A4" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[6].SetActive(true);
            TextsforArea[3].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A4구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "A4" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[7].SetActive(true);
            TextsforArea[3].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A4구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "A5" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[8].SetActive(true);
            TextsforArea[4].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A5구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "A5" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[9].SetActive(true);
            TextsforArea[4].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // A5구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B1" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[10].SetActive(true);
            TextsforArea[5].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B1구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B1" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[11].SetActive(true);
            TextsforArea[5].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B1구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B2" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[12].SetActive(true);
            TextsforArea[6].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B2구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B2" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[13].SetActive(true);
            TextsforArea[6].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B2구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B3" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[14].SetActive(true);
            TextsforArea[7].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B3구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B3" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[15].SetActive(true);
            TextsforArea[7].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B3구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B4" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[16].SetActive(true);
            TextsforArea[8].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B4구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B4" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[17].SetActive(true);
            TextsforArea[8].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B4구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B5" && Dropdown_Posture.options[Dropdown_Posture.value].text == "45 (deg)")
        {
            GameobjectsforArea[18].SetActive(true);
            TextsforArea[9].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B5구역
        }

        else if (Dropdown_Area.options[Dropdown_Area.value].text == "B5" && Dropdown_Posture.options[Dropdown_Posture.value].text == "-45 (deg)")
        {
            GameobjectsforArea[19].SetActive(true);
            TextsforArea[9].text = Dropdown_F35B.options[Dropdown_F35B.value].text; // B5구역
        }
        #endregion

        int A_index = ServiceAreaList.IndexOf(Dropdown_Area.options[Dropdown_Area.value].text);
        AircraftsLocationOnDeck.AddInfo(A_index, Dropdown_Area.options[Dropdown_Area.value].text, Dropdown_F35B.options[Dropdown_F35B.value].text, Dropdown_Posture.captionImage.transform.eulerAngles.z);
        UpdateAreaDropdown();

        FleetOptionManage();

        Dropdown_F35B.options.Remove(Dropdown_F35B.options[Dropdown_F35B.value]);
        Dropdown_F35B.SetValueWithoutNotify(-1); // 이거 없음 안된다
    }

    public void FleetOptionManage()
    {
        HowmanyF35B.Add(Dropdown_F35B.value); // HowmanyF35B 리스트는 똑같은 값들도 중복으로 처리되지 않고, 잘 담기는거 확인 완료

        if (FleetValue == 3) // Fleet #3이면 -> 비행갑판에 2대, 정비구역에 2대
        {
            if (HowmanyF35B.Count == 2) // 비행갑판에 함재기 2대면
            {
                Dropdown_Fleet.options.Remove(Dropdown_Fleet.options[Dropdown_Fleet.value]); // Fleet #3 편대 지우기
                Dropdown_Fleet.SetValueWithoutNotify(-1);

                if (Dropdown_Fleet.options.Count == 0) // 얘가 마지막 값이면
                {
                    InitF35BButtons[0].SetActive(false);
                }

                else
                {
                    Dropdown_Fleet.interactable = true;
                    InitF35BButtons[0].SetActive(true); // 다른 편대 옵션 선택해야 하니까!
                }

                HowmanyF35B.Clear();
            }

            else // (아직) 선택한, 해당 편대의 함재기들을 모두 선택하지 않은 경우
            {
                Dropdown_Fleet.interactable = false;
                InitF35BButtons[0].SetActive(false);
            }
        }

        else // Fleet #1,2인 경우
        {
            if (HowmanyF35B.Count == 4)
            {
                Dropdown_Fleet.options.Remove(Dropdown_Fleet.options[Dropdown_Fleet.value]); // 선택한 편대에 있는 함재기들 다 선택했으니까 지워야지
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

            else // (아직) 선택한, 해당 편대의 함재기들을 모두 선택하지 않은 경우
            {
                Dropdown_Fleet.interactable = false;
                InitF35BButtons[0].SetActive(false);
            }
        }
    }

    #region F35B 버튼들 사라졌다 나타났다 관리 함수들
    //public GameObject[] InitF35BButtons = new GameObject[5]; // Fleet 버튼, F35B 버튼, Posture 버튼, Select 버튼, Save F35B's Info 버튼 순으로
    public void FleetButton()
    {
        List<int> Fleets = new List<int>(); // 지역 변수는 들어올때마다 초기화 되니까

        Fleets.Add(Dropdown_Fleet.value);

        if (Fleets.Count == 1)
        {
            InitF35BButtons[0].SetActive(false);
        }
    }

    public void F35BButton() // F35B# 고르고, 버튼 누르면 함재기 선택 버튼 죽어
    {
        List<int> F35Bs = new List<int>(); // F35B 함재기가 하나씩 선택될 때마다

        F35Bs.Add(Dropdown_F35B.value);

        if (F35Bs.Count == 1)
        {
            InitF35BButtons[1].SetActive(false);
            InitF35BButtons[2].SetActive(true);
        }
    }

    public void PostureButton() // 자세 고르고, 버튼 누르면 자세 선택 버튼 죽어
    {
        List<int> Postures = new List<int>(); 

        Postures.Add(Dropdown_F35B.value);

        if (Postures.Count == 1)
        {
            InitF35BButtons[2].SetActive(false);
        }
    }

    public void F35BSelectButton() // Select 버튼 누르면 다 살아남
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

    public void F35BInitialization() // F35B에 관한 정보 모두 Reset
    {
        All_Fleets.text = null;
        All_Fleets.readOnly = false; // 사용자가 입력 가능

        Deck_Fleet.text = null;
        Deck_Fleet.readOnly = false; // 사용자가 입력 가능

        Dropdown_Fleet.interactable = true;
        Dropdown_Fleet.ClearOptions(); // dropdown.ClearOptions() == dropdown.options.Clear() : 드롭다운에서 옵션 목록 지움 
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
        Start(); // 서비스 구역 때문에 있어야함

        InitF35BButtons[0].SetActive(true);
        InitF35BButtons[1].SetActive(true);
        InitF35BButtons[2].SetActive(true);
        InitF35BButtons[3].SetActive(true);
        InitF35BButtons[4].SetActive(false);
    }

    //Resource 관련 UGUI
    public void Resource_Tractor()
    {
        DeckData.Tractors = int.Parse(TractorValue.text);
        DestroyResourceImage(TractorClones);
        TractorClones.Clear();

        if (DeckData.Tractors == 0)
        {
            TractorImage.SetActive(false);
            Debug.Log("1개 이상의 트랙터가 필요합니다.");
            TractorValue.text = null;
        }

        else if (DeckData.Tractors > 6)
        {
            TractorImage.SetActive(false);
            Debug.Log(string.Format("{0}개 이상의 트랙터는 운용할 수 없습니다.", DeckData.Tractors));
            TractorValue.text = null;
        }

        else
        {
            TractorValue.readOnly = true;
            TractorImage.SetActive(true);

            // 복제되는 트랙터 이미지의 위치는 절대 좌표계로 해야해서,,
            List<Vector2> TractorLocation = new List<Vector2>() { new Vector2(1150, 255), new Vector2(1120, 255), new Vector2(1090, 255), new Vector2(1060, 255), new Vector2(1030, 255) };

            for (int i = 0; i < DeckData.Tractors - 1; i++)
            {
                GameObject TCloneindex = Instantiate(TractorImage, new Vector3(TractorLocation[i].x, TractorLocation[i].y), Quaternion.identity);
                TCloneindex.transform.parent = EmptyResourceObject.transform;
                //TCloneindex.transform.parent = TractorImage.transform; -> 왜 일케하면 쭉쭉쭉쭉 상속이 되지?
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
            Debug.Log("1명 이상의 Red 인원이 필요합니다.");
            RedValue.text = null;
        }

        else if (DeckData.RedGroup > 6)
        {
            RedImage.SetActive(false);
            Debug.Log(string.Format("{0}명 이상의 Red 인적 자원은 운용할 수 없습니다.", DeckData.RedGroup));
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

    public void SaveF35BInfo() // Brown 개수 정하려고. Brown = 함재기 개수
    {
        CheckingBrowns.Clear();

        for (int i = 0; i < TextsforArea.Length; i++)
        {
            if (TextsforArea[i].text != "") // TextsforArea[i].text가 ""이 아니면 -> 함재기가 채워졌다는 의미
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
            Debug.Log("1명 이상의 Purple 인원이 필요합니다.");
            PurpleValue.text = null;
        }

        else if (DeckData.PurpleGroup > 6)
        {
            PurpleImage.SetActive(false);
            Debug.Log(string.Format("{0}명 이상의 Purple 인적 자원은 운용할 수 없습니다.", DeckData.PurpleGroup));
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
            Debug.Log("1명 이상의 Green 인원이 필요합니다.");
            GreenValue.text = null;
        }

        else if (DeckData.GreenGroup > 6)
        {
            GreenImage.SetActive(false);
            Debug.Log(string.Format("{0}명 이상의 Green 인적 자원은 운용할 수 없습니다.", DeckData.GreenGroup));
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

    public void ResourceInitialization() // Deck Resource에 관한 정보 모두 Reset
    {
        TractorValue.text = null;
        RedValue.text = null;
        //BrownValue.text = null; //Brown : 함재기 개수가 정해지면 그 수에 맞춰 정해지는 거라서 바뀌거나 바꿀 수 없음
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

        //BrownImage.SetActive(false); //Brown : 함재기 개수가 정해지면 그 수에 맞춰 정해지는 거라서 바뀌거나 바꿀 수 없음
        //DestroyResourceImage(BrownClones);

        PurpleImage.SetActive(false);
        DestroyResourceImage(PurpleClones);

        GreenImage.SetActive(false);
        DestroyResourceImage(GreenClones);
    }

    public void Alone_ToOtherInfoScene() // 단독 모드에서의 "NextButton : Hangar & EV Info >>"
    {
        SceneManager.LoadScene("Alone_OtherInformation");
        DontDestroyOnLoad(DeckData.AllDataManager);
    }

    public void Inte_ToOtherInfoScene() // 통합 모드에서의 "NextButton : Hangar & EV Info >>"
    {
        SceneManager.LoadScene("Inte_OtherInformation");
        DontDestroyOnLoad(DeckData.AllDataManager);
    }
}
