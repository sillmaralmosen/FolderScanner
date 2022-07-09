using FolderScanner.Models;
using FolderScanner.Models.Response;

namespace FolderScanner.Services
{
    public interface IFolderScanService
    {
        Task<FolderResponse> Compare(string path);
        Task<FolderModel> Scan(string path);
    }
}
