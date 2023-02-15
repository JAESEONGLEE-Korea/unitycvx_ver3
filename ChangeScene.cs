using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    //OtherOptions HangarScene = new OtherOptions(); // 220916 14:45 주석 처리

    public void ToSelectScene() // 단독 모드 or 통합 모드 선택 씬
    {
        SceneManager.LoadScene("SecondScene");
    }

    public void Alone_ToDeckInformation() // 단독 운용 모드로
    {
        SceneManager.LoadScene("Alone_DeckInformation");
    }

    public void Inte_ToDeckInformation() // 통합 운용 모드로
    {
        SceneManager.LoadScene("Inte_DeckInformation");
    }
}
