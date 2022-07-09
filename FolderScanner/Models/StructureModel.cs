using Newtonsoft.Json;

namespace FolderScanner.Models
{
    public abstract class BaseModel
    {
        public string Path { get; set; }  
    }

    public class Filemodel : BaseModel
    {
        public Filemodel (string path, byte[] hash)
        {
            Path = path;
            Hash = hash;
            Version = 1;
        }

        public Filemodel(string path)
        {
            Path = path;
        }

        [JsonConstructor]
        public Filemodel(string path, byte[]? hash, int? version)
        {
            Path = path;
            Hash = hash;
            Version = version;
        }

        public byte[]? Hash { get; set; }
        public int? Version { get; set; }
    }

    public class FolderModel : BaseModel
    { 
        public List<Filemodel>? ChildrenFiles { get; set; }
    }

}
