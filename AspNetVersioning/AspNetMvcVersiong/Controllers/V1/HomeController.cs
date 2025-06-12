using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace AspNetMvcVersiong.Controllers.V1;

[ApiVersion(1.0)]
[ApiController]
[Route("api/v{apiVersion:apiVersion}/[controller]/[action]")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public Task<IActionResult> Index()
    {
        return Task.FromResult<IActionResult>(Ok("Hello from V1 HomeController!"));
    }

    // aciton中可标注与Controller的不同版本
    [MapToApiVersion(2.0)]
    [HttpGet]
    public Task<IActionResult> Index2()
    {
        return Task.FromResult<IActionResult>(Ok("Hello from V2 HomeController!"));
    }
}
