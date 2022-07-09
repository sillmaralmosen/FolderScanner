using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace FolderScanner.Models.Response
{
    public class FolderResponse
    {
       public List<Action> Actions { get; set; }

        public FolderResponse()=>
            Actions = new List<Action>();

        public void AddAction(string type,string path, int? version = null)
        {
            Actions.Add(new Action() { ActionType = type, Path = path, Version = version });
        }

    }

    public class Action
    {
        public string ActionType { get; set; }
        public string Path { get; set; }
        public int? Version { get; set; }
    }

    public static class ActionType
    {
        public const string Modified = "Modified";
        public const string Added = "Added";
        public const string Deleted = "Deleted";
    }

    //[JsonConverter(typeof(StringEnumConverter))]
    //public enum ActionType
    //{
    //    [EnumMember(Value = "Added")]
    //    Added,
    //    [EnumMember(Value = "Modified")]
    //    Modified,
    //    [EnumMember(Value = "Deleted")]
    //    Deleted
    //}
}
