using FolderScanner.Models;
using FolderScanner.Models.Request;
using FolderScanner.Models.Response;
using FolderScanner.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace FolderScanner.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IFolderScanService _folderScanService;

        public FileController(IFolderScanService folderScanService)
        {
            _folderScanService = folderScanService;
        }

        [HttpGet("{Path}/GetFilesChanges")]
        public async Task<FolderResponse> GetFilesChanges([FromRoute]FolderRequest request)=>await _folderScanService.Compare(request.Path); 
        
    }
}
