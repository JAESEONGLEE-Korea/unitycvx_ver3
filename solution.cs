using BlockAlign.Numeric;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
//using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using NFP;

public class solution : MonoBehaviour // solution 클래스 : RayCast를 이용하여 함재기 형태(Polygon)를 좌표 값으로 따옴 => F35B, HELO 함재기 Polygon을 따와서 파일로 저장
{
    List<Vector3> poly;
    [HideInInspector]
    GameObject b;
    [HideInInspector]
    public static bool IsEnd = false;
    //string path = @"C:\JsonData\Polygon_NPS\Model\";
    List<string> nameIdx = new List<string>();
    int idx;
    bool isConvex = false;
    bool chkAgain = false;
    public GameObject polygon;
    public GameObject forPolygonDesign;

    public void Start()
    {
        {
            Sensor s = new Sensor();
            var v = s.Get();

            if (v.Count < 3)  //센서의 count가 3이하라면 그냥 바로 continue 시키기
            {
                chkAgain = false;
                Debug.LogError(polygon.name + " : raycast가 3개 이하의 정점 인식");
            }

            try { poly = MakeKNN.Gen(v); }
            catch
            {
                if (chkAgain)
                {
                    isConvex = false;
                    chkAgain = false;
                }
                isConvex = true;
                chkAgain = true;
            }
            ////////////////////////////////////////////////


            ////////////////////폴리곤 내부 교차 2차 검증//////////////////////
            bool con1;
            bool c = false;
            for (int i = 1; i < poly.Count - 1; i++)
            {
                pt s1; s1.x = poly[i - 1].x; s1.y = poly[i - 1].z;
                pt e1; e1.x = poly[i].x; e1.y = poly[i].z;

                for (int i2 = 1; i2 < poly.Count - 1; i2++)
                {
                    //if(i == i2) continue;
                    if (Math.Abs(i - i2) < 2) continue;
                    pt s2; s2.x = poly[i2 - 1].x; s2.y = poly[i2 - 1].z;
                    pt e2; e2.x = poly[i2].x; e2.y = poly[i2].z;

                    con1 = Intersected(ref s1, ref e1, ref s2, ref e2);

                    if (con1)
                    {
                        c = true;
                        break;
                    }
                }

                if (c) break;
            }

            if (c)
            {
                if (chkAgain)
                {
                    isConvex = false;
                    chkAgain = false;
                }
                isConvex = true;
                chkAgain = true;

            }

            ///////////////////////////////////////////////////////////////////
            MakePolygon.Gen(poly);
            chkAgain = false;
            isConvex = false;
        }

        Application.Quit();
    }
    public void Convex()
    {
        if (b != null)
        {
            b.GetComponent<MeshCollider>().convex = true;
        }
    }

    public void Concave()
    {
        if (b != null)
        {
            b.GetComponent<MeshCollider>().convex = false;
        }
    }

    int Orient(ref pt p1, ref pt p2, ref pt p3)
    {
        double d = ((p2.y - p1.y) * (p3.x - p2.x) - (p3.y - p2.y) * (p2.x - p1.x)) / (Math.Sqrt(Math.Pow(p1.x - p2.x, 2) + Math.Pow(p1.y - p2.y, 2)));
        int a;
        if (d < -1.0E-8) a = -1;
        else
        {
            if (Math.Abs(d) > 1.0E-8) a = 1; //d가 0이 아니면
            else a = 0;                     //d가 0이면
        };
        return a;
    }

    bool InLine(ref pt s, ref pt e, ref pt p)
    {
        if (Math.Abs(s.x - e.x) > 1.0E-8 && Math.Abs(s.y - e.y) > 0)
        {
            double d = (e.y - s.y) / (e.x - s.x), y;
            y = d * (p.x - s.x) + s.y;
            return s.x <= p.x && p.x <= e.x && Math.Abs(y - p.y) < 1.0E-8;
        }
        if (Math.Abs(s.x - e.x) < 1.0E-8) return Math.Abs(p.x - s.x) < 1.0E-8 && s.y <= p.y && p.y <= e.y;
        else return Math.Abs(p.y - s.y) < 1.0E-8 && s.x <= p.x && p.x <= e.x;
    }

    bool Intersected(ref pt s1, ref pt e1, ref pt s2, ref pt e2)
    {
        bool can = false;
        int d1 = Orient(ref s1, ref e1, ref s2), d2 = Orient(ref s1, ref e1, ref e2);
        if ((d1 != d2) && (Orient(ref s2, ref e2, ref s1) != Orient(ref s2, ref e2, ref e1))) can = true;
        else if (d1 == 0 && d2 == 0) can = InLine(ref s1, ref e1, ref s2) || InLine(ref s1, ref e1, ref e2) || InLine(ref s2, ref e2, ref s1) || InLine(ref s2, ref e2, ref e1);
        return can;
    }
}

