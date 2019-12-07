using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerWithFile
{
    class FilesDetector
    {
        private const string pathToFolder = "D:\\temp\\ServerDirectory";
        public FilesDetector(List<FileInformation> filesPathsAndTimeCreateOrChangeFiles, ClientConector clientConect)
        {
            this.filesPathsAndTimeCreateOrChangeFiles = filesPathsAndTimeCreateOrChangeFiles;
            this.clientConect = clientConect;
        }
        ClientConector clientConect;
        List<FileInformation> filesPathsAndTimeCreateOrChangeFiles;
        List<FileInformation> filesPathsAndTimeCreateOrChangeFilesNew;
        public void Detect()
        {
            var deletePathsFiles = new List<FileInformation>();
            var newPathsFiles = new List<FileInformation>();
            var changePathsFiles = new List<FileInformation>();

            var filesPathsNew = Directory.GetFiles(pathToFolder);

            filesPathsAndTimeCreateOrChangeFilesNew = new List<FileInformation>();
            foreach (var filePathNew in filesPathsNew)
            {
                AddFilesAndThemTimeToList(filePathNew, true, true);
            }

            (deletePathsFiles, newPathsFiles, changePathsFiles) = CreateDeleteAndNewPathsFilesList(filesPathsAndTimeCreateOrChangeFilesNew);
            clientConect.NotifyClient(deletePathsFiles, newPathsFiles, changePathsFiles);
        }
        //private string CreateStringFromPaths(List<FileStruct> pathFiles)
        //{
        //    StringBuilder stringBuilder = new StringBuilder();
        //    foreach (var pathFile in pathFiles)
        //    {
        //        stringBuilder.Append($"*{pathFile}");
        //    }
        //    return stringBuilder.ToString();
        //}
        private (List<FileInformation> deletePathsFiles, List<FileInformation> newPathsFiles, List<FileInformation> changePathsFiles) CreateDeleteAndNewPathsFilesList(List<FileInformation> filesPathsAndTimeCreateOrChangeFilesNew)
        {
            var deletePathsFiles = CreateDeletePathsFilesList(filesPathsAndTimeCreateOrChangeFilesNew);
            var (newPathsFiles, changePathsFiles) = CreateNewAndChangePathsFilesList(filesPathsAndTimeCreateOrChangeFilesNew);
            return (deletePathsFiles, newPathsFiles, changePathsFiles);
        }
        private (List<FileInformation> newPathsFiles, List<FileInformation> changePathsFiles) CreateNewAndChangePathsFilesList(List<FileInformation> filesPathsAndTimeCreateOrChangeFilesNew)
        {
            var newPathsFiles = new List<FileInformation>();
            var changePathsFiles = new List<FileInformation>();
            foreach (var filePathAndTimeCreateOrChangeFileNew in filesPathsAndTimeCreateOrChangeFilesNew)
            {
                var containFileTime = false;
                var containFilePath = false;
                foreach (var filePathAndTimeCreateOrChangeFile in filesPathsAndTimeCreateOrChangeFiles)
                {
                    if (filePathAndTimeCreateOrChangeFileNew.filePath == filePathAndTimeCreateOrChangeFile.filePath)
                    {
                        if (filePathAndTimeCreateOrChangeFileNew.timeCreateOrChangeFile == filePathAndTimeCreateOrChangeFile.timeCreateOrChangeFile)
                        {
                            containFileTime = true;
                            break;
                        }
                        clientConect.filesPathsAndTimeCreateOrChangeFiles.Remove(filePathAndTimeCreateOrChangeFile);
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
        private List<FileInformation> CreateDeletePathsFilesList(List<FileInformation> filesPathsAndTimeCreateOrChangeFilesNew)
        {
            var deletePathsFiles = new List<FileInformation>();
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
                clientConect.filesPathsAndTimeCreateOrChangeFiles.Remove(deletePathFile);
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
            FileInformation file = new FileInformation();
            file.filePath = filePathNew;
            file.timeCreateOrChangeFile = File.GetLastWriteTime(filePath);
            if (forNewFiles)
            {
                filesPathsAndTimeCreateOrChangeFilesNew.Add(file);
                return;
            }
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
    }
}
