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
    public interface IFiberService : IService
    {
        bool IsDisposed();
        int Create(SchedulerType schedulerType, int fiberId, Action<Fiber> action);
        int Create(SchedulerType schedulerType, Action<Fiber> action);
        void Remove(int id);
        int Count();
    }
}