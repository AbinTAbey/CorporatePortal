using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data.SqlClient;


namespace CorporatePortal.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DbTest()
        {
            ConnectionStringSettings settings =
                ConfigurationManager.ConnectionStrings["JobFinderDB"];

            if (settings == null)
            {
                return Content(
                    "FAILED: JobFinderDB connection string not found.");
            }

            try
            {
                using (SqlConnection con =
                    new SqlConnection(settings.ConnectionString))
                {
                    con.Open();

                    return Content(
                        "SUCCESS: Connected to " + con.Database);
                }
            }
            catch (Exception)
            {
                return Content(
                    "FAILED: Could not connect to the application database.");
            }
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
