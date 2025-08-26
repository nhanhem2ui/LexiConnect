using BusinessObjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Course
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CourseId { get; set; }

    [Required, MaxLength(20)]
    public string CourseCode { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string CourseName { get; set; } = string.Empty;

    [ForeignKey(nameof(Major))]
    public int MajorId { get; set; }
    public virtual Major Major { get; set; }

    public byte? Semester { get; set; }
    public int? AcademicYear { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public virtual ICollection<Document> Documents { get; set; } = new List<Document>();
}
