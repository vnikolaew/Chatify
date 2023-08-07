using Microsoft.AspNetCore.Mvc;

namespace Chatify.Web.Features;

public class IndexController : Controller
{
    [HttpGet]
    [Route("index")]
    public IActionResult Index() => View();
}