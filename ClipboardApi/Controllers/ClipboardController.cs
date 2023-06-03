using ClipboardApi.Dtos;
using ClipboardApi.Models;
using ClipboardApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace ClipboardApi.Controllers;

[ApiController]
[Route("clipboard")]
public class ClipboardController : ControllerBase
{
    private readonly ClipboardService _clipboardService;

    public ClipboardController(ClipboardService clipboardService)
    {
        _clipboardService = clipboardService;
    }

    [HttpGet("/fetch/{userId}")]
    public async Task<ActionResult<List<Record>>> GetRecords(string userId)
    {
        var records = await _clipboardService.GetRecordsByUserId(userId);

        if (records == null)
        {
            return NotFound();
        }
        return Ok(records);
    }
    
    [HttpPost("/post")]
    public async Task<ActionResult<Record>> PostClipboardContent(PostClipboardContent postClipboardContent)
    {
        var userId = postClipboardContent.UserId;
        var type = postClipboardContent.Type;
        var content = postClipboardContent.Content;

        var clipboardId = await _clipboardService.GetClipboardByUserId(userId) ??
                        await _clipboardService.CreateClipboard(userId);

        var record = await _clipboardService.AddContentToClipboard(userId, clipboardId, type, content);

        return Ok(record);
    }
    
    [HttpDelete("/delete/{recordId:guid}")]
    public async Task<ActionResult<Record>> PostClipboardContent(Guid recordId)
    {
        var successful = await _clipboardService.RemoveContentFromClipboard(recordId);

        if (!successful)
        {
            return NotFound();
        }
        return Ok();
    }
}