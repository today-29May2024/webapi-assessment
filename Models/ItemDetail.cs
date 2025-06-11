using System.ComponentModel.DataAnnotations;

namespace AssessmentInnocel.Models;

public class ItemDetail
{
    [Required(ErrorMessage = "PartnetItemRef is Required."), StringLength(50)]
    public required string PartnerItemRef { get; set; }
    [Required(ErrorMessage = "Name is Required."), StringLength(100)]
    public required string Name { get; set; }
    [Range(1, 5, ErrorMessage = "The Qty value must be greater than 1 and less than 6.")]
    public int Qty { get; set; }
    [Range(0, 99999, ErrorMessage = "The UnitPrice value must be a positive value and not exceed than 5 digit")]
    public long UnitPrice { get; set; }
    
}