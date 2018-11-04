using System.Web.Mvc;

namespace Maha.Microservices.Host.Controllers
{
    public class HomeController : Controller
    {
        //[OutputCache(Duration = 60 * 60 * 4/*4 hours*/, VaryByParam = "none")]
        public ActionResult Index()
        {
            return View();
        }

        [OutputCache(Duration = 60 * 60 * 4/*4 hours*/, VaryByParam = "name")]
        public ActionResult Method()
        {
            return View();
        }

        [OutputCache(Duration = 60 * 60 * 4/*4 hours*/, VaryByParam = "name")]
        public ActionResult Type()
        {
            return View();
        }
    }
}