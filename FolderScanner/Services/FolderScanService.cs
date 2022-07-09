using FolderScanner.Models;
using FolderScanner.Models.Response;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;


namespace FolderScanner.Services
{
    public class FolderScanService : IFolderScanService
    {
        public static string? programTempPath;

        public async Task<FolderResponse> Compare(string path)
        {
            if (!Directory.Exists(path))
                throw new Exception("Directory not exists");

            programTempPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");

            if (!Directory.Exists(programTempPath))
            {
                try
                {
                    Directory.CreateDirectory(programTempPath);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                };
            }

            string[] tempFiles = await Task.Run<string[]>(() => Directory.GetFileSystemEntries(programTempPath, "*", SearchOption.AllDirectories));

            var folderTempModels = new List<KeyValuePair<string,FolderModel>>();

            await tempFiles.ForEachAsync(async x =>
            {
                var value = JsonConvert.DeserializeObject<FolderModel>(await File.ReadAllTextAsync(x));
                if (value != null)
                    folderTempModels.Add(new KeyValuePair<string, FolderModel>(x, value));
            });

            var actualFolderModel = await Scan(path);

            var response = new FolderResponse();

            if (!folderTempModels.Where(x => x.Value.Path == path).Any())
            {
                try
                {
                    File.WriteAllText(Path.Combine(programTempPath,"Temp" + (tempFiles.Count() + 1) + ".json"), JsonConvert.SerializeObject(actualFolderModel));
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                };
            }
            else
            {
                var previousFolderModel = folderTempModels.Where(x => x.Value.Path == path).FirstOrDefault();
                if (previousFolderModel.Value?.ChildrenFiles != null && actualFolderModel?.ChildrenFiles != null)
                {
                    actualFolderModel.ChildrenFiles
                        .Where(x => previousFolderModel.Value.ChildrenFiles.Where(y => y.Path == x.Path && x.Hash != null && y.Hash != null && !FilesAreEqualHash(y.Hash, x.Hash)).Any())
                        .ToList()
                        .ForEach(x =>
                        {
                            x.Version = (previousFolderModel.Value.ChildrenFiles.Where(y => y.Path == x.Path).FirstOrDefault()?.Version ?? 0) + 1;
                            response.AddAction(ActionType.Modified, x.Path, x.Version);
                        });

                    previousFolderModel.Value.ChildrenFiles
                        .Where(x => !actualFolderModel.ChildrenFiles.Where(y => y.Path == x.Path).Any())
                        .ToList()
                        .ForEach(x =>
                        {
                            response.AddAction(ActionType.Deleted, x.Path);
                        });

                    actualFolderModel.ChildrenFiles.Where(x => !previousFolderModel.Value.ChildrenFiles.Where(y => y.Path == x.Path).Any())
                        .ToList()
                        .ForEach(x =>
                        {
                            response.AddAction(ActionType.Added, x.Path, x.Version);
                        });
                }

                try
                {
                    File.WriteAllText(previousFolderModel.Key, JsonConvert.SerializeObject(actualFolderModel));
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                };
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

        static bool FilesAreEqualHash(byte[] firstHash, byte[] secondHash)
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
