using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    //OtherOptions HangarScene = new OtherOptions(); // 220916 14:45 �ּ� ó��

    public void ToSelectScene() // �ܵ� ��� or ���� ��� ���� ��
    {
        SceneManager.LoadScene("SecondScene");
    }

    public void Alone_ToDeckInformation() // �ܵ� ��� ����
    {
        SceneManager.LoadScene("Alone_DeckInformation");
    }

    public void Inte_ToDeckInformation() // ���� ��� ����
    {
        SceneManager.LoadScene("Inte_DeckInformation");
    }
}
