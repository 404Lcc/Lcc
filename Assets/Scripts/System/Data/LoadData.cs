using System;
using System.Collections.Generic;
using UnityEngine;

public class LoadData : MonoBehaviour
{
    public static int loadid;
    public static List<PanelType> open = new List<PanelType>();
    public static List<PanelType> clear = new List<PanelType>();
    public static bool bloadpanel;
    public static Action action;
}