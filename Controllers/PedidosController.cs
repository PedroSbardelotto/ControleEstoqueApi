using Microsoft.AspNetCore.Mvc;

namespace ControleEstoque.Api.Controllers
{
    public class PedidosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
