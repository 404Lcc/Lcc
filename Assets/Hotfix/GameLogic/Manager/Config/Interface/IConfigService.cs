using System;
using System.Collections.Generic;
using System.Reflection;
using cfg;
using LccModel;
using Luban;
using SimpleJSON;
using UnityEngine;

namespace LccHotfix
{
    public interface IConfigService : IService
    {
        Tables Tables { get; set; }
    }
}