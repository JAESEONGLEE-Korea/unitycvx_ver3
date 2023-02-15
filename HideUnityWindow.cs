using System;
using UnityEngine;
using System.Runtime.InteropServices;

public class HideUnityWindow : MonoBehaviour
{
    [DllImport("user32.dll")]
    private static extern IntPtr GetActiveWindow();

    [DllImport("user32.dll")]
    static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    const int SW_HIDE = 0;

    void Awake()
    {
        var hwnd = GetActiveWindow();
        ShowWindow(hwnd, SW_HIDE);
    }
}