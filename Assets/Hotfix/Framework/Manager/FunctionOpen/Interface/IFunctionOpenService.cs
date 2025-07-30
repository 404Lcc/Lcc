using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using cfg;
using LccModel;
using Luban;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Events;

namespace LccHotfix
{
    public interface IFunctionOpenService : IService
    {
        public bool IsFuncOpened(int functionID, bool dataCheck = false);
        public bool GetFuncOpenState(FunctionID functionID);
        public bool IsFunctionOpenedAndShowTips(int functionID, bool useNotice = false, bool popTips = true);
    }
}