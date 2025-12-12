using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Baseer.Models
{
    public class Reporter
    {
        [Key]
        public long ReporterId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = null!;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [MaxLength(20)]
        public string? NationalId { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        // Navigation property
        public ICollection<EmergencyReport> EmergencyReports { get; set; } = new List<EmergencyReport>();
    }

    public class EmergencyReport
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public long ReporterId { get; set; }

        [ForeignKey(nameof(ReporterId))]
        [JsonIgnore]  // <-- Add this
        public Reporter Reporter { get; set; } = null!;

        [Required]
        public string Description { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string EmergencyType { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Severity { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string Priority { get; set; } = null!;

        [Required]
        [MaxLength(100)]
        public string ResponsibleAgency { get; set; } = null!;

        [MaxLength(200)]
        public string? Location { get; set; }

        [MaxLength(200)]
        public string? PeopleAffected { get; set; }

        [MaxLength(500)]
        public string? ResourcesNeeded { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";
        
        [MaxLength(500)]
        public string? ImageUrl { get; set; }
    }
}