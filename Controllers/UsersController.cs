using Microsoft.AspNetCore.Mvc;

namespace ControleEstoque.Api.Controllers
{
    public class UsersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
