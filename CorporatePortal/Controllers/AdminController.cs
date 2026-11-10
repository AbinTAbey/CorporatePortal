using System.Linq;
using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using CorporatePortal.Infrastructure;
using CorporatePortal.Models;

namespace CorporatePortal.Controllers
{
    [SessionAuthorize("Admin")]
    public class AdminController : Controller
    {
        public ActionResult Dashboard_Pageload() { return View(); }

        public ActionResult ManageJobseekers_Pageload()
        {
            using (var db = new CorporatePortalEntities()) return View(db.Jobseekers.OrderBy(x => x.Name).ToList());
        }

        public ActionResult ManageEmployers_Pageload()
        {
            using (var db = new CorporatePortalEntities()) return View(db.Employers.OrderBy(x => x.Name).ToList());
        }

        public ActionResult ManageJobs_Pageload()
        {
            using (var db = new CorporatePortalEntities()) return View(db.Jobs.OrderBy(x => x.JobTitle).ToList());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DeleteJob(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "The job could not be deleted. The ID is invalid.";
                return RedirectToAction("ManageJobs_Pageload");
            }

            try
            {
                using (var db = new CorporatePortalEntities())
                using (var transaction = db.Database.BeginTransaction())
                {
                    var job = db.Jobs.SingleOrDefault(item => item.JobId == id);
                    if (job == null)
                    {
                        TempData["Error"] = "The job was not found.";
                        return RedirectToAction("ManageJobs_Pageload");
                    }

                    db.Database.ExecuteSqlCommand(
                        "DELETE FROM dbo.JobApplication WHERE JobId = @JobId",
                        new SqlParameter("@JobId", SqlDbType.Int) { Value = id });
                    db.Jobs.Remove(job);
                    db.SaveChanges();
                    transaction.Commit();
                }

                TempData["Success"] = "The job and its applications were deleted.";
            }
            catch (Exception)
            {
                TempData["Error"] = "The job could not be deleted because a database operation failed.";
            }

            return RedirectToAction("ManageJobs_Pageload");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteJobseeker(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "The Job Seeker could not be deleted. The ID is invalid.";
                return RedirectToAction("ManageJobseekers_Pageload");
            }

            var resumePaths = new System.Collections.Generic.List<string>();
            try
            {
                using (var db = new CorporatePortalEntities())
                using (var transaction = db.Database.BeginTransaction())
                {
                    var jobseeker = db.Jobseekers.SingleOrDefault(x => x.JobseekerId == id);
                    if (jobseeker == null)
                    {
                        TempData["Error"] = "The Job Seeker was not found.";
                        return RedirectToAction("ManageJobseekers_Pageload");
                    }

                    db.Database.ExecuteSqlCommand(
                        "DELETE FROM dbo.JobApplication WHERE JobseekerId = @JobseekerId",
                        new SqlParameter("@JobseekerId", SqlDbType.Int) { Value = id });
                    var profiles = db.JobseekerProfiles.Where(x => x.JobseekerId == id).ToList();
                    var logins = db.Logins.Where(x => x.JobseekerId == id).ToList();
                    resumePaths.AddRange(profiles.Select(x => x.ResumePath).Where(x => !string.IsNullOrWhiteSpace(x)));

                    db.JobseekerProfiles.RemoveRange(profiles);
                    db.Logins.RemoveRange(logins);
                    db.Jobseekers.Remove(jobseeker);
                    db.SaveChanges();
                    transaction.Commit();
                }

                bool resumeCleanupSucceeded = DeleteUploadedResumes(resumePaths);
                TempData["Success"] = resumeCleanupSucceeded
                    ? "The Job Seeker and associated records were deleted."
                    : "The Job Seeker and associated records were deleted, but a resume file could not be removed.";
            }
            catch (Exception)
            {
                TempData["Error"] = "The Job Seeker could not be deleted because the database operation failed or related records prevent deletion.";
            }

            return RedirectToAction("ManageJobseekers_Pageload");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteEmployer(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "The Employer could not be deleted. The ID is invalid.";
                return RedirectToAction("ManageEmployers_Pageload");
            }

            try
            {
                using (var db = new CorporatePortalEntities())
                using (var transaction = db.Database.BeginTransaction())
                {
                    var employer = db.Employers.SingleOrDefault(x => x.EmployerId == id);
                    if (employer == null)
                    {
                        TempData["Error"] = "The Employer was not found.";
                        return RedirectToAction("ManageEmployers_Pageload");
                    }

                    var logins = db.Logins.Where(x => x.EmployerId == id).ToList();
                    var companyDetails = db.CompanyDetails.Where(x => x.EmployerId == id).ToList();
                    var jobs = db.Jobs.Where(x => x.EmployerId == id).ToList();

                    db.Database.ExecuteSqlCommand(
                        "DELETE FROM dbo.JobApplication WHERE JobId IN (SELECT JobId FROM dbo.Jobs WHERE EmployerId = @EmployerId)",
                        new SqlParameter("@EmployerId", SqlDbType.Int) { Value = id });
                    db.Logins.RemoveRange(logins);
                    db.CompanyDetails.RemoveRange(companyDetails);
                    db.Jobs.RemoveRange(jobs);
                    db.Employers.Remove(employer);
                    db.SaveChanges();
                    transaction.Commit();
                }

                TempData["Success"] = "The Employer and associated company details, jobs, and login records were deleted.";
            }
            catch (Exception)
            {
                TempData["Error"] = "The Employer could not be deleted because the database operation failed or related records prevent deletion.";
            }

            return RedirectToAction("ManageEmployers_Pageload");
        }

        private bool DeleteUploadedResumes(System.Collections.Generic.IEnumerable<string> resumePaths)
        {
            bool success = true;
            string uploadRoot;
            try
            {
                uploadRoot = Path.GetFullPath(Server.MapPath("~/UploadedResumes"));
            }
            catch (Exception)
            {
                return !resumePaths.Any();
            }

            string rootWithSeparator = uploadRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            foreach (string resumePath in resumePaths.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    string normalizedVirtualPath = resumePath.Replace('\\', '/');
                    if (!normalizedVirtualPath.StartsWith("~/UploadedResumes/", StringComparison.OrdinalIgnoreCase))
                    {
                        success = false;
                        continue;
                    }

                    string physicalPath = Path.GetFullPath(Server.MapPath(normalizedVirtualPath));
                    if (!physicalPath.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
                    {
                        success = false;
                        continue;
                    }

                    if (System.IO.File.Exists(physicalPath)) System.IO.File.Delete(physicalPath);
                }
                catch (Exception)
                {
                    success = false;
                }
            }

            return success;
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear(); Session.Abandon();
            return RedirectToAction("Login_Pageload", "Login");
        }
    }
}
