using System;
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using CorporatePortal.Models;

namespace CorporatePortal.Controllers
{
    public class LoginController : Controller
    {
        [HttpGet]
        public ActionResult Login_Pageload() { return View(new LoginViewModel()); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login_Pageload(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            sp_ValidateLogin_Result account;
            try
            {
                using (var db = new CorporatePortalEntities())
                    account = db.sp_ValidateLogin(model.Username.Trim()).FirstOrDefault();
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Login could not be completed. Please try again.");
                return View(model);
            }

            if (account == null || !PasswordMatches(account.PasswordHash, model.Password) ||
                !ValidAccountReferences(account))
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            Session.Clear();
            Session["LoginId"] = account.LoginId;
            Session["Role"] = account.Role;
            if (account.JobseekerId.HasValue) Session["JobseekerId"] = account.JobseekerId.Value;
            if (account.EmployerId.HasValue) Session["EmployerId"] = account.EmployerId.Value;

            switch (account.Role)
            {
                case "Jobseeker": return RedirectToAction("Dashboard_Pageload", "JobSeeker");
                case "Employer": return RedirectToAction("Dashboard_Pageload", "Employer");
                case "Admin": return RedirectToAction("Dashboard_Pageload", "Admin");
                default:
                    Session.Clear();
                    ModelState.AddModelError("", "Invalid username or password.");
                    return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login_Pageload");
        }

        private static bool PasswordMatches(string storedPassword, string enteredPassword)
        {
            if (storedPassword == null || enteredPassword == null) return false;
            if (IsCryptoHash(storedPassword))
            {
                try { return Crypto.VerifyHashedPassword(storedPassword, enteredPassword); }
                catch (Exception) { return false; }
            }

            // Prototype registrations store plaintext in this column. Compare exactly;
            // never pass a plaintext value to the hash verifier.
            return string.Equals(storedPassword, enteredPassword, StringComparison.Ordinal);
        }

        private static bool IsCryptoHash(string storedPassword)
        {
            try
            {
                byte[] payload = Convert.FromBase64String(storedPassword);
                // System.Web.Helpers.Crypto.HashPassword uses a version marker followed
                // by a 16-byte salt and a 32-byte derived subkey.
                return payload.Length == 49 && payload[0] == 0;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static bool ValidAccountReferences(sp_ValidateLogin_Result account)
        {
            if (account.Role == "Jobseeker") return account.JobseekerId.HasValue && !account.EmployerId.HasValue;
            if (account.Role == "Employer") return account.EmployerId.HasValue && !account.JobseekerId.HasValue;
            if (account.Role == "Admin") return !account.EmployerId.HasValue && !account.JobseekerId.HasValue;
            return false;
        }
    }
}
