using System.Collections;
using System.Collections.Generic;
using LccHotfix;
using UnityEngine;

namespace LccHotfix
{
    //简易信息
    public class PlayerSimpleData
    {
        public long UID { get; set; } // 角色id
        public string Name { get; set; } // 昵称
    }

    [Model]
    public class ModPlayer : ModelTemplate
    {

    }
}