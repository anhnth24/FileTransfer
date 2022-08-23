using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FileTransfer
{
    public static class ServiceFileTransfer
    {

        private static readonly string ToUploadPath = ConfigurationManager.AppSettings["ToUploadPath"].ToString();
        private static readonly string FromUploadPath = ConfigurationManager.AppSettings["FromUploadPath"].ToString();
        private static readonly string accountFtp = ConfigurationManager.AppSettings["AccountFTP"].ToString();
        private static readonly string passwordFtp = ConfigurationManager.AppSettings["PasswordFTP"].ToString();
        private static readonly string[] fileExtension = ConfigurationManager.AppSettings["FileExtension"].Split(',');

        public static void SendFile()
        {
            DirectoryInfo locationPath = new DirectoryInfo(FromUploadPath);
            var filesLocal = locationPath.GetFilesByExtensions(fileExtension);
            var fileServerName = GetReceiveServerFileName(ToUploadPath);
            foreach (var file in filesLocal)
            {
                try
                {
                    if (!fileServerName.Contains(file.Name))
                    {
                        var a = new Uri(String.Format("{0}{1}", ToUploadPath, file.Name));
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(a);
                        request.Credentials = new NetworkCredential(accountFtp, passwordFtp);
                        request.Method = WebRequestMethods.Ftp.UploadFile;
                        using (Stream fileStream = File.OpenRead(FromUploadPath + "\\" + file.Name))
                        using (Stream ftpStream = request.GetRequestStream())
                        {
                            byte[] buffer = new byte[10240];
                            int read;
                            while ((read = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ftpStream.Write(buffer, 0, read);

                            }
                            Console.WriteLine("File transfer {0} bytes", fileStream.Position);
                        }
                    }
                    else
                    {
                        Console.WriteLine("file " + file.Name + " exists !");
                    }
                }
                catch (Exception e)
                {
                    Console.Write(e);
                }
                
            }
        }
        public static void DownFile()
        {
            //DirectoryInfo serverPath = new DirectoryInfo(ToUploadPath);
            //var fileLocalName  = GetCurrentServerFileName(FromUploadPath);
            //var filesServer = serverPath.GetFilesByExtensions(fileExtension);
            var fileServerList = getFileList(ToUploadPath, accountFtp, passwordFtp);
            var fileLocal = GetCurrentServerFileName(FromUploadPath);
            int bytesRead;
            byte[] buffer = new byte[2048];
            foreach(var file in fileServerList)
            {
                if (!fileLocal.Contains(file))
                {
                    FtpWebRequest request = CreateFtpWebRequest(ToUploadPath + file, accountFtp, passwordFtp, true);

                    request.Method = WebRequestMethods.Ftp.DownloadFile;

                    Stream reader = request.GetResponse().GetResponseStream();
                    FileStream fileStream = new FileStream(FromUploadPath + "\\" + file, FileMode.Create);

                    while (true)
                    {
                        bytesRead = reader.Read(buffer, 0, buffer.Length);

                        if (bytesRead == 0)
                            break;

                        fileStream.Write(buffer, 0, bytesRead);
                    }
                    fileStream.Close();
                    Console.WriteLine("Downloaded " + file + "!");
                }
                else
                {
                    Console.WriteLine("File " + file + " exist !");
                }
            }
            
        }

        public static List<string> getFileList(string ftpDirectoryPath, string accountFTP, string passwordFTP)
        {
            List<string> sourceList = new List<string>();
            string line = "";
            FtpWebRequest request;
            request = (FtpWebRequest)WebRequest.Create(ftpDirectoryPath);
            request.Credentials = new NetworkCredential(accountFTP, passwordFTP);
            request.Method = WebRequestMethods.Ftp.ListDirectory;
            request.UseBinary = true;
            request.KeepAlive = false;
            request.Timeout = -1;
            request.UsePassive = true;
            FtpWebResponse sourceRespone = (FtpWebResponse)request.GetResponse();
            
            using (Stream responseStream = sourceRespone.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    
                    line = reader.ReadLine();
                    while (line != null)
                    {

                        sourceList.Add(line);
                        line = reader.ReadLine();
                    }
                }
            }
            return sourceList;
        }

        private static List<string> GetCurrentServerFileName(string url)
        {
            return  new DirectoryInfo(url).GetFiles().Select(o => o.Name).ToList();
        }
        
        private static List<string> GetReceiveServerFileName(string url)
        {
            FtpWebRequest requestGetFile = (FtpWebRequest)WebRequest.Create(url);
            requestGetFile.Method = WebRequestMethods.Ftp.ListDirectory;

            requestGetFile.Credentials = new NetworkCredential(accountFtp, passwordFtp);
            FtpWebResponse response = (FtpWebResponse)requestGetFile.GetResponse();
            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            string names = reader.ReadToEnd();
            reader.Close();
            response.Close();
            return names.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        private static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException("extensions");
            IEnumerable<FileInfo> files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension));
        }

        private static FtpWebRequest CreateFtpWebRequest(string ftpDirectoryPath, string userName, string password, bool keepAlive = false)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(ftpDirectoryPath));

            //Set proxy to null. Under current configuration if this option is not set then the proxy that is used will get an html response from the web content gateway (firewall monitoring system)
            request.Proxy = null;

            request.UsePassive = true;
            request.UseBinary = true;
            request.KeepAlive = keepAlive;

            request.Credentials = new NetworkCredential(userName, password);

            return request;
        }
    }
}
