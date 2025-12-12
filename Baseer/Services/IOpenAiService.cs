using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Baseer
{
    public interface IOpenAiService
    {
        Task<EmergencyReportDto?> AnalyzeReport(string? description,string? imagePath);
    }

    public class OpenAiService : IOpenAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;

        public OpenAiService(HttpClient httpClient, string apiKey, string model = "gpt-4")
        {
            _httpClient = httpClient;
            _apiKey = apiKey;
            _model = model;
        }

public async Task<EmergencyReportDto?> AnalyzeReport(string? description,string? imagePath)
{
    string prompt = "";

    if (!string.IsNullOrWhiteSpace(description) && string.IsNullOrWhiteSpace(imagePath))
    {
        // Text-only prompt
        prompt = $@"
تحليل النص التالي لتصنيف حالة طارئة:
'{description}'

قواعد تحديد الجهة المعنية (Official Saudi Agencies):

1- تعطّل سيارة أو نفاد بنزين → ""أمن الطرق""
2- حادث مروري → ""المرور""
3- حريق → ""الدفاع المدني""
4- تسوّل → ""الشرطة"" أو ""الأمن العام""
5- اشتباه مخدرات → ""مكافحة المخدرات""

الرجاء تصنيف البلاغ وإعطاء JSON يحتوي على الحقول التالية فقط:

{{
  ""EmergencyType"": """",
  ""Severity"": """",
  ""Priority"": """",
  ""ResponsibleAgency"": """",
  ""location"": """",
  ""people_affected"": """",
  ""resources_needed"": """"
}}

- استخدم أسماء الجهات الرسمية في السعودية فقط كما هو محدد أعلاه.
- أعطِ النتيجة في JSON **بدون أي نص إضافي** خارج JSON.
- لا تغير ترتيب الحقول أو أسماءها.
";
    }
    else if (!string.IsNullOrWhiteSpace(imagePath))
    {
        // Image prompt (optional description included)
        prompt = $@"
تحليل الصوره {(description != null ? $"ومع النص: '{description}'" : "")} واستخراج نوع البلاغ ومدى خطورته ومن هي الجهه المختصه:

قواعد تحديد الجهة المعنية (Official Saudi Agencies):

1- تعطّل سيارة أو نفاد بنزين → ""أمن الطرق""
2- حادث مروري → ""المرور""
3- حريق → ""الدفاع المدني""
4- تسوّل → ""الشرطة"" أو ""الأمن العام""
5- اشتباه مخدرات → ""مكافحة المخدرات""

الرجاء تصنيف البلاغ وإعطاء JSON يحتوي على الحقول التالية فقط:

{{
  ""EmergencyType"": """",
  ""Severity"": """",
  ""Priority"": """",
  ""ResponsibleAgency"": """",
  ""location"": """",
  ""people_affected"": """",
  ""resources_needed"": """"
}}

- استخدم أسماء الجهات الرسمية في السعودية فقط كما هو محدد أعلاه.
- أعطِ النتيجة في JSON **بدون أي نص إضافي** خارج JSON.
- لا تغير ترتيب الحقول أو أسماءها.
";
    }
    else
    {
        throw new ArgumentException("Either description or imagePath must be provided.");
    }
    prompt = prompt.Replace("{description}", description);

    // Build request
    var requestBody = new
    {
        model = _model,
        messages = new[]
        {
            new { role = "system", content = "أنت مساعد AI لتصنيف الحالات الطارئة في السعودية." },
            new { role = "user", content = prompt } 
        }
    };

    var jsonBody = JsonSerializer.Serialize(requestBody);
    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

    var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    request.Content = content;

    // Send request
    var response = await _httpClient.SendAsync(request);
    response.EnsureSuccessStatusCode();

    var responseJson = await response.Content.ReadAsStringAsync();

    // Parse JSON to extract text from OpenAI
    using var doc = JsonDocument.Parse(responseJson);
    var text = doc.RootElement
        .GetProperty("choices")[0]
        .GetProperty("message")
        .GetProperty("content")
        .GetString();

    if (string.IsNullOrWhiteSpace(text))
        return null;

    // قص JSON فقط لتجنب نصوص إضافية من AI
    int start = text.IndexOf('{');
    int end = text.LastIndexOf('}');
    if (start >= 0 && end > start)
    {
        string jsonOnly = text.Substring(start, end - start + 1);

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var report = JsonSerializer.Deserialize<EmergencyReportDto>(jsonOnly, options);
            if (report != null)
            {
                report.Description = description;
                report.CreatedAt = DateTime.UtcNow;
            }
            return report;
        }
        catch (JsonException)
        {
            Console.WriteLine("Failed to parse AI response:");
            Console.WriteLine(jsonOnly);
            return null;
        }
    }
    else
    {
        Console.WriteLine("No valid JSON found in AI response:");
        Console.WriteLine(text);
        return null;
    }
}

    }

    public class EmergencyReportDto
    {
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string EmergencyType { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string ResponsibleAgency { get; set; } = string.Empty;
        public string ResourcesNeeded { get; set; } = string.Empty;
        public Dictionary<string, string> AdditionalInfo { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string? PeopleAffected { get; set; }
        public string? Location { get; set; }
    }
    
}
