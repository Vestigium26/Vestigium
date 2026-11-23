using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Vestigium.Models.ViewModels
{
    public class ImportUsersVM
    {
        [Required]
        public IFormFile ExcelFile { get; set; } = null!;
    }
}