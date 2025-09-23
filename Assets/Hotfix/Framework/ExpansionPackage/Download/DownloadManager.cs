using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace LccHotfix
{
    internal class DownloadManager : Module, IDownloadService
    {
        public object lockObject = new object();

        //最大任务数
        public int taskCount = 20;

        //准备队列
        public Queue<DownloadFile> readyQueue = new Queue<DownloadFile>();

        //运行
        public Dictionary<Thread, DownloadFile> runningDict = new Dictionary<Thread, DownloadFile>();

        //完成列表
        public List<DownloadData> completedList = new List<DownloadData>();

        //错误列表
        public List<DownloadFile> errorList = new List<DownloadFile>();


        public DownloadManager()
        {
            ServicePointManager.DefaultConnectionLimit = 100;
            ServicePointManager.ServerCertificateValidationCallback = ServerCertificateValidationCallback;
        }


        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
            UpdateTask();
            UpdateProgress();
            UpdateCompleted();
            UpdateError();
        }

        internal override void Shutdown()
        {
            readyQueue.Clear();
            foreach (var item in runningDict)
            {
                item.Key.Abort();
            }

            runningDict.Clear();
            completedList.Clear();
            errorList.Clear();
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

            if (runningDict.Count >= taskCount) return;
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
                runningDict.Add(Thread.CurrentThread, null);
            }

            while (true)
            {
                DownloadFile downloadFile = null;
                lock (lockObject)
                {
                    if (readyQueue.Count > 0)
                    {
                        downloadFile = readyQueue.Dequeue();
                        runningDict[Thread.CurrentThread] = downloadFile;
                    }
                }

                if (downloadFile == null) break;
                downloadFile.Download();
                if (downloadFile.state == DownloadState.Completed)
                {
                    lock (lockObject)
                    {
                        completedList.Add(downloadFile.downloadData);
                        runningDict[Thread.CurrentThread] = null;
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
            if (readyQueue.Count == 0 && runningDict.Count == 0) return;
            lock (lockObject)
            {
                List<Thread> threadList = new List<Thread>();
                foreach (KeyValuePair<Thread, DownloadFile> item in runningDict)
                {
                    //卡死线程
                    if (!item.Key.IsAlive)
                    {
                        if (item.Value != null)
                        {
                            readyQueue.Enqueue(item.Value);
                        }

                        threadList.Add(item.Key);
                    }
                }

                foreach (Thread item in threadList)
                {
                    item.Abort();
                    runningDict.Remove(item);
                }
            }

            if (NetworkUtility.CheckNetwork())
            {
                if (runningDict.Count < taskCount && readyQueue.Count > 0)
                {
                    Task task = Task.Run(DownloadTask);
                    task.Start();
                }
            }
        }

        public void UpdateProgress()
        {
            if (runningDict.Count == 0) return;
            List<DownloadFile> downloadFileList = new List<DownloadFile>();
            lock (lockObject)
            {
                foreach (DownloadFile item in runningDict.Values)
                {
                    if (item != null)
                    {
                        downloadFileList.Add(item);
                    }
                }
            }

            foreach (DownloadFile item in downloadFileList)
            {
                item.downloadData.ProgressExcute(item.currentSize, item.downloadData.size);
            }
        }

        public void UpdateCompleted()
        {
            if (completedList.Count == 0) return;
            List<DownloadData> downloadDataList = new List<DownloadData>();
            lock (lockObject)
            {
                downloadDataList.AddRange(completedList.ToArray());
                completedList.Clear();
            }

            foreach (DownloadData item in downloadDataList)
            {
                item.ProgressExcute(item.size, item.size);
                item.CompletedExcute();
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
                item.downloadData.ErrorExcute(item.error);
            }
        }
    }
}