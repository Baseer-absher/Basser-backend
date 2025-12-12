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
        string? imagePath = null;

        // Save image if provided
        if (dto.Image != null && dto.Image.Length > 0)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, fileName);
            await using var stream = new FileStream(filePath, FileMode.Create);
            await dto.Image.CopyToAsync(stream);

            imagePath = filePath; // store path in DB
        }
        var report = await _service.CreateReportAsync(dto.Name, dto.PhoneNumber, dto.NationalId, dto.Description,imagePath);
        if (report == null) return BadRequest("Failed to analyze report.");
        return Ok(report);
    }
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var reports = await _service.GetAllReportsAsync();
        return Ok(reports);
    }
    // [HttpPost("create-with-image")]
    // public async Task<IActionResult> CreateWithImage([FromForm] CreateReportWithImageDto dto)
    // {
    //     // Save uploaded image to server / cloud
    //     string? imageUrl = null;
    //     if (dto.Image != null && dto.Image.Length > 0)
    //     {
    //         var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.Image.FileName)}";
    //         var filePath = Path.Combine("Uploads", fileName);
    //         Directory.CreateDirectory("Uploads");
    //         await using var stream = new FileStream(filePath, FileMode.Create);
    //         await dto.Image.CopyToAsync(stream);
    //         imageUrl = filePath; // or full URL if using cloud
    //     }
    //
    //     // Analyze image via OpenAI Vision
    //     var aiResult = await _aiService.AnalyzeImageReport(dto.Description, imageUrl, dto.Image);
    //
    //     // Save reporter + EmergencyReport
    //     var report = await _emergencyService.CreateReportAsync(
    //         dto.Name,
    //         dto.PhoneNumber,
    //         dto.NationalId,
    //         dto.Description,
    //         imageUrl,
    //         aiResult
    //     );
    //
    //     return Ok(report);
    // }

    public class CreateReportDto
    {
        public string Name { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? NationalId { get; set; }
        public string Description { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }
    public class CreateReportWithImageDto
    {
        public string Name { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? NationalId { get; set; }
        public string Description { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
    }

}