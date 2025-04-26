using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LccHotfix
{
    public interface IControl : IDispose
    {
        public LogicEntity Entity { get; set; }
        public void Update();
    }
}