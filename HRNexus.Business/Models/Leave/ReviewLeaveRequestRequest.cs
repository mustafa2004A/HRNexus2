using System.ComponentModel.DataAnnotations;

namespace HRNexus.Business.Models.Leave;

public sealed class ReviewLeaveRequestRequest
{
    [Range(1, int.MaxValue)]
    public int RequestStatusId { get; set; }

    [Range(1, int.MaxValue)]
    public int? ReviewedByUserId { get; set; }

    [StringLength(500)]
    public string? ReviewNotes { get; set; }
}
