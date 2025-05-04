using Microsoft.AspNetCore.Mvc;

namespace TaskMonitor.Controllers;

[ApiController]
[Route("/api/v1alpha")]
public class ApiController : ControllerBase
{
    [HttpGet]
    public string Hello()
    {
        return "Hello!";
    }
}
