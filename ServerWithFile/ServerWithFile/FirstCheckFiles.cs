using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ServerWithFile
{
    class FirstCheckFiles
    {
        public FirstCheckFiles(List<FileStruct> filesPathsAndTimeCreateOrChangeFiles, Socket listener)
        {
            this.filesPathsAndTimeCreateOrChangeFiles = filesPathsAndTimeCreateOrChangeFiles;
            this.listener = listener;
        }
        private StringBuilder data = new StringBuilder();
        private byte[] buffer;
        const int size = 256;
        private Socket listener;
        
        List<FileStruct> filesPathsAndTimeCreateOrChangeFiles;


        public void CheckFile()
        {
            var filesInStringBuilder = CreateStringWithFilesPathsAndTime();
            SendMessage(filesInStringBuilder.ToString());
            AnswerClient();
            if (data.ToString() != "?")
            {
                var nonClientFiles = CreateNewStringArrayWithChangeDirectory();
                SendFiles(nonClientFiles);
            }
        }
        private string[] CreateNewStringArrayWithChangeDirectory()
        {
            var nonClientFiles = Split();
            var nonClientFilesNew = new string[nonClientFiles.Length - 1];
            for (int i = 0; i < nonClientFiles.Length - 1; i++) // why -1?
            {
                nonClientFilesNew[i] = ChangeDirectory(nonClientFiles[i]);
            }
            return nonClientFilesNew;
        }
        private string ChangeDirectory(string filePath)
        {
            var filePathNew = new StringBuilder();
            var nameDirectoryArray = new string[1]; // string[] nameDirectoryArray = {"ClientDirectory"};
            nameDirectoryArray[0] = "ClientDirectory";
            var withoutNameDirectory = filePath.Split(nameDirectoryArray, StringSplitOptions.None);
            filePathNew.Append(withoutNameDirectory[0]);
            filePathNew.Append("ServerDirectory");
            filePathNew.Append(withoutNameDirectory[1]);
            return filePathNew.ToString();
        }
        private void SendFilesPathsAndTimeCreateOrChangeFiles()
        {

        }
        private void SendFiles(string[] nonClientFiles)
        {
            foreach (var nonClientFile in nonClientFiles)
            {
                var file = File.ReadAllText(nonClientFile);
                if (file.Length != 0)
                {
                    SendMessage(file);
                }
                else
                {
                    SendMessage("?");
                }
                AnswerClient();
                if (data.ToString() == "?")
                {
                    continue;
                }
                else
                {
                    throw new Exception();
                }
            }
        }
        private StringBuilder CreateStringWithFilesPathsAndTime()
        {
            StringBuilder filesAndPathsTimeInStringBuilder = new StringBuilder();
            foreach (var filePathAndTimeCreateOrChangeFile in filesPathsAndTimeCreateOrChangeFiles)
            {
                filesAndPathsTimeInStringBuilder.Append($"{filePathAndTimeCreateOrChangeFile.filePath}?");
            }
            foreach (var filePathAndTimeCreateOrChangeFile in filesPathsAndTimeCreateOrChangeFiles)
            {
                filesAndPathsTimeInStringBuilder.Append($"{filePathAndTimeCreateOrChangeFile.timeCreateOrChangeFile}*");
            }
            return filesAndPathsTimeInStringBuilder;
        }
        private void AnswerClient()
        {
            buffer = new byte[size];
            data = new StringBuilder();
            do
            {
                var sizeReceivedBuffer = listener.Receive(buffer);
                data.Append(Encoding.ASCII.GetString(buffer, 0, sizeReceivedBuffer));
            } while (listener.Available != 0);
        }
        private void SendMessage(string message)
        {
            listener.Send(Encoding.ASCII.GetBytes(message));
        }
        private string[] Split()
        {
            return data.ToString().Split('?');
        }

    }
}
