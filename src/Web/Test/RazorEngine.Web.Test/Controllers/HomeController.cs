using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RazorEngine.Web.Test.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            string template = "Hello @Model.Name";
            string result = Razor.Parse(template, new { Name = "Matt" });

            string template2 = "@model RazorEngine.Web.Test.Controllers.Person\n\r@Model.Name (@Model.Age)";
            string result2 = Razor.Parse(template2, new Person { Name = "Matt", Age = 29 });

            ViewBag.Message = result2;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }

    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}
