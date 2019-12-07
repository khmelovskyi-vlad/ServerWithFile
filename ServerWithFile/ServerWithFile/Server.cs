using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerWithFile
{
    class Server
    {
        private string pathToFolder = "D:\\temp\\ServerDirectory";
        public Server()
        {
            tcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var tcpEndPoint = new IPEndPoint(IPAddress.Any, port);
            tcpSocket.Bind(tcpEndPoint);
            tcpSocket.Listen(6);
            AddFilesAndThemTime();
            Action run = Run;
            clientConect = new ClientConector(filesPathsAndTimeCreateOrChangeFiles, run);
            fileDet = new FilesDetector(filesPathsAndTimeCreateOrChangeFiles, clientConect);
            Run();
            Detect();
        }
        private const int port = 2048;
        private Socket tcpSocket;
        List<FileInformation> filesPathsAndTimeCreateOrChangeFiles = new List<FileInformation>();
        ClientConector clientConect;
        FilesDetector fileDet;
        public void Run()
        {
            tcpSocket.BeginAccept(ar =>
            {
                try
                {
                    var listener = tcpSocket.EndAccept(ar);
                    clientConect.Run(listener);
                }
                catch (SocketException socketException)
                {
                    var errorCode = socketException.ErrorCode;
                    if (errorCode == 10054 || errorCode == 10053)
                    {
                        Run();
                    }
                    else
                    {
                        Console.WriteLine(socketException.Message);
                    }
                }
            }, tcpSocket);
        }
        AutoResetEvent waitFilesCheck = new AutoResetEvent(true);
        private bool enterClick = false;
        private void Detect()
        {
            ThreadPool.QueueUserWorkItem(x => Timer());
            while (true)
            {
                var key = Console.ReadKey(false);
                if (key.Key == ConsoleKey.Enter)
                {
                    enterClick = true;
                    waitFilesCheck.Set();
                    fileDet.Detect();
                }
            }
        }
        private void Timer()
        {
            while (true)
            {
                waitFilesCheck.WaitOne(10000);
                if (enterClick)
                {
                    enterClick = false;
                    continue;
                }
                fileDet.Detect();
            }
        }
        private void AddFilesAndThemTime()
        {
            List<string> filesPaths = new List<string>();
            filesPaths.AddRange(Directory.GetFiles(pathToFolder));
            for (int i = 0; i < filesPaths.Count; i++)
            {
                AddFilesAndThemTimeToList(filesPaths[i]);
            }
        }
        private void AddFilesAndThemTimeToList(string filePath)
        {
            var filePathNew = ChangeDirectory(filePath);
            FileInformation file = new FileInformation();
            file.filePath = filePathNew;
            file.timeCreateOrChangeFile = File.GetLastWriteTime(filePath);
            filesPathsAndTimeCreateOrChangeFiles.Add(file);
        }
        private string ChangeDirectory(string filePath)
        {
            var filePathNew = new StringBuilder();
            var nameDirectoryArray = new string[1];
            nameDirectoryArray[0] = "ServerDirectory";
            var withoutNameDirectory = filePath.Split(nameDirectoryArray, StringSplitOptions.None);
            filePathNew.Append(withoutNameDirectory[0]);
            filePathNew.Append("ClientDirectory");
            filePathNew.Append(withoutNameDirectory[1]);
            return filePathNew.ToString();
        }
    }
}
