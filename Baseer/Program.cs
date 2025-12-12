using Baseer;
using Baseer.Data;
using Baseer.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient(); // Needed for HttpClient
builder.Services.AddScoped<IOpenAiService>(sp =>
    new OpenAiService(
        sp.GetRequiredService<HttpClient>(),
        builder.Configuration["OpenAI:ApiKey"],
        "gpt-4" // or gpt-4o / gpt-4-32k
    )
);

builder.Services.AddScoped<EmergencyService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Baseer API V1");
});
app.UseHttpsRedirection();
app.MapControllers();
app.Run();