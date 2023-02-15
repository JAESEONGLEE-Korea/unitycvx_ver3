using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveDeckData
{
    public class ReloadDeckScene : MonoBehaviour
    {
        //public Dictionary<Vector3, string> DeckServiceArea = new Dictionary<Vector3, string>();
        // 딕셔너리로 위치값에 따른 구역을 정해줘야하지않을까용
        public static Dictionary<string, Vector3> DeckServiceArea = new Dictionary<string, Vector3>();

        public static float ParkingAngle = 0.0f;
        public static int InternalPort;

        // 221103 정비구역->비행갑판으로 넘어갈 때, DontDestroyOnLoad가 없어서 아래가 null이 되는 것 같다
        //public static GameObject[] NewAircraft = new GameObject[1]; // 고장난 함재기를 대신해서, 정비구역에서 등장한 새로운 함재기
    }
}