using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace PackageService.Models
{
    public class VehicleItems
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Araç")]
        public int VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        [Display(Name = "Araç")]
        public virtual Vehicle Vehicle { get; set; }

        [Display(Name = "Eşya")]
        public int ItemId { get; set; }
        [ForeignKey("ItemId")]
        [Display(Name = "Eşya")]
        public virtual Item Item { get; set; }

        [DefaultValue(-1)]
        public int Amount { get; set; }
    }
}
