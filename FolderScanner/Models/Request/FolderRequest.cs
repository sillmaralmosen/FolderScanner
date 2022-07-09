using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace FolderScanner.Models.Request
{
    public class FolderRequest
    {
        [Required]
        public string Path { get; set; }
    }
}