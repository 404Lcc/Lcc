using System;

namespace LccModel
{
    public class DownloadData
    {
        //文件名
        public string name;
        //下载地址
        public string url;
        //保存路径
        public string path;
        //文件长度
        public long size;
        //进度回调
        public event Action<DownloadData, long, long> Progress;
        //完成回调
        public event Action<DownloadData> Complete;
        //错误回调
        public event Action<DownloadData, string> Error;
        public DownloadData()
        {
        }
        public DownloadData(string name, string url, string path)
        {
            this.name = name;
            this.url = url;
            this.path = path;
        }
        public void ProgressExcute(long currentSize, long size)
        {
            Progress?.Invoke(this, currentSize, size);
        }
        public void CompleteExcute()
        {
            Complete?.Invoke(this);
        }
        public void ErrorExcute(string error)
        {
            Error?.Invoke(this, error);
        }
    }
}