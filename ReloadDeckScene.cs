using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SaveDeckData
{
    public class ReloadDeckScene : MonoBehaviour
    {
        //public Dictionary<Vector3, string> DeckServiceArea = new Dictionary<Vector3, string>();
        // ��ųʸ��� ��ġ���� ���� ������ ������������������
        public static Dictionary<string, Vector3> DeckServiceArea = new Dictionary<string, Vector3>();

        public static float ParkingAngle = 0.0f;
        public static int InternalPort;

        // 221103 ���񱸿�->���఩������ �Ѿ ��, DontDestroyOnLoad�� ��� �Ʒ��� null�� �Ǵ� �� ����
        //public static GameObject[] NewAircraft = new GameObject[1]; // ���峭 ����⸦ ����ؼ�, ���񱸿����� ������ ���ο� �����
    }
}