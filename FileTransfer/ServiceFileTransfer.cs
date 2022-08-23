using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FileTransfer
{
    public static class ServiceFileTransfer
    {
        public static string urlDestination = "ftp://10.15.163.49//file//";
        public static string localPath = "C:\\FileSend";
        private static readonly string accountFtp = "Anhnth27@fpt.com.vn";
        private static readonly string passwordFtp = "Anhnth27@fpt.com.vn";
        public static IEnumerable<FileInfo> GetFilesByExtensions(this DirectoryInfo dir, params string[] extensions)
        {
            if (extensions == null)
                throw new ArgumentNullException("extensions");
            IEnumerable<FileInfo> files = dir.EnumerateFiles();
            return files.Where(f => extensions.Contains(f.Extension));
        }


        public static List<string> GetServerFileName (string url)
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

        

        public static void SendFile()
        {
            //send file from local to server
            DirectoryInfo locationPath = new DirectoryInfo(localPath);
            var filesLocal = locationPath.GetFilesByExtensions(".xlsx", "xls");
            var fileServerName = GetServerFileName(urlDestination);
            foreach (var file in filesLocal)
            {
                try
                {
                    if (!fileServerName.Contains(file.Name))
                    {
                        var a = new Uri(String.Format("{0}{1}", urlDestination, file.Name));
                        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(a);
                        request.Credentials = new NetworkCredential(accountFtp, passwordFtp);
                        request.Method = WebRequestMethods.Ftp.UploadFile;
                        using (Stream fileStream = File.OpenRead(localPath + "\\" + file.Name))
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
            DirectoryInfo locationPath = new DirectoryInfo(localPath);
            var filesLocal = locationPath.GetFilesByExtensions(".xlsx", "xls");
            var fileServerName = GetServerFileName(urlDestination);
            
            //foreach(var fileServer)
        }
    }
}
