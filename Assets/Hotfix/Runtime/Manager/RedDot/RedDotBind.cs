using LccModel;
using System;
using TMPro;
using UnityEngine;

namespace LccHotfix
{
    public class RedDotBind : MonoBehaviour
    {

        public TextMeshProUGUI text;


        public string key;
        public int id;

        public bool Show { get; private set; }


        public int Count { get; private set; }

        private void Awake()
        {
            Action<string, int, int> action = OnRedDotChangeHandler;
            HotfixFunc.CallSingletonMethod("LccHotfix", "RedDotManager", "Instance", "AddChanged", new object[] { key, id, action });
        }

        private void OnDestroy()
        {
            Action<string, int, int> action = OnRedDotChangeHandler;
            HotfixFunc.CallSingletonMethod("LccHotfix", "RedDotManager", "Instance", "RemoveChanged", new object[] { key, id, action });
        }

        private void OnRedDotChangeHandler(string key, int id, int count)
        {
            if (this.key == key && this.id == id)
            {
                Show = count >= 1;
                Count = count;
                Refresh();
            }
        }

        private void Refresh()
        {
            gameObject.SetActive(Show);
            if (text != null)
                text.text = Count.ToString();
        }
    }
}