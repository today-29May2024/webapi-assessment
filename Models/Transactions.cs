using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace AssessmentInnocel.Models;

public class Transaction
{
    [Required(ErrorMessage = "Partnerkey is Required."), StringLength(50)]
    public string PartnerKey { get; set; }
    [Required(ErrorMessage = "PartnerRefNo is Required."), StringLength(50)]
    public string PartnerRefNo { get; set; }
    [Required(ErrorMessage = "PartnerPassword is Required."), StringLength(50)]
    public string PartnerPassword { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "The TotalAmount only allow positive value.")]
    public long TotalAmount { get; set; }
    public ICollection<ItemDetail> Items { get; set; } = new List<ItemDetail>();
    [Required(ErrorMessage = "Timestamp is Required.")]
    public string Timestamp { get; set; }
    [Required(ErrorMessage = "Sig is Required.")]
    public string Sig { get; set; }
}