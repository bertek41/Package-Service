using PackageService.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace PackageService.Models
{
    [Table("Vehicles")]
    public class Vehicle
    {
        [Key]
        [Display(Name = "Araç No")]
        public int Id { get; set; }
        [Display(Name = "İsim")]
        [Required(ErrorMessage = "{0} alanı boş bırakılamaz.")]
        [MaxLength(50, ErrorMessage = "İsim 50 karakteri geçemez.")]
        [NotNull]
        public required string Name { get; set; }

        [Display(Name = "Ürünler")]
        [AllowNull]
        public string? Items { get; set; }

        [AllowNull]
        [Display(Name = "Adres")]
        public string? Address { get; set; }

        [AllowNull]
        public virtual ICollection<Report>? Reports { get; set; }
    }
}
