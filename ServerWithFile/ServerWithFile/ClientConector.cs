﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerWithFile
{
    class ClientConector
    {
        public ClientConector(List<FileInformation> filesPathsAndTimeCreateOrChangeFiles, Action serverRun)
        {
            this.filesPathsAndTimeCreateOrChangeFiles = filesPathsAndTimeCreateOrChangeFiles;
            this.serverRun = serverRun;
            listenerSockets = new List<Socket>();
        }
        private StringBuilder data;
        private byte[] buffer;
        const int size = 256;
        private List<Socket> listenerSockets;
        Action serverRun;
        public List<FileInformation> filesPathsAndTimeCreateOrChangeFiles;

        public void NotifyClient(List<FileInformation> deletePathsFiles, List<FileInformation> newPathsFiles, List<FileInformation> changePathsFiles)
        {
            UpdateFilesPathAndTime(deletePathsFiles, newPathsFiles, changePathsFiles);
            SendNewFiles(deletePathsFiles, newPathsFiles, changePathsFiles);
        }
        private void SendNewFiles(List<FileInformation> deletePathsFiles, List<FileInformation> newPathsFiles, List<FileInformation> changePathsFiles)
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
        private void SendNewOrChangeFiles(string sendMessage, List<FileInformation> changePathsFiles)
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
            int j = 0;
            while (true)
            {
                var exception = false;
                for (int i = j; i < listenerSockets.Count; i++)
                {
                    try
                    {
                        AnswerClient(listenerSockets[i]);
                    }
                    catch (SocketException socketException)
                    {
                        (j, exception) = DisconectClient(socketException, i, j);
                    }
                }
                if (!exception)
                {
                    break;
                }
            }
        }
        private void SendFiles(string filePath)
        {
            var newFilePath = ChangeDirectoryToNormal(filePath);
            var file = File.ReadAllText(newFilePath);
            if (file.Length != 0)
            {
                SendMessageAllListener($"*{file}");
            }
            else
            {
                SendMessageAllListener("?");
            }
        }
        private string CreateStringFromList(List<FileInformation> somePathsFiles)
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
                        (j, exception) = DisconectClient(socketException, i, j);
                    }
                }
                if (!exception)
                {
                    break;
                }
            }
        }
        private (int j, bool exception) DisconectClient(SocketException socketException, int i, int j)
        {
            var errorCode = socketException.ErrorCode;
            if (errorCode == 10054 || errorCode == 10053)
            {
                listenerSockets[i].Close();
                listenerSockets.Remove(listenerSockets[i]);
                serverRun.Invoke();
                return (i, true);
            }
            else
            {
                Console.WriteLine(socketException.Message);
                return (j, false);
            }
        }
        private void UpdateFilesPathAndTime(List<FileInformation> deletePathsFiles, List<FileInformation> newPathsFiles, List<FileInformation> changePathsFiles)
        {
            foreach (var deletePathFile in deletePathsFiles)
            {
                filesPathsAndTimeCreateOrChangeFiles.Remove(deletePathFile);
            }
            foreach (var changePathFile in changePathsFiles)
            {
                filesPathsAndTimeCreateOrChangeFiles.Remove(changePathFile);
                filesPathsAndTimeCreateOrChangeFiles.Add(changePathFile);
            }
            foreach (var newPathFile in newPathsFiles)
            {
                filesPathsAndTimeCreateOrChangeFiles.Add(newPathFile);
            }
        }
        public void Run(Socket listener)
        {
            listenerSockets.Add(listener);
            Synchronizer synchronizer = new Synchronizer(filesPathsAndTimeCreateOrChangeFiles, listener);
            synchronizer.FileSynchronization();
        }
        private string[] Split()
        {
            return data.ToString().Split('?');
        }
        //private string ChangeDirectory(string filePath)
        //{
        //    var filePathNew = new StringBuilder();
        //    var nameDirectoryArray = new string[1];
        //    nameDirectoryArray[0] = "ServerDirectory";
        //    var withoutNameDirectory = filePath.Split(nameDirectoryArray, StringSplitOptions.None);
        //    filePathNew.Append(withoutNameDirectory[0]);
        //    filePathNew.Append("ClientDirectory");
        //    filePathNew.Append(withoutNameDirectory[1]);
        //    return filePathNew.ToString();
        //}
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
    }
}
