using Baseer;
using Baseer.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class EmergencyController : ControllerBase
{
    private readonly IOpenAiService _openAiService; 
    private readonly EmergencyService _service;


    public EmergencyController(IOpenAiService openAiService, EmergencyService service)
    {
        _openAiService = openAiService;
        _service = service;
    }

    [HttpPost("analyze")]
    public async Task<EmergencyReportDto?> Analyze([FromBody] string description,string? imagePath)
    {
        return await _openAiService.AnalyzeReport(description,imagePath);
    }
    [HttpPost("create")]
    public async Task<IActionResult> Create([FromForm] CreateReportDto dto)
    {
        List<string> imagePaths = new List<string>();

        if (dto.Images != null && dto.Images.Count > 0)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            Directory.CreateDirectory(folderPath);

            foreach (var image in dto.Images)
            {
                if (image != null && image.Length > 0)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);

                    await using var stream = new FileStream(filePath, FileMode.Create);
                    await image.CopyToAsync(stream);

                    imagePaths.Add(filePath); // store all saved paths
                }
            }
        }
        var report = await _service.CreateReportAsync(dto.Description,imagePaths,dto.EmergencyType,dto.LicensePlate,dto.Latitude,dto.Longitude);
        if (report == null) return BadRequest("Failed to analyze report.");
        return Ok(report);
    }
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var reports = await _service.GetAllReportsAsync();
        return Ok(reports);
    }

    public class CreateReportDto
    {
        public string Description { get; set; } = string.Empty;
        public List<IFormFile>? Images { get; set; }
        public EmergencyTypeEnum? EmergencyType { get; set; }
        public string? LicensePlate { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }
 


}