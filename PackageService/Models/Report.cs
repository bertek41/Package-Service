using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace PackageService.Models
{
    [Table("Reports")]
    public class Report
    {
        [Key]
        [Display(Name = "Rapor No")]
        public int Id { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Tarih")]
        public DateTime Date { get; set; }

        [Display(Name = "Saat")]
        public string Time { get; set; }

        [Display(Name = "Araç")]
        public int VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        [Display(Name = "Araç")]
        public virtual Vehicle Vehicle { get; set; }

        [Display(Name = "Adres")]
        public String Address { get; set; }

        [Display(Name = "Ürünler")]
        public string Items { get; set; }

        [Display(Name = "Sipariş Tutarı")]
        public double Total { get; set; }
    }
}