public struct pt { public double x, y; };

public class Sensor
{
    float xMin = float.MaxValue;
    float xMax = float.MinValue;
    float zMin = float.MaxValue;
    float zMax = float.MinValue;
    private List<Vector3> output = new List<Vector3>();

    public Sensor()
    {
        output.Clear();
        float r = 0.1f;

        Vector3 start = new Vector3(0, 0, 0);
        Vector3 end = new Vector3(0, -1, 0);
        RaycastHit hitData;

        xMax = 20;
        xMin = -20;
        zMax = 20;
        zMin = -20;

        float xD = xMax - xMin;
        float zD = zMax - zMin;

        int n1 = (int)(xD / r) + 5;
        int n2 = (int)(zD / r) + 5;

        for (int i = 0; i < n1; i++)
        {
            for (int j = 0; j < n2; j++)
            {
                start.x = xMin - r + r * i;
                start.y = 20;
                start.z = zMin - r + r * j;


                Ray ray = new Ray(start, end);
                if (Physics.Raycast(ray, out hitData)) output.Add(hitData.point);
            }
        }
    }

    public List<Vector3> Get()
    {
        return output;
    }
}

public class MakeKNN
{
    static List<RuyVector> vec = new List<RuyVector>();
    static List<Vector3> KNNpolygon = new List<Vector3>();

    public static List<Vector3> Gen(List<Vector3> list)
    {
        vec.Clear();
        KNNpolygon.Clear();

        for (int j = 0; j < list.Count; j++)
        {
            vec.Add(new RuyVector(list[j].x, list[j].z, 0));
        }

        ConcaveHull t = new ConcaveHull(vec, 3, 15);
        var pol = t.Run();

        foreach (var v in pol)
        {
            KNNpolygon.Add(new Vector3((float)v.X, 0, (float)v.Y));
        }

        return KNNpolygon;
    }
}

public class MakePolygon : MonoBehaviour // 추가
{
    //public GameObject forPolygonDesign; // Polygon 그릴 Gameobject

    public static void Gen(List<Vector3> poly)
    {
        List<Vector3> v = poly;
        List<Vector2> v1 = new List<Vector2>();

        for (int i = 0; i < v.Count - 1; i++) v1.Add(new Vector2(v[i].x, v[i].z));

        Vector2 a = new Vector2();
        Vector2 b = new Vector2();
        for (int i = 1; i < v1.Count - 1; i++)
        {
            a.x = v1[i - 1].x - v1[i].x;
            a.y = v1[i - 1].y - v1[i].y;
            b.x = v1[i + 1].x - v1[i].x;
            b.y = v1[i + 1].y - v1[i].y;
            if (Para(a, b)) { v1.RemoveAt(i); i--; }
        }

        string json = JsonUtility.ToJson(new Serialization<Vector2>(v1));

        string fileName = "F35BPolygon";
        //string fileName = "HELOPolygon";

        //string fileName = GameObject.Find("Canvas").GetComponent<solution>().polygon.name;
        //fileName = fileName.RemoveEnd(3); // 파일명을 짜르는거네

        DirectoryInfo di = new DirectoryInfo(@"C:\JsonData\Polygon_NPS\Result\");
        if (di.Exists == false) di.Create();

        string path = @"C:\JsonData\Polygon_NPS\Result\" + fileName + ".Json";
        File.WriteAllText(path, json);

        MakePolygon forCheck = new MakePolygon();
        forCheck.PolygonDesign(v1);
    }

    public void PolygonDesign(List<Vector2> v)
    {
        GameObject GameObject_forGetScript = GameObject.Find("GetPolygon");
        solution GetScript = GameObject_forGetScript.GetComponent<solution>();
        GameObject OriginalObject = GetScript.forPolygonDesign;

        //Debug.Log(v.Count);

        for (int i = 0; i < v.Count; i++)
        {
            //Debug.Log(string.Format("X : {0}", v[i].x));
            //Debug.Log(string.Format("Y : {0}", v[i].y));
            //Debug.Log(OriginalObject);

            Instantiate(OriginalObject, new Vector2(v[i].x, v[i].y), Quaternion.identity); // MonoBehaviour를 상속받지 않으면 Instantiate 사용 불가
        }
    }

    static float c;
    public static bool Para(Vector2 a, Vector2 b)
    {
        c = a.x * b.y - a.y * b.x;
        if (Math.Abs(c) < 1.0E-3) return true;
        else return false;
    }
}

public static class StringExtensions
{
    public static String RemoveEnd(this String str, int len)
    {
        if (str.Length < len)
        {
            return string.Empty;
        }

        return str.Remove(str.Length - len);
    }
}

[Serializable]
public class Serialization<T>
{
    [SerializeField]
    List<T> vector;
    public List<T> ToList() { return vector; }

    public Serialization(List<T> vector)
    {
        this.vector = vector;
    }
}