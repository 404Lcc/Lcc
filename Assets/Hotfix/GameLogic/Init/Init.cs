using System.Collections;
using LccModel;
using UnityEngine;

namespace LccHotfix
{
    public partial class Init
    {
        public static void Start()
        {
            Log.SetLogHelper(new DefaultLogHelper());
            Main.SetMain(new GameMain());
        }
    }
}