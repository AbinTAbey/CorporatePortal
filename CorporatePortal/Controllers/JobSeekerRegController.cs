using System;
using System.IO;
using System.Linq;
using System.Web.Helpers;
using System.Web.Mvc;
using CorporatePortal.Models;

namespace CorporatePortal.Controllers
{
    public class JobSeekerRegController : Controller
    {
        // Registration page
        [HttpGet]
        public ActionResult Insert_Pageload()
        {
            return View(new RegisterViewModel());
        }

        // Handle registration form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Insert_Pageload(RegisterViewModel model)
        {
            // Role is assigned on the server, not trusted from the form
            ModelState.Remove("Role");
            model.Role = "Jobseeker";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string extension = null;

            // Validate optional resume upload
            if (model.ResumeFile != null &&
                model.ResumeFile.ContentLength > 0)
            {
                extension = Path.GetExtension(
                    model.ResumeFile.FileName);

                string[] allowedExtensions =
                    { ".pdf", ".doc", ".docx" };

                if (string.IsNullOrEmpty(extension) ||
                    !allowedExtensions.Contains(
                        extension.ToLowerInvariant()))
                {
                    ModelState.AddModelError(
                        "ResumeFile",
                        "Upload a PDF, DOC, or DOCX file.");

                    return View(model);
                }

                if (model.ResumeFile.ContentLength > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError(
                        "ResumeFile",
                        "Resume size must not exceed 5 MB.");

                    return View(model);
                }
            }

            try
            {
                string passwordHash = model.Password;

                using (var dbobj = new CorporatePortalEntities())
                {
                    // Register Job Seeker and Login records
                    dbobj.sp_RegisterUser(
                        model.Name.Trim(),
                        model.Email.Trim(),
                        model.Phone.Trim(),
                        model.Username.Trim(),
                        passwordHash,
                        "Jobseeker"
                    ).FirstOrDefault();

                    // Save resume and profile path if a file was uploaded
                    if (model.ResumeFile != null &&
                        model.ResumeFile.ContentLength > 0)
                    {
                        var login = dbobj.Logins.SingleOrDefault(
                            x => x.Username == model.Username.Trim());

                        if (login != null &&
                            login.JobseekerId.HasValue)
                        {
                            string folder =
                                Server.MapPath("~/UploadedResumes");

                            Directory.CreateDirectory(folder);

                            string fileName =
                                Guid.NewGuid().ToString("N") +
                                extension.ToLowerInvariant();

                            string physicalPath =
                                Path.Combine(folder, fileName);

                            model.ResumeFile.SaveAs(physicalPath);

                            string resumePath =
                                "~/UploadedResumes/" + fileName;

                            dbobj.sp_ManageJobseekerProfile(
                                login.JobseekerId.Value,
                                resumePath,
                                null,
                                null,
                                null,
                                null
                            ).FirstOrDefault();
                        }
                    }
                }

                TempData["Success"] =
                    "Registration successful! You can now log in.";

                return RedirectToAction("Insert_Pageload");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    "",
                    "Registration could not be completed. " +
                    "Check whether the username or email is already " +
                    "registered, and try again.");

                return View(model);
            }
        }
    }
}