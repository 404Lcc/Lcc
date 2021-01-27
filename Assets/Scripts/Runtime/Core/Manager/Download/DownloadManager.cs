using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace LccModel
{
    public class DownloadManager : Singleton<DownloadManager>
    {
        public object lockObject = new object();
        //最大任务数
        public int taskCount = 20;
        //准备队列
        public Queue<DownloadFile> readyQueue = new Queue<DownloadFile>();
        //运行
        public Hashtable runnings = new Hashtable();
        //完成列表
        public List<DownloadData> completeList = new List<DownloadData>();
        //错误列表
        public List<DownloadFile> errorList = new List<DownloadFile>();
        public void InitManager()
        {
            ServicePointManager.DefaultConnectionLimit = 100;
            ServicePointManager.ServerCertificateValidationCallback = ServerCertificateValidationCallback;
        }
        public override void Update()
        {
            UpdateTask();
            UpdateProgress();
            UpdateComplete();
            UpdateError();
        }
        public bool ServerCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                        bool chainIsValid = chain.Build((X509Certificate2)certificate);
                        if (!chainIsValid)
                        {
                            isOk = false;
                        }
                    }
                }
            }
            return isOk;
        }
        public void DownloadAsync(DownloadData downloadData)
        {
            DownloadFile downloadFile = new DownloadFile(downloadData);
            lock (lockObject)
            {
                readyQueue.Enqueue(downloadFile);
            }
            if (runnings.Count >= taskCount) return;
            Task task = Task.Run(DownloadTask);
            task.Start();
        }
        public void DownloadAsync(DownloadData[] downloadDatas)
        {
            foreach (DownloadData item in downloadDatas)
            {
                DownloadAsync(item);
            }
        }
        public void DownloadTask()
        {
            lock (lockObject)
            {
                runnings.Add(Thread.CurrentThread, null);
            }
            while (true)
            {
                DownloadFile downloadFile = null;
                lock (lockObject)
                {
                    if (readyQueue.Count > 0)
                    {
                        downloadFile = readyQueue.Dequeue();
                        runnings[Thread.CurrentThread] = downloadFile;
                    }
                }
                if (downloadFile == null) break;
                downloadFile.Download();
                if (downloadFile.state == DownloadState.Complete)
                {
                    lock (lockObject)
                    {
                        completeList.Add(downloadFile.downloadData);
                        runnings[Thread.CurrentThread] = null;
                    }
                }
                else if (downloadFile.state == DownloadState.Error)
                {
                    lock (lockObject)
                    {
                        if (downloadFile.count == 5)
                        {
                            errorList.Add(downloadFile);
                        }
                        else
                        {
                            readyQueue.Enqueue(downloadFile);
                        }
                    }
                    break;
                }
                else
                {
                    break;
                }
            }
        }
        public void UpdateTask()
        {
            if (readyQueue.Count == 0 && runnings.Count == 0) return;
            lock (lockObject)
            {
                List<Thread> threadList = new List<Thread>();
                foreach (DictionaryEntry item in runnings)
                {
                    //卡死线程
                    if (!((Thread)item.Key).IsAlive)
                    {
                        if (item.Value != null)
                        {
                            readyQueue.Enqueue((DownloadFile)item.Value);
                        }
                        threadList.Add((Thread)item.Key);
                    }
                }
                foreach (Thread item in threadList)
                {
                    item.Abort();
                    runnings.Remove(item);
                }
            }
            if (NetworkUtil.CheckNetwork())
            {
                if (runnings.Count < taskCount && readyQueue.Count > 0)
                {
                    Task task = Task.Run(DownloadTask);
                    task.Start();
                }
            }
        }
        public void UpdateProgress()
        {
            if (runnings.Count == 0) return;
            List<DownloadFile> downloadFileList = new List<DownloadFile>();
            lock (lockObject)
            {
                foreach (DownloadFile item in runnings.Values)
                {
                    if (item != null)
                    {
                        downloadFileList.Add(item);
                    }
                }
            }
            foreach (DownloadFile item in downloadFileList)
            {
                item.downloadData.progress?.Invoke(item.downloadData, item.currentSize, item.downloadData.size);
            }
        }
        public void UpdateComplete()
        {
            if (completeList.Count == 0) return;
            List<DownloadData> downloadDataList = new List<DownloadData>();
            lock (lockObject)
            {
                downloadDataList.AddRange(completeList.ToArray());
                completeList.Clear();
            }
            foreach (DownloadData item in downloadDataList)
            {
                item.progress?.Invoke(item, item.size, item.size);
                item.complete?.Invoke(item);
            }
        }
        public void UpdateError()
        {
            if (errorList.Count == 0) return;
            List<DownloadFile> downloadFileList = new List<DownloadFile>();
            lock (lockObject)
            {
                downloadFileList.AddRange(errorList.ToArray());
                errorList.Clear();
            }
            foreach (DownloadFile item in downloadFileList)
            {
                item.downloadData.error?.Invoke(item.downloadData, item.error);
            }
        }
    }
}