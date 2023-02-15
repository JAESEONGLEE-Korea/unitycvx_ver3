using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataManagement : MonoBehaviour
{
    // 내가 진짜다!!!!!
    public GameObject AllDataManager; // DontDestroyOnLoad()를 사용해야해서 GameObject 형태
    public ServiceAreaManagement Area_management;
    public HangarAreaManagement HangarArea_management;

    public int External_PortNumber;
    public int Internal_PortNumber;

    public int AllFleets;
    public int DeckFleets;

    public int Tractors;
    public int RedGroup;
    public int BrownGroup;
    public int PurpleGroup;
    public int GreenGroup;

    public int Hangar_Trators;
    public int Hangar_GreenGroup;

    public double D1_Velocity;
    public double D2_Velocity;

    //public DataManagement()
    //{
    //    Area_management = new ServiceAreaManagement();
    //}

    private void Start()
    {
        Area_management = new ServiceAreaManagement(); // 비행갑판 구역 함재기 정보
        HangarArea_management = new HangarAreaManagement(); // 정비구역 함재기 정보
    }
}
