﻿using FolderScanner.Models;
using FolderScanner.Models.Response;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;


namespace FolderScanner.Services
{
    public class FolderScanService : IFolderScanService
    {
        public static string programTempPath;

        public async Task<FolderResponse> Compare(string path)
        {
            if (!Directory.Exists(path))
                throw new Exception("Directory not exists");

            programTempPath = AppDomain.CurrentDomain.BaseDirectory + "temp\\";
            var response = new FolderResponse();

            if (!Directory.Exists(programTempPath))
                Directory.CreateDirectory(programTempPath);

            string[] tempFiles = await Task.Run<string[]>(() => Directory.GetFileSystemEntries(programTempPath, "*", SearchOption.AllDirectories));

            var folderTempModels = new List<KeyValuePair<string,FolderModel>>(); //KeyValuePair<FolderModel, string>  -- ošetřit cestu pro reuložení

     
            await tempFiles.ForEachAsync(async x => folderTempModels.Add(new KeyValuePair<string, FolderModel>(x,await Task.Run(async () => JsonConvert.DeserializeObject<FolderModel>(await File.ReadAllTextAsync(x))))));


            var actualFolderModel = await Scan(path);

            if (!folderTempModels.Where(x => x.Value.Path == path).Any())
            {
                File.WriteAllText(programTempPath + "Temp" + (tempFiles.Count() + 1) + ".json", JsonConvert.SerializeObject(actualFolderModel));
            }
            else
            {
                var oldFolderModel = folderTempModels.Where(x => x.Value.Path == path).FirstOrDefault();

                actualFolderModel.ChildrenFiles.Where(x => oldFolderModel.Value.ChildrenFiles.Where(y => y.Path == x.Path && x.Hash != null && y.Hash != null && !FilesAreEqual_Hash(y.Hash, x.Hash)).Any()).ToList()
                    .ForEach(x =>
                    {
                        x.Version = oldFolderModel.Value.ChildrenFiles.Where(y => y.Path == x.Path).FirstOrDefault().Version + 1;
                        response.AddAction(ActionType.Modified, x.Path, x.Version);
                    });

                oldFolderModel.Value.ChildrenFiles.Where(x => !actualFolderModel.ChildrenFiles.Where(y => y.Path == x.Path).Any()).ToList()
                          .ForEach(x =>
                          {
                              response.AddAction(ActionType.Deleted, x.Path);
                          });

                actualFolderModel.ChildrenFiles.Where(x => !oldFolderModel.Value.ChildrenFiles.Where(y => y.Path == x.Path).Any()).ToList()
                  .ForEach(x =>
                  {
                      response.AddAction(ActionType.Added, x.Path, x.Version);
                  });

                File.WriteAllText(oldFolderModel.Key, JsonConvert.SerializeObject(actualFolderModel));   
            }
            return response;
        }

   

        public async Task<FolderModel> Scan(string path)
        {
           var folderModel = new FolderModel()
           {
                Path = path
           };
            
            var childrenFiles = new List<Filemodel>();
  
            string[] files = await Task.Run<string[]>(() => Directory.GetFileSystemEntries(path, "*", SearchOption.AllDirectories));

            files.ForEach( x =>
            {
                if (File.GetAttributes(x).HasFlag(FileAttributes.Directory))
                {
                    childrenFiles.Add(new Filemodel(x));
                }
                else 
                {
                    var fi1e = new FileInfo(x);
                    byte[] Hash = MD5.Create().ComputeHash(fi1e.OpenRead());
                    childrenFiles.Add(new Filemodel(x, Hash));
                }  
            });

            folderModel.ChildrenFiles = childrenFiles;

            return folderModel;

        }

        static bool FilesAreEqual_Hash(byte[] firstHash, byte[] secondHash)
        {
            for (int i = 0; i < firstHash.Length; i++)
            {
                if (firstHash[i] != secondHash[i])
                    return false;
            }
            return true;
        }
    }
}