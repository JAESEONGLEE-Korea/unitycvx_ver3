//using BlockAlign.Numeric;
//using System.Collections;
//using System.Collections.Generic;
//using System.IO;
//using UnityEngine;
//using UnityEngine.AddressableAssets;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;

//public class LoadFromFileExample : MonoBehaviour
//{
//    GameObject b;
//    public InputField I;
//    UnityEngine.AssetBundle myLoadedAssetBundle;
//    [HideInInspector]
//    public static bool IsEnd = false;
//    string path = @"C:\JsonData\Polygon_NPS\Model\";
//    List<string> nameIdx = new List<string>();
//    int idx;
//    public void Awake()
//    {
//        if (System.IO.Directory.Exists(path))
//        {

//            //DirectoryInfo 객체 생성

//            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(path);

//            //해당 폴더에 있는 파일이름을 출력

//            foreach (var item in di.GetFiles())
//            {
//                nameIdx.Add(item.Name);
//            }

//        }
//    }

//    public void Send()
//    {
//        if (I.textComponent != null)
//        {
//            idx = int.Parse(I.textComponent.text);
//            if (idx < nameIdx.Count || idx >= 0)
//            {
//                if (myLoadedAssetBundle != null)
//                {
//                    Destroy(b);
//                    myLoadedAssetBundle.UnloadAsync(true);
//                    SceneManager.LoadScene("SampleScene");
//                    IsEnd = false;
//                }

//                IsEnd = false;
//                myLoadedAssetBundle = AssetBundle.LoadFromFile(path + nameIdx[idx]);

//                if (myLoadedAssetBundle == null)
//                {
//                    Debug.Log("Failed to load AssetBundle!");
//                    return;
//                }

//                //var prefab = myLoadedAssetBundle.LoadAllAssets();
//                var prefab = myLoadedAssetBundle.LoadAsset<GameObject>(nameIdx[idx].RemoveEnd(3));
//                b = (GameObject)Instantiate(prefab/*[0]*/, new Vector3(0, -10, 0), Quaternion.identity);
//                if (b.GetComponent<MeshCollider>().convex) b.GetComponent<MeshCollider>().convex = false;
//                GameObject.Find("Canvas").GetComponent<solution>().polygon = b;
//            }
//            else GameObject.Find("PopUp").transform.GetChild(3).gameObject.SetActive(true);
//        }
//    }

//    public void Reset()
//    {
//        if (myLoadedAssetBundle != null)
//        {
//            Destroy(b);
//            myLoadedAssetBundle.UnloadAsync(true);
//            IsEnd = false;
//        }
//        SceneManager.LoadScene("SampleScene");
//    }

//    public void Convex()
//    {
//        if (b != null)
//        {
//            b.GetComponent<MeshCollider>().convex = true;
//        }
//    }

//    public void Concave()
//    {
//        if (b != null)
//        {
//            b.GetComponent<MeshCollider>().convex = false;
//        }
//    }

//    public void Update()
//    {
//        if (IsEnd)
//        {
//            Destroy(b);
//            myLoadedAssetBundle.UnloadAsync(true);
//            IsEnd = false;
//        }
//    }
//}