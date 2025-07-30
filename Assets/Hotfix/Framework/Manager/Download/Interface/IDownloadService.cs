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

namespace LccHotfix
{
    public interface IDownloadService : IService
    {
        bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors);

        void DownloadAsync(DownloadData downloadData);

        void DownloadAsync(DownloadData[] downloadDatas);

        void DownloadTask();

        void UpdateTask();

        void UpdateProgress();

        void UpdateCompleted();

        void UpdateError();
    }
}