using System.IO;
using System.Net;

namespace LccModel
{
    public class DownloadFile
    {
        public int fileReadLength = 16 * 1024;
        public int md5ReadLength = 16 * 1024;
        public int readWriteTimeout = 5 * 1000;
        public int timeout = 5 * 1000;
        public int count;
        public long currentSize;
        public DownloadState state;
        public string error;
        public DownloadData downloadData;
        public DownloadFile()
        {
        }
        public DownloadFile(DownloadData downloadData)
        {
            this.downloadData = downloadData;
        }
        public void Download()
        {
            count++;
            state = DownloadState.Ready;
            if (!ReadSize()) return;
            state = DownloadState.Downloading;
            if (!Downloading()) return;
            state = DownloadState.Complete;
        }
        public bool ReadSize()
        {
            if (downloadData.size > 0) return true;
            HttpWebRequest httpWebRequest = null;
            WebResponse webResponse = null;
            try
            {
                httpWebRequest = (HttpWebRequest)WebRequest.Create(downloadData.url);
                httpWebRequest.ReadWriteTimeout = readWriteTimeout;
                httpWebRequest.Timeout = timeout;
                webResponse = httpWebRequest.GetResponse();
                downloadData.size = (int)webResponse.ContentLength;
                return true;
            }
            catch
            {
                state = DownloadState.Error;
                error = "获取文件失败";
            }
            finally
            {
                if (httpWebRequest != null)
                {
                    httpWebRequest.Abort();
                    httpWebRequest = null;
                }
                if (webResponse != null)
                {
                    webResponse.Dispose();
                    webResponse = null;
                }
            }
            return false;
        }
        public bool Downloading()
        {
            if (File.Exists(downloadData.path))
            {
                currentSize = downloadData.size;
                return true;
            }
            long position = 0;
            string tempPath = downloadData.path + ".temp";
            if (File.Exists(tempPath))
            {
                using (FileStream fileStream = new FileStream(tempPath, FileMode.Open, FileAccess.Write, FileShare.ReadWrite))
                {
                    position = fileStream.Length;
                    fileStream.Seek(position, SeekOrigin.Current);
                    if (position == downloadData.size)
                    {
                        if (File.Exists(downloadData.path))
                        {
                            File.Delete(downloadData.path);
                        }
                        File.Move(tempPath, downloadData.path);
                        currentSize = position;
                        return true;
                    }
                }
            }
            else
            {
                using (FileStream fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    HttpWebRequest httpWebRequest = null;
                    HttpWebResponse httpWebResponse = null;
                    try
                    {
                        httpWebRequest = (HttpWebRequest)WebRequest.Create(downloadData.url);
                        httpWebRequest.ReadWriteTimeout = readWriteTimeout;
                        httpWebRequest.Timeout = timeout;
                        if (position > 0)
                        {
                            httpWebRequest.AddRange((int)position);
                        }
                        httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        using (Stream stream = httpWebResponse.GetResponseStream())
                        {
                            stream.ReadTimeout = timeout;
                            long currentSize = position;
                            byte[] bytes = new byte[fileReadLength];
                            int readSize = stream.Read(bytes, 0, fileReadLength);
                            while (readSize > 0)
                            {
                                fileStream.Write(bytes, 0, readSize);
                                currentSize += readSize;
                                if (currentSize == downloadData.size)
                                {
                                    if (File.Exists(downloadData.path))
                                    {
                                        File.Delete(downloadData.path);
                                    }
                                    File.Move(tempPath, downloadData.path);
                                }
                                this.currentSize = currentSize;
                                readSize = stream.Read(bytes, 0, fileReadLength);
                            }
                        }
                    }
                    catch
                    {
                        if (File.Exists(tempPath))
                        {
                            File.Delete(tempPath);
                        }
                        if (File.Exists(downloadData.path))
                        {
                            File.Delete(downloadData.path);
                        }
                        state = DownloadState.Error;
                        error = "文件下载失败";
                    }
                    finally
                    {
                        if (httpWebRequest != null)
                        {
                            httpWebRequest.Abort();
                            httpWebRequest = null;
                        }
                        if (httpWebResponse != null)
                        {
                            httpWebResponse.Dispose();
                            httpWebResponse = null;
                        }
                    }
                }
            }
            if (state == DownloadState.Error)
            {
                return false;
            }
            return true;
        }
    }
}