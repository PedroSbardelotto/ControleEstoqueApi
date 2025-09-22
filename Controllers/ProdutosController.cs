using Microsoft.AspNetCore.Mvc;

namespace ControleEstoque.Api.Controllers
{
    public class ProdutosController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
