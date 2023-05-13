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
    public async Task<ActionResult<Clipboard>> GetClipboard(string userId)
    {
        try
        {
            var clipboard = await _clipboardService.GetClipboardByUserId(userId);

            if (clipboard == null)
            {
                return NotFound();
            }
            return Ok(clipboard);
        }
        catch (Exception exception)
        {
            Console.Write(exception);
            return Ok();
        }
    }
    
    [HttpPost("/post")]
    public async Task<ActionResult<Record>> PostClipboardContent(PostClipboardContent postClipboardContent)
    {
        try
        {
            var userId = postClipboardContent.UserId;
            var type = postClipboardContent.Type;
            var content = postClipboardContent.Content;

            var clipboard = await _clipboardService.GetClipboardByUserId(userId) ??
                            await _clipboardService.CreateClipboard(userId);

            var record = await _clipboardService.AddContentToClipboard(userId, clipboard.Id, type, content);

            return Ok(record);
        }
        catch (Exception exception)
        {
            Console.Write(exception);
            return Ok();
        }
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