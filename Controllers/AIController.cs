using LearnToCode.Models;
using LearnToCode.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearnToCode.Controllers;

[ApiController]
[Route("api/ai")]
public sealed class AIController : ControllerBase
{
    private readonly AIService _aiService;

    public AIController(AIService aiService)
    {
        _aiService = aiService;
    }

    // ================= MAIN AI ENDPOINT =================
    [HttpPost("analyze")]
    public async Task<ActionResult<CodeResponse>> Analyze(
        [FromBody] CodeRequest request,
        CancellationToken cancellationToken)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Code))
        {
            return BadRequest(new
            {
                error = "Code cannot be empty"
            });
        }

        var response = await _aiService.AnalyzeAsync(request, cancellationToken);

        return Ok(response);
    }

    // ================= USER PROGRESS =================
    [HttpGet("progress/{userId}")]
    public async Task<ActionResult<UserProgress>> GetProgress(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return BadRequest(new
            {
                error = "UserId is required"
            });
        }

        var progress = await _aiService.GetProgressAsync(userId);

        return Ok(progress);
    }
}