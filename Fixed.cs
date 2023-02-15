using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fixed : MonoBehaviour
{    
    // Start is called before the first frame update
    void Start()
    {
        //SetResolution();
        Application.runInBackground = true;
    }

    //public void SetResolution()
    //{
    //    int setWidth = 400;  //화면 너비
    //    int setHeight = 300;  //화면 높이

    //    Screen.SetResolution(setWidth, setHeight, false);
    //}
}
