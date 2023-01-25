using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace LccModel
{
    public class CombatContext : Entity
    {
        public static CombatContext Instance { get; private set; }


        public override void Awake()
        {
            base.Awake();
            Instance = this;
        }


    }

    public class CombatEndEvent
    {

    }
}