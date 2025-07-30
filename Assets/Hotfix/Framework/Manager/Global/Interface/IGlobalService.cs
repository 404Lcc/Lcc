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
using UnityEngine.Video;

namespace LccHotfix
{
    public interface IGlobalService : IService
    {
        Transform Global { get; set; }
        AudioSource Music { get; set; }
        AudioSource SoundFX { get; set; }
        VideoPlayer VideoPlayer { get; set; }
    }
}