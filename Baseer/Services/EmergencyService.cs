using Baseer.Data;
using Baseer.Models;
using Microsoft.EntityFrameworkCore;

namespace Baseer.Services
{
    public class EmergencyService
    {
        private readonly AppDbContext _db;
        private readonly IOpenAiService _aiService;

        public EmergencyService(AppDbContext db, IOpenAiService aiService)
        {
            _db = db;
            _aiService = aiService;
        }

        public async Task<EmergencyReport?> CreateReportAsync(
            string? reporterName,
            string? phoneNumber,
            string? nationalId,
            string? description,
            string? imagePath)
        {
            
            
            // 1️⃣ Analyze report via AI
            var aiResult = await _aiService.AnalyzeReport(description,imagePath);
            if (aiResult == null)
                return null;

            // 2️⃣ Check if reporter exists
            var reporter = await _db.Reporters
                .FirstOrDefaultAsync(r =>
                    (!string.IsNullOrEmpty(phoneNumber) && r.PhoneNumber == phoneNumber) ||
                    (!string.IsNullOrEmpty(nationalId) && r.NationalId == nationalId));

            if (reporter == null)
            {
                reporter = new Reporter
                {
                    Name = reporterName,
                    PhoneNumber = phoneNumber,
                    NationalId = nationalId,
                    CreatedAt = DateTimeOffset.UtcNow
                };
                _db.Reporters.Add(reporter);
                await _db.SaveChangesAsync();
            }

            // 3️⃣ Map AI result to EmergencyReport
            var report = new EmergencyReport
            {
                ReporterId = reporter.ReporterId,
                Description = description,
                EmergencyType = aiResult.EmergencyType,
                Severity = aiResult.Severity,
                Priority = aiResult.Priority,
                ResponsibleAgency = aiResult.ResponsibleAgency,
                Location = aiResult.Location,
                PeopleAffected = aiResult.PeopleAffected,
                ResourcesNeeded = aiResult.ResourcesNeeded,
                CreatedAt = DateTimeOffset.UtcNow,
                Status = "Pending",
                ImageUrl = imagePath, // save image path

            };

            _db.EmergencyReports.Add(report);
            await _db.SaveChangesAsync();

            return report;
        }
        public async Task<List<EmergencyReportDtoResponse>> GetAllReportsAsync()
        {
            var reports = await _db.EmergencyReports
                .Include(r => r.Reporter)  // join reporter
                .ToListAsync();

            // Map to DTO
            return reports.Select(r => new EmergencyReportDtoResponse
            {
                Id = r.Id,
                Description = r.Description,
                EmergencyType = r.EmergencyType,
                Severity = r.Severity,
                Priority = r.Priority,
                ResponsibleAgency = r.ResponsibleAgency,
                Location = r.Location,
                PeopleAffected = r.PeopleAffected,
                ResourcesNeeded = r.ResourcesNeeded,
                CreatedAt = r.CreatedAt,
                Status = r.Status,
                ReporterName = r.Reporter.Name,
                ReporterPhone = r.Reporter.PhoneNumber,
                ReporterNationalId = r.Reporter.NationalId
            }).ToList();
        }

    }
    public class EmergencyReportDtoResponse
    {
        public long Id { get; set; }
        public string Description { get; set; } = null!;
        public string EmergencyType { get; set; } = null!;
        public string Severity { get; set; } = null!;
        public string Priority { get; set; } = null!;
        public string ResponsibleAgency { get; set; } = null!;
        public string? Location { get; set; }
        public string? PeopleAffected { get; set; }
        public string? ResourcesNeeded { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string Status { get; set; } = "تم الاستلام";

        // Reporter info
        public string ReporterName { get; set; } = null!;
        public string? ReporterPhone { get; set; }
        public string? ReporterNationalId { get; set; }
    }
}
