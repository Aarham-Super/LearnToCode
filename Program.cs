var builder = WebApplication.CreateBuilder(args);

#region Core Framework
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddLogging();
#endregion

#region HTTP Clients
builder.Services.AddHttpClient<LearnToCode.Services.GeminiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
});
#endregion

#region Application Services

// Core AI Orchestrator
builder.Services.AddScoped<LearnToCode.Services.AIService>();

// AI Provider
builder.Services.AddScoped<LearnToCode.Services.GeminiService>();

// Language System
builder.Services.AddSingleton<LearnToCode.Services.LanguageDetector>();

// Code Processing
builder.Services.AddScoped<LearnToCode.Services.CodeFixService>();
builder.Services.AddScoped<LearnToCode.Services.TutorService>();

// Language Modules
builder.Services.AddScoped<LearnToCode.Services.SqlService>();
builder.Services.AddScoped<LearnToCode.Services.AclService>();

// Learning System
builder.Services.AddSingleton<LearnToCode.Services.LearningProgressStore>();

#endregion

var app = builder.Build();

#region Middleware Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();
#endregion

#region Endpoints
app.MapControllers();
app.MapRazorPages();
#endregion

app.Run();