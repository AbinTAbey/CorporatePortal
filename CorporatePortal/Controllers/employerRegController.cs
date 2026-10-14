using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CorporatePortal.Models;

namespace CorporatePortal.Controllers
{
    public class employerRegController : Controller
    {
        // GET: employerReg
      
            // Employer registration page
            [HttpGet]
            public ActionResult Insert_Pageload()
            {
                return View(new RegisterViewModel());
            }

            // Handle registration submission
            [HttpPost]
            [ValidateAntiForgeryToken]
            public ActionResult Insert_Pageload(RegisterViewModel model)
            {
                // Assign the role on the server
                ModelState.Remove("Role");
                model.Role = "Employer";

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                try
                {
                    string passwordHash = model.Password;

                    using (var dbobj = new CorporatePortalEntities())
                    {
                        dbobj.sp_RegisterUser(
                            model.Name.Trim(),
                            model.Email.Trim(),
                            model.Phone.Trim(),
                            model.Username.Trim(),
                            passwordHash,
                            "Employer"
                        ).FirstOrDefault();
                    }

                    TempData["Success"] =
                        "Employer registration successful! You can now log in.";

                    return RedirectToAction("Insert_Pageload");
                }
                catch (Exception)
                {
                    ModelState.AddModelError(
                        "",
                        "Registration failed. The username or email may " +
                        "already be registered. Please try again.");

                    return View(model);
                }
            }
    }
}