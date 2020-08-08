using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

public static class DisplayMode
{
    [DllImport("user32.dll")]
    static extern IntPtr SetWindowLong(IntPtr hwnd, int _nIndex, int dwNewLong);
    [DllImport("user32.dll")]
    static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll")]
    static extern IntPtr GetForegroundWindow();
    // not used rigth now
    //const uint SWP_NOMOVE = 0x2;
    //const uint SWP_NOSIZE = 1;
    //const uint SWP_NOZORDER = 0x4;
    //const uint SWP_HIDEWINDOW = 0x0080;
    const uint SWP_SHOWWINDOW = 0x0040;
    const int GWL_STYLE = -16;
    const int WS_BORDER = 1;
    const int WS_POPUP = 0x800000;
    public static IEnumerator SetNoFrame(int width, int height)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();
        int posx = (Screen.currentResolution.width - width) / 2;
        int posy = (Screen.currentResolution.height - height) / 2;
        SetWindowLong(GetForegroundWindow(), GWL_STYLE, WS_POPUP);
        bool result = SetWindowPos(GetForegroundWindow(), 0, posx, posy, width, height, SWP_SHOWWINDOW);
    }
}