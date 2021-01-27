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
        public Action<DownloadData, long, long> progress;
        //完成回调
        public Action<DownloadData> complete;
        //错误回调
        public Action<DownloadData, string> error;
        public DownloadData()
        {
        }
        public DownloadData(string name, string url, string path)
        {
            this.name = name;
            this.url = url;
            this.path = path;
        }
    }
}