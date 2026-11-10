using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;
using CorporatePortal.Infrastructure;
using CorporatePortal.Models;

namespace CorporatePortal.Controllers
{
    [SessionAuthorize("Employer")]
    public class EmployerController : Controller
    {
        public ActionResult Dashboard_Pageload() { return View(); }

        [HttpGet]
        public ActionResult PostJob_Pageload() { return View(new JobFormViewModel()); }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult PostJob_Pageload(JobFormViewModel model)
        {
            ValidateJobMode(model);
            if (!ModelState.IsValid) return View(model);
            try
            {
                using (var db = new CorporatePortalEntities())
                using (var transaction = db.Database.BeginTransaction())
                {
                    int? createdJobId = db.sp_ManageJobs("Insert", (int)Session["EmployerId"], null, model.JobTitle,
                        model.JobType, model.ExperienceRequired, model.Location, model.SalaryRange,
                        model.RequiredSkills, model.JobDescription).FirstOrDefault();
                    if (!createdJobId.HasValue || createdJobId.Value <= 0)
                        throw new InvalidOperationException("The job procedure did not return a job ID.");
                    transaction.Commit();
                }
                TempData["Success"] = "Job posted.";
                return RedirectToAction("ManageJobs_Pageload");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "The job could not be posted. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public ActionResult ManageJobs_Pageload()
        {
            using (var db = new CorporatePortalEntities())
                return View(db.sp_GetEmployerJobs((int)Session["EmployerId"]).ToList());
        }

        [HttpGet]
        public ActionResult Applications_Pageload()
        {
            int employerId = (int)Session["EmployerId"];
            using (var db = new CorporatePortalEntities())
            {
                var applications = db.Database.SqlQuery<EmployerApplicationViewModel>(
                    "EXEC dbo.sp_GetEmployerApplications @EmployerId",
                    new SqlParameter("@EmployerId", SqlDbType.Int) { Value = employerId }).ToList();

                var employerJobIds = new HashSet<int>(db.Jobs
                    .Where(job => job.EmployerId == employerId)
                    .Select(job => job.JobId)
                    .ToList());
                applications = applications.Where(application => employerJobIds.Contains(application.JobId)).ToList();
                return View(applications);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult UpdateApplicationStatus(int applicationId, string status)
        {
            if (applicationId <= 0)
            {
                TempData["Error"] = "The application could not be found.";
                return RedirectToAction("Applications_Pageload");
            }

            var allowedStatuses = new[] { "Pending", "Shortlisted", "Interview", "Rejected", "Hired" };
            var selectedStatus = allowedStatuses.FirstOrDefault(value =>
                string.Equals(value, status, StringComparison.OrdinalIgnoreCase));
            if (selectedStatus == null)
            {
                TempData["Error"] = "Select a valid application status.";
                return RedirectToAction("Applications_Pageload");
            }

            int employerId = (int)Session["EmployerId"];
            try
            {
                using (var db = new CorporatePortalEntities())
                {
                    int affected = db.Database.ExecuteSqlCommand(
                        "UPDATE A SET Status = @Status FROM dbo.JobApplication AS A WHERE A.ApplicationId = @ApplicationId AND EXISTS (SELECT 1 FROM dbo.Jobs AS J WHERE J.JobId = A.JobId AND J.EmployerId = @EmployerId)",
                        new SqlParameter("@Status", SqlDbType.VarChar, 20) { Value = selectedStatus },
                        new SqlParameter("@ApplicationId", SqlDbType.Int) { Value = applicationId },
                        new SqlParameter("@EmployerId", SqlDbType.Int) { Value = employerId });
                    TempData[affected == 1 ? "Success" : "Error"] = affected == 1
                        ? "Application status updated."
                        : "The application was not found for one of your jobs.";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "The application status could not be updated. Please try again.";
            }

            return RedirectToAction("Applications_Pageload");
        }

        [HttpGet]
        public ActionResult EditJob_Pageload(int id)
        {
            int employerId = (int)Session["EmployerId"];
            using (var db = new CorporatePortalEntities())
            {
                var job = db.Jobs.SingleOrDefault(x => x.JobId == id && x.EmployerId == employerId);
                if (job == null) return HttpNotFound();
                ViewBag.LegacyJobMode = !IsAllowedJobMode(job.JobType) ? job.JobType : null;
                return View(new JobFormViewModel { JobId = job.JobId, JobTitle = job.JobTitle,
                    JobType = job.JobType, ExperienceRequired = job.ExperienceRequired, Location = job.Location,
                    SalaryRange = job.SalaryRange, RequiredSkills = job.RequiredSkills, JobDescription = job.JobDescription });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditJob_Pageload(JobFormViewModel model)
        {
            int employerId = (int)Session["EmployerId"];
            if (!model.JobId.HasValue) return HttpNotFound();
            ValidateJobMode(model);
            if (!ModelState.IsValid) return View(model);
            try
            {
                using (var db = new CorporatePortalEntities())
                using (var transaction = db.Database.BeginTransaction())
                {
                    if (!db.Jobs.Any(x => x.JobId == model.JobId.Value && x.EmployerId == employerId)) return HttpNotFound();
                    int? updatedJobId = db.sp_ManageJobs("Update", employerId, model.JobId, model.JobTitle, model.JobType,
                        model.ExperienceRequired, model.Location, model.SalaryRange, model.RequiredSkills,
                        model.JobDescription).FirstOrDefault();
                    if (updatedJobId != model.JobId)
                        throw new InvalidOperationException("The job procedure returned an unexpected job ID.");
                    transaction.Commit();
                }
                TempData["Success"] = "Job updated.";
                return RedirectToAction("ManageJobs_Pageload");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "The job could not be updated. Please try again.");
                return View(model);
            }
        }

        private void ValidateJobMode(JobFormViewModel model)
        {
            var modes = new[] { "Hybrid", "On-site", "Remote" };
            var normalized = modes.FirstOrDefault(mode => string.Equals(mode, model.JobType, StringComparison.OrdinalIgnoreCase));
            if (normalized == null)
                ModelState.AddModelError("JobType", "Select Hybrid, On-site, or Remote.");
            else
                model.JobType = normalized;
        }

        private static bool IsAllowedJobMode(string value)
        {
            return new[] { "Hybrid", "On-site", "Remote" }
                .Any(mode => string.Equals(mode, value, StringComparison.OrdinalIgnoreCase));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DeleteJob(int id)
        {
            int employerId = (int)Session["EmployerId"];
            try
            {
                using (var db = new CorporatePortalEntities())
                using (var transaction = db.Database.BeginTransaction())
                {
                    if (!db.Jobs.Any(x => x.JobId == id && x.EmployerId == employerId)) return HttpNotFound();
                    db.Database.ExecuteSqlCommand(
                        "DELETE FROM dbo.JobApplication WHERE JobId = @JobId",
                        new SqlParameter("@JobId", SqlDbType.Int) { Value = id });
                    int? deletedJobId = db.sp_ManageJobs("Delete", employerId, id, null, null, null, null, null, null, null).FirstOrDefault();
                    if (deletedJobId != id)
                        throw new InvalidOperationException("The job procedure returned an unexpected job ID.");
                    transaction.Commit();
                }
                TempData["Success"] = "Job deleted.";
            }
            catch (Exception) { TempData["Error"] = "The job could not be deleted."; }
            return RedirectToAction("ManageJobs_Pageload");
        }

        [HttpGet]
        public ActionResult Profile_Pageload()
        {
            int employerId = (int)Session["EmployerId"];
            using (var db = new CorporatePortalEntities())
            {
                var employer = db.Employers.SingleOrDefault(x => x.EmployerId == employerId);
                if (employer == null) return HttpNotFound();
                var company = db.CompanyDetails.SingleOrDefault(x => x.EmployerId == employerId);
                return View(new EmployerProfileViewModel { Name = employer.Name, Email = employer.Email,
                    Phone = employer.Phone, CompanyName = company == null ? null : company.CompanyName,
                    Website = company == null ? null : company.Website, Address = company == null ? null : company.Address,
                    Description = company == null ? null : company.Description });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(EmployerProfileViewModel model)
        {
            if (!ModelState.IsValid) return View("Profile_Pageload", model);
            try
            {
                using (var db = new CorporatePortalEntities())
                    db.sp_ManageCompanyDetails((int)Session["EmployerId"], model.CompanyName,
                        model.Website, model.Address, model.Description).FirstOrDefault();
                TempData["Success"] = "Company profile updated.";
                return RedirectToAction("Profile_Pageload");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Company profile could not be updated. Please try again.");
                return View("Profile_Pageload", model);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear(); Session.Abandon();
            return RedirectToAction("Login_Pageload", "Login");
        }
    }
}
