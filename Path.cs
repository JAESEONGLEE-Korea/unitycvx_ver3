using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path : MonoBehaviour
{
    public TextAsset txt;
    public GameObject F35B;
    string[,] Sentence;
    int lineSize, rowSize;
    int k = 0;
    float t = 0;

    void Start()
    {
        string currentText = txt.text.Substring(0, txt.text.Length - 1);
        string[] line = currentText.Split('\n');
        lineSize = line.Length;
        rowSize = line[0].Split('\t').Length;
        Sentence = new string[lineSize, rowSize];
        for (int i = 0; i < lineSize; i++)
        {
            string[] row = line[i].Split('\t');
            for (int j = 0; j < rowSize; j++) Sentence[i, j] = row[j];
        }
    }

    void FixedUpdate()
    {
        float tractor_x = float.Parse(Sentence[k, 0]);
        float tractor_y = float.Parse(Sentence[k, 1]);

        float w_deg = float.Parse(Sentence[k, 2]);
        F35B.transform.position = new Vector3(tractor_x, tractor_y, -1.9f);
        F35B.transform.rotation = Quaternion.Euler(w_deg, 90, -90);
        k = k+1;
    }
}
