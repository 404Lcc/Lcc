using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public static class IUILogicExtension
    {
        public static Window OpenChild(this IUILogic logic, string windowName, object[] param = null)
        {
            return Entry.GetModule<WindowManager>().OpenWindow(logic.wNode, windowName, param);
        }
    }
}