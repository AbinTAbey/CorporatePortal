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

            bool accountCreated = false;
            bool resumeProfileFailed = false;

            try
            {
                string passwordHash = model.Password;

                using (var dbobj = new CorporatePortalEntities())
                {
                    // Register Job Seeker and Login records
                    string registrationResult = dbobj.sp_RegisterUser(
                        model.Name.Trim(),
                        model.Email.Trim(),
                        model.Phone.Trim(),
                        model.Username.Trim(),
                        passwordHash,
                        "Jobseeker"
                    ).FirstOrDefault();

                    if (!string.Equals(
                        registrationResult,
                        "Registration successful",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        throw new InvalidOperationException(
                            "The registration procedure did not confirm account creation.");
                    }

                    // The registration procedure commits the account independently
                    // of the optional profile and resume operation.
                    accountCreated = true;

                    // Save resume and profile path if a file was uploaded
                    if (model.ResumeFile != null &&
                        model.ResumeFile.ContentLength > 0)
                    {
                        try
                        {
                            var login = dbobj.Logins.SingleOrDefault(
                                x => x.Username == model.Username.Trim());

                            if (login == null || !login.JobseekerId.HasValue)
                            {
                                throw new InvalidOperationException(
                                    "The newly registered job seeker could not be retrieved.");
                            }

                            SaveResumeAndProfile(
                                dbobj,
                                login.JobseekerId.Value,
                                model.ResumeFile,
                                extension);
                        }
                        catch (Exception)
                        {
                            // Keep the committed account and let the user continue
                            // to login even if this optional step could not complete.
                            resumeProfileFailed = true;
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (accountCreated)
                {
                    resumeProfileFailed = model.ResumeFile != null &&
                        model.ResumeFile.ContentLength > 0;
                }
                else
                {
                    ModelState.AddModelError(
                        "",
                        "Registration could not be completed. " +
                        "Check whether the username or email is already " +
                        "registered, and try again.");

                    return View(model);
                }
            }

            TempData["Success"] = resumeProfileFailed
                ? "Your account was created successfully. The optional resume could not be saved; you can add it from your profile after signing in."
                : "Registration successful! You can now log in.";

            return RedirectToAction("Insert_Pageload");
        }

        private void SaveResumeAndProfile(
            CorporatePortalEntities db,
            int jobseekerId,
            System.Web.HttpPostedFileBase resumeFile,
            string extension)
        {
            string physicalPath = null;
            System.Data.Entity.DbContextTransaction transaction = null;
            bool transactionStarted = false;
            bool rollbackConfirmed = false;
            bool committed = false;

            try
            {
                string folder = Server.MapPath("~/UploadedResumes");
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString("N") +
                    extension.ToLowerInvariant();
                string targetPath = Path.Combine(folder, fileName);

                // CreateNew prevents overwriting any existing file. Track the path
                // only after this request has successfully created the file.
                using (var output = new FileStream(
                    targetPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                    physicalPath = targetPath;
                    resumeFile.InputStream.CopyTo(output);
                }

                string resumePath = "~/UploadedResumes/" + fileName;
                transaction = db.Database.BeginTransaction();
                transactionStarted = true;

                string profileResult = db.sp_ManageJobseekerProfile(
                    jobseekerId, resumePath, null, null, null, null).FirstOrDefault();
                if (!string.Equals(
                    profileResult,
                    "Profile saved successfully",
                    StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "The profile procedure did not confirm the resume update.");
                }

                transaction.Commit();
                committed = true;
                physicalPath = null;
            }
            catch
            {
                if (transactionStarted && !committed)
                {
                    try
                    {
                        transaction.Rollback();
                        rollbackConfirmed = true;
                    }
                    catch (Exception)
                    {
                        // Keep the file if rollback status is uncertain; the database
                        // may already reference it.
                    }
                }

                if (physicalPath != null &&
                    (!transactionStarted || rollbackConfirmed))
                {
                    TryDeleteCreatedResume(physicalPath);
                }

                throw;
            }
            finally
            {
                if (transaction != null)
                {
                    transaction.Dispose();
                }
            }
        }

        private static void TryDeleteCreatedResume(string physicalPath)
        {
            try
            {
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }
            catch (IOException)
            {
                // Cleanup is best effort; never replace the original operation error.
            }
            catch (UnauthorizedAccessException)
            {
                // Cleanup is best effort; never replace the original operation error.
            }
        }
    }
}
