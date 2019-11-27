using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServerWithFile
{
    class Files
    {
        public Files()
        {

        }
        /*
        private List<Socket> listenerSockets;
        private string pathToFolder = "D:\\temp\\ServerDirectory";
        private byte[] buffer;
        const int size = 256;
        private StringBuilder data;

        List<FileStruct> filesPathsAndTimeCreateOrChangeFiles = new List<FileStruct>();
        List<FileStruct> filesPathsAndTimeCreateOrChangeFilesNew;

        AutoResetEvent waitFilesCheck = new AutoResetEvent(true);
        private bool enterClick = false;
        private void CheckFiles()
        {
            ThreadPool.QueueUserWorkItem(x => ClickEnterToCheckFiles());
            while (true)
            {
                waitFilesCheck.WaitOne(10000);
                if (enterClick)
                {
                    enterClick = false;
                    continue;
                }
                FindUpdates();
            }
        }
        private void ClickEnterToCheckFiles()
        {
            while (true)
            {
                var key = Console.ReadKey(false);
                if (key.Key == ConsoleKey.Enter)
                {
                    enterClick = true;
                    waitFilesCheck.Set();
                    FindUpdates();
                }
            }
        }
        private void FindUpdates()
        {
            var deletePathsFiles = new List<FileStruct>();
            var newPathsFiles = new List<FileStruct>();
            var changePathsFiles = new List<FileStruct>();

            var filesPathsNew = Directory.GetFiles(pathToFolder);

            filesPathsAndTimeCreateOrChangeFilesNew = new List<FileStruct>();
            foreach (var filePathNew in filesPathsNew)
            {
                AddFilesAndThemTimeToList(filePathNew, true, true);
            }

            (deletePathsFiles, newPathsFiles, changePathsFiles) = CreateDeleteAndNewPathsFilesList(filesPathsAndTimeCreateOrChangeFilesNew);

            SendNewFiles(deletePathsFiles, newPathsFiles, changePathsFiles);

        }
        private void SendNewFiles(List<FileStruct> deletePathsFiles, List<FileStruct> newPathsFiles, List<FileStruct> changePathsFiles)
        {
            if (deletePathsFiles.Count != 0)
            {
                SendMessageAllListener("delete");
                SendMessageAllListener(CreateStringFromList(deletePathsFiles));
                AnswerAllListener();
            }
            if (newPathsFiles.Count != 0)
            {
                SendNewOrChangeFiles("new", newPathsFiles);
            }
            if (changePathsFiles.Count != 0)
            {
                SendNewOrChangeFiles("change", changePathsFiles);
            }
        }
        private void SendNewOrChangeFiles(string sendMessage, List<FileStruct> changePathsFiles)
        {
            SendMessageAllListener(sendMessage);
            AnswerAllListener();
            SendMessageAllListener(CreateStringFromList(changePathsFiles));
            AnswerAllListener();
            foreach (var newPathFile in changePathsFiles)
            {
                SendFiles(newPathFile.filePath);
                AnswerAllListener();
            }
        }
        private void AnswerAllListener()
        {
            foreach (var listenerSocket in listenerSockets)
            {
                AnswerClient(listenerSocket);
            }
        }
        private void SendFiles(string filePath)
        {
            var newFilePath = ChangeDirectoryToNormal(filePath);
            var file = File.ReadAllText(newFilePath);
            if (file.Length != 0)
            {
                SendMessageAllListener(file);
            }
            else
            {
                SendMessageAllListener("?");
            }
        }
        private string CreateStringFromList(List<FileStruct> somePathsFiles)
        {
            var somePathsFilesStingBuilder = new StringBuilder();
            foreach (var somePathFile in somePathsFiles)
            {
                somePathsFilesStingBuilder.Append($"{somePathFile.filePath}?");
            }
            return somePathsFilesStingBuilder.ToString();
        }
        private void SendMessageAllListener(string message)
        {
            int j = 0;
            while (true)
            {
                var exception = false;
                for (int i = j; i < listenerSockets.Count; i++)
                {
                    try
                    {
                        SendMessage(message, listenerSockets[i]);
                    }
                    catch (SocketException socketException)
                    {
                        var errorCode = socketException.ErrorCode;
                        if (errorCode == 10054)
                        {
                            listenerSockets.Remove(listenerSockets[i]);
                            j = i;
                            exception = true;
                            Run();
                        }
                        else
                        {
                            Console.WriteLine(socketException.Message); // This
                        }
                        break;
                    }
                }
                if (!exception)
                {
                    break;
                }
            }
        }
        private (List<FileStruct> deletePathsFiles, List<FileStruct> newPathsFiles, List<FileStruct> changePathsFiles) CreateDeleteAndNewPathsFilesList(List<FileStruct> filesPathsAndTimeCreateOrChangeFilesNew)
        {
            var deletePathsFiles = CreateDeletePathsFilesList(filesPathsAndTimeCreateOrChangeFilesNew);
            var (newPathsFiles, changePathsFiles) = CreateNewAndChangePathsFilesList(filesPathsAndTimeCreateOrChangeFilesNew);
            return (deletePathsFiles, newPathsFiles, changePathsFiles);
        }
        private (List<FileStruct> newPathsFiles, List<FileStruct> changePathsFiles) CreateNewAndChangePathsFilesList(List<FileStruct> filesPathsAndTimeCreateOrChangeFilesNew)
        {
            var newPathsFiles = new List<FileStruct>();
            var changePathsFiles = new List<FileStruct>();
            foreach (var filePathAndTimeCreateOrChangeFileNew in filesPathsAndTimeCreateOrChangeFilesNew)
            {
                var containFileTime = false; // or bool containFileTime, containFilePath = false;
                var containFilePath = false;
                // Enter it`s OK?
                foreach (var filePathAndTimeCreateOrChangeFile in filesPathsAndTimeCreateOrChangeFiles)
                {
                    if (filePathAndTimeCreateOrChangeFileNew.filePath == filePathAndTimeCreateOrChangeFile.filePath)
                    {
                        if (filePathAndTimeCreateOrChangeFileNew.timeCreateOrChangeFile == filePathAndTimeCreateOrChangeFile.timeCreateOrChangeFile)
                        {
                            containFileTime = true;
                            break;
                        }
                        filesPathsAndTimeCreateOrChangeFiles.Remove(filePathAndTimeCreateOrChangeFile);
                        containFilePath = true;
                        break;
                    }
                }
                if (!containFileTime)
                {
                    AddFilesAndThemTimeToList(filePathAndTimeCreateOrChangeFileNew.filePath, false, false);
                    if (containFilePath)
                    {
                        changePathsFiles.Add(filePathAndTimeCreateOrChangeFileNew);
                        continue;
                    }
                    newPathsFiles.Add(filePathAndTimeCreateOrChangeFileNew);
                }
            }
            return (newPathsFiles, changePathsFiles);
        }
        private List<FileStruct> CreateDeletePathsFilesList(List<FileStruct> filesPathsAndTimeCreateOrChangeFilesNew)
        {
            var deletePathsFiles = new List<FileStruct>();
            foreach (var filePathAndTimeCreateOrChangeFile in filesPathsAndTimeCreateOrChangeFiles)
            {
                var containFile = false;
                foreach (var filePathAndTimeCreateOrChangeFileNew in filesPathsAndTimeCreateOrChangeFilesNew)
                {
                    if (filePathAndTimeCreateOrChangeFileNew.filePath == filePathAndTimeCreateOrChangeFile.filePath)
                    {
                        containFile = true;
                        break;
                    }
                }
                if (!containFile)
                {
                    deletePathsFiles.Add(filePathAndTimeCreateOrChangeFile);
                }
            }
            foreach (var deletePathFile in deletePathsFiles)
            {
                filesPathsAndTimeCreateOrChangeFiles.Remove(deletePathFile);
            }
            return deletePathsFiles;
        }
        private void AddFilesAndThemTimeToList(string filePath, bool forNewFiles, bool needChangeDirectory)
        {
            var filePathNew = filePath;
            if (needChangeDirectory)
            {
                filePathNew = ChangeDirectory(filePath);
            }
            else
            {
                filePath = ChangeDirectoryToNormal(filePath);
            }
            FileStruct file = new FileStruct();
            file.filePath = filePathNew;
            file.timeCreateOrChangeFile = File.GetLastWriteTime(filePath);
            if (forNewFiles)
            {
                filesPathsAndTimeCreateOrChangeFilesNew.Add(file);
                return;
            }
            filesPathsAndTimeCreateOrChangeFiles.Add(file);
        }
        private void AddFilesAndThemTime()
        {
            List<string> filesPaths = new List<string>();
            filesPaths.AddRange(Directory.GetFiles(pathToFolder));
            for (int i = 0; i < filesPaths.Count; i++)
            {
                AddFilesAndThemTimeToList(filesPaths[i], false, true);
            }
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
        private string ChangeDirectoryToNormal(string filePath)
        {
            var filePathNew = new StringBuilder();
            var nameDirectoryArray = new string[1];
            nameDirectoryArray[0] = "ClientDirectory";
            var withoutNameDirectory = filePath.Split(nameDirectoryArray, StringSplitOptions.None);
            filePathNew.Append(withoutNameDirectory[0]);
            filePathNew.Append("ServerDirectory");
            filePathNew.Append(withoutNameDirectory[1]);
            return filePathNew.ToString();
        }


        private void AnswerClient(Socket listener)
        {
            buffer = new byte[size];
            data = new StringBuilder();
            do
            {
                var sizeReceivedBuffer = listener.Receive(buffer);
                data.Append(Encoding.ASCII.GetString(buffer, 0, sizeReceivedBuffer));
            } while (listener.Available > 0);
        }
        private void SendMessage(string message, Socket listener)
        {
            listener.Send(Encoding.ASCII.GetBytes(message));
        }
        private string[] Split()
        {
            return data.ToString().Split('?');
        }*/

    }
}
