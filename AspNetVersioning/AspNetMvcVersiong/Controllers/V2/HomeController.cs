using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMvcVersiong.Controllers.V2;

[ApiVersion(2.0)]
[ApiController]
[Route("api/v{apiVersion:apiVersion}/[controller]/[action]")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> Index()
    {
        return Task.FromResult<IActionResult>(Ok("Hello from V2 HomeController!"));
    }
}

