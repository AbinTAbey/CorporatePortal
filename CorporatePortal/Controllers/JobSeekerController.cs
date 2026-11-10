using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using CorporatePortal.Infrastructure;
using CorporatePortal.Models;

namespace CorporatePortal.Controllers
{
    [SessionAuthorize("Jobseeker")]
    public class JobSeekerController : Controller
    {
        public ActionResult Dashboard_Pageload() { return View(); }

        [HttpGet]
        public ActionResult MyApplications_Pageload()
        {
            int jobseekerId = (int)Session["JobseekerId"];
            using (var db = new CorporatePortalEntities())
            {
                var applications = db.Database.SqlQuery<JobseekerApplicationViewModel>(
                    "SELECT A.ApplicationId, A.JobId, J.JobTitle, COALESCE(C.CompanyName, E.Name) AS CompanyName, A.AppliedDate, A.Status FROM dbo.JobApplication AS A INNER JOIN dbo.Jobs AS J ON J.JobId = A.JobId INNER JOIN dbo.Employer AS E ON E.EmployerId = J.EmployerId OUTER APPLY (SELECT TOP (1) CD.CompanyName FROM dbo.CompanyDetails AS CD WHERE CD.EmployerId = E.EmployerId ORDER BY CD.CompanyId DESC) AS C WHERE A.JobseekerId = @JobseekerId ORDER BY A.AppliedDate DESC",
                    new SqlParameter("@JobseekerId", SqlDbType.Int) { Value = jobseekerId }).ToList();
                return View(applications);
            }
        }

        [HttpGet]
        public ActionResult MyJobs_Pageload(string tab = "Applied")
        {
            int jobseekerId = (int)Session["JobseekerId"];
            string activeTab = NormalizeMyJobsTab(tab);
            using (var db = new CorporatePortalEntities())
            {
                var jobs = db.Database.SqlQuery<MyJobItemViewModel>(
                    "SELECT J.JobId, J.JobTitle, COALESCE(NULLIF(C.CompanyName, ''), E.Name) AS CompanyName, J.Location, J.JobType, J.SalaryRange, A.AppliedDate, A.Status, CAST(CASE WHEN A.ApplicationId IS NULL THEN 0 ELSE 1 END AS bit) AS IsApplied, CAST(COALESCE(P.IsSaved, 0) AS bit) AS IsSaved, CAST(COALESCE(P.IsArchived, 0) AS bit) AS IsArchived FROM dbo.Jobs AS J INNER JOIN dbo.Employer AS E ON E.EmployerId = J.EmployerId OUTER APPLY (SELECT TOP (1) CD.CompanyName FROM dbo.CompanyDetails AS CD WHERE CD.EmployerId = E.EmployerId ORDER BY CD.CompanyId DESC) AS C OUTER APPLY (SELECT TOP (1) JA.ApplicationId, JA.AppliedDate, JA.Status FROM dbo.JobApplication AS JA WHERE JA.JobId = J.JobId AND JA.JobseekerId = @JobseekerId ORDER BY JA.AppliedDate DESC, JA.ApplicationId DESC) AS A LEFT JOIN dbo.JobseekerJobPreference AS P ON P.JobId = J.JobId AND P.JobseekerId = @JobseekerId WHERE A.ApplicationId IS NOT NULL OR COALESCE(P.IsSaved, 0) = 1 OR COALESCE(P.IsArchived, 0) = 1 ORDER BY J.JobTitle",
                    new SqlParameter("@JobseekerId", SqlDbType.Int) { Value = jobseekerId }).ToList();

                return View(new MyJobsViewModel
                {
                    Jobs = jobs,
                    ActiveTab = activeTab,
                    AppliedCount = jobs.Count(job => job.IsApplied && !job.IsArchived),
                    SavedCount = jobs.Count(job => job.IsSaved && !job.IsArchived),
                    InterviewCount = jobs.Count(job => job.IsApplied && !job.IsArchived && string.Equals(job.Status, "Interview", StringComparison.OrdinalIgnoreCase)),
                    ArchivedCount = jobs.Count(job => job.IsArchived)
                });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult SaveJob(int id, string source = null, string tab = null)
        {
            int jobseekerId = (int)Session["JobseekerId"];
            if (!JobExists(id)) return HttpNotFound();
            try
            {
                using (var db = new CorporatePortalEntities())
                    SetJobPreference(db, jobseekerId, id, "IsSaved");
                TempData["JobStateMessage"] = "Job saved to your list.";
            }
            catch (Exception)
            {
                TempData["JobStateError"] = "This job could not be saved. Please try again.";
            }
            return RedirectAfterJobState(source, id, tab);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult RemoveSavedJob(int id, string source = null, string tab = null)
        {
            int jobseekerId = (int)Session["JobseekerId"];
            try
            {
                using (var db = new CorporatePortalEntities())
                {
                    db.Database.ExecuteSqlCommand(
                        "UPDATE dbo.JobseekerJobPreference SET IsSaved = 0, UpdatedAt = SYSUTCDATETIME() WHERE JobseekerId = @JobseekerId AND JobId = @JobId; DELETE FROM dbo.JobseekerJobPreference WHERE JobseekerId = @JobseekerId AND JobId = @JobId AND IsSaved = 0 AND IsArchived = 0",
                        new SqlParameter("@JobseekerId", SqlDbType.Int) { Value = jobseekerId },
                        new SqlParameter("@JobId", SqlDbType.Int) { Value = id });
                }
                TempData["JobStateMessage"] = "Job removed from Saved.";
            }
            catch (Exception)
            {
                TempData["JobStateError"] = "The saved job could not be removed. Please try again.";
            }
            return RedirectAfterJobState(source, id, tab);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ArchiveJob(int id, string source = null, string tab = null)
        {
            int jobseekerId = (int)Session["JobseekerId"];
            if (!JobExists(id)) return HttpNotFound();
            try
            {
                using (var db = new CorporatePortalEntities())
                    SetJobPreference(db, jobseekerId, id, "IsArchived");
                TempData["JobStateMessage"] = "Job archived. Your application and saved state are unchanged.";
            }
            catch (Exception)
            {
                TempData["JobStateError"] = "This job could not be archived. Please try again.";
            }
            return RedirectAfterJobState(source, id, tab);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult RestoreJob(int id, string source = null, string tab = null)
        {
            int jobseekerId = (int)Session["JobseekerId"];
            try
            {
                using (var db = new CorporatePortalEntities())
                {
                    db.Database.ExecuteSqlCommand(
                        "UPDATE dbo.JobseekerJobPreference SET IsArchived = 0, UpdatedAt = SYSUTCDATETIME() WHERE JobseekerId = @JobseekerId AND JobId = @JobId; DELETE FROM dbo.JobseekerJobPreference WHERE JobseekerId = @JobseekerId AND JobId = @JobId AND IsSaved = 0 AND IsArchived = 0",
                        new SqlParameter("@JobseekerId", SqlDbType.Int) { Value = jobseekerId },
                        new SqlParameter("@JobId", SqlDbType.Int) { Value = id });
                }
                TempData["JobStateMessage"] = "Job restored to the appropriate active list.";
            }
            catch (Exception)
            {
                TempData["JobStateError"] = "This job could not be restored. Please try again.";
            }
            return RedirectAfterJobState(source, id, tab);
        }

        [HttpGet]
        public ActionResult SearchJobs_Pageload(string jobTitle, string experienceRequired, string salaryRange, string location, string skills)
        {
            using (var db = new CorporatePortalEntities())
            {
                var jobs = db.sp_GetJobs(EmptyToNull(jobTitle), EmptyToNull(experienceRequired),
                    EmptyToNull(salaryRange), EmptyToNull(location), EmptyToNull(skills)).ToList();
                PopulateJobState(db, (int)Session["JobseekerId"]);
                return View(jobs);
            }
        }

        [HttpGet]
        public ActionResult JobDetails_Pageload(int id)
        {
            if (id <= 0) return HttpNotFound();
            using (var db = new CorporatePortalEntities())
            {
                var job = db.sp_GetJobDetails(id).FirstOrDefault();
                if (job == null) return HttpNotFound();
                int jobseekerId = (int)Session["JobseekerId"];
                ViewBag.HasApplied = HasApplied(db, id, jobseekerId);
                ViewBag.IsSaved = db.Database.SqlQuery<int>(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.JobseekerJobPreference WHERE JobseekerId = @JobseekerId AND JobId = @JobId AND IsSaved = 1) THEN 1 ELSE 0 END",
                    new SqlParameter("@JobseekerId", SqlDbType.Int) { Value = jobseekerId },
                    new SqlParameter("@JobId", SqlDbType.Int) { Value = id }).Single() == 1;
                ViewBag.IsArchived = db.Database.SqlQuery<int>(
                    "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.JobseekerJobPreference WHERE JobseekerId = @JobseekerId AND JobId = @JobId AND IsArchived = 1) THEN 1 ELSE 0 END",
                    new SqlParameter("@JobseekerId", SqlDbType.Int) { Value = jobseekerId },
                    new SqlParameter("@JobId", SqlDbType.Int) { Value = id }).Single() == 1;
                return View(job);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult ApplyForJob(int id, string coverLetter)
        {
            if (id <= 0) return HttpNotFound();
            if (coverLetter != null && coverLetter.Length > 2000)
            {
                TempData["ApplicationError"] = "The cover letter must be 2,000 characters or fewer.";
                return RedirectToAction("JobDetails_Pageload", new { id });
            }

            object sessionJobseekerId = Session["JobseekerId"];
            if (sessionJobseekerId == null) return new HttpStatusCodeResult(403);
            int jobseekerId = (int)sessionJobseekerId;

            try
            {
                using (var db = new CorporatePortalEntities())
                {
                    if (!db.Jobs.Any(job => job.JobId == id)) return HttpNotFound();
                    if (HasApplied(db, id, jobseekerId))
                    {
                        TempData["ApplicationError"] = "You have already applied for this job.";
                        return RedirectToAction("JobDetails_Pageload", new { id });
                    }

                    db.Database.ExecuteSqlCommand(
                        "EXEC dbo.sp_ApplyForJob @JobId = @JobId, @JobseekerId = @JobseekerId, @CoverLetter = @CoverLetter",
                        new SqlParameter("@JobId", SqlDbType.Int) { Value = id },
                        new SqlParameter("@JobseekerId", SqlDbType.Int) { Value = jobseekerId },
                        new SqlParameter("@CoverLetter", SqlDbType.VarChar, 2000)
                        {
                            Value = string.IsNullOrWhiteSpace(coverLetter) ? (object)DBNull.Value : coverLetter.Trim()
                        });

                    if (!HasApplied(db, id, jobseekerId))
                    {
                        TempData["ApplicationError"] = "Your application could not be submitted. Please try again.";
                        return RedirectToAction("JobDetails_Pageload", new { id });
                    }
                }

                TempData["ApplicationSuccess"] = "Your application was submitted successfully.";
            }
            catch (Exception exception)
            {
                TempData["ApplicationError"] = IsDuplicateApplication(exception)
                    ? "You have already applied for this job."
                    : "Your application could not be submitted. Please try again.";
            }

            return RedirectToAction("JobDetails_Pageload", new { id });
        }

        [HttpGet]
        public ActionResult Profile_Pageload()
        {
            int jobseekerId = (int)Session["JobseekerId"];
            using (var db = new CorporatePortalEntities())
            {
                var person = db.Jobseekers.SingleOrDefault(x => x.JobseekerId == jobseekerId);
                if (person == null) return HttpNotFound();
                var profile = db.JobseekerProfiles.SingleOrDefault(x => x.JobseekerId == jobseekerId);
                return View(new JobseekerProfileViewModel
                {
                    Name = person.Name, Email = person.Email, Phone = person.Phone,
                    ResumePath = profile == null ? null : profile.ResumePath,
                    Qualification = profile == null ? null : profile.Qualification,
                    Skills = profile == null ? null : profile.Skills,
                    Experience = profile == null ? null : profile.Experience,
                    Address = profile == null ? null : profile.Address
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateProfile(JobseekerProfileViewModel model)
        {
            int jobseekerId = (int)Session["JobseekerId"];
            string resumePath = null;
            if (model.ResumeFile != null && model.ResumeFile.ContentLength > 0)
            {
                string extension = Path.GetExtension(model.ResumeFile.FileName);
                string[] allowed = { ".pdf", ".doc", ".docx" };
                if (string.IsNullOrEmpty(extension) || !allowed.Contains(extension.ToLowerInvariant()))
                    ModelState.AddModelError("ResumeFile", "Upload a PDF, DOC, or DOCX file.");
                if (model.ResumeFile.ContentLength > 5 * 1024 * 1024)
                    ModelState.AddModelError("ResumeFile", "Resume size must not exceed 5 MB.");
            }
            if (!ModelState.IsValid) return View("Profile_Pageload", model);

            string newlyCreatedResumePath = null;
            bool safeToDeleteNewResume = true;
            try
            {
                using (var db = new CorporatePortalEntities())
                {
                    var profile = db.JobseekerProfiles.SingleOrDefault(x => x.JobseekerId == jobseekerId);
                    resumePath = profile == null ? null : profile.ResumePath;
                    if (model.ResumeFile != null && model.ResumeFile.ContentLength > 0)
                    {
                        string extension = Path.GetExtension(model.ResumeFile.FileName).ToLowerInvariant();
                        string folder = Server.MapPath("~/UploadedResumes");
                        Directory.CreateDirectory(folder);
                        string fileName = Guid.NewGuid().ToString("N") + extension;
                        string physicalPath = Path.Combine(folder, fileName);
                        using (var output = new FileStream(
                            physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                        {
                            // This unique path belongs to this request only after
                            // CreateNew has succeeded, so cleanup cannot remove an
                            // earlier resume or a colliding file.
                            newlyCreatedResumePath = physicalPath;
                            model.ResumeFile.InputStream.CopyTo(output);
                        }
                        resumePath = "~/UploadedResumes/" + fileName;
                    }

                    System.Data.Entity.DbContextTransaction transaction = null;
                    bool transactionStarted = false;
                    bool rollbackConfirmed = false;
                    bool committed = false;
                    try
                    {
                        transaction = db.Database.BeginTransaction();
                        transactionStarted = true;

                        string profileResult = db.sp_ManageJobseekerProfile(
                            jobseekerId, resumePath, model.Qualification,
                            model.Skills, model.Experience, model.Address).FirstOrDefault();
                        if (!string.Equals(
                            profileResult,
                            "Profile saved successfully",
                            StringComparison.OrdinalIgnoreCase))
                        {
                            throw new InvalidOperationException(
                                "The profile procedure did not confirm the update.");
                        }

                        transaction.Commit();
                        committed = true;
                        newlyCreatedResumePath = null;
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
                                // Keep a new resume if commit/rollback status is
                                // uncertain, so the database cannot point to a
                                // file removed by this cleanup path.
                            }
                        }

                        if (newlyCreatedResumePath != null &&
                            (!transactionStarted || rollbackConfirmed))
                        {
                            TryDeleteCreatedResume(newlyCreatedResumePath);
                        }
                        else if (transactionStarted && !rollbackConfirmed)
                        {
                            safeToDeleteNewResume = false;
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
                TempData["Success"] = "Profile updated.";
                return RedirectToAction("Profile_Pageload");
            }
            catch (Exception)
            {
                if (newlyCreatedResumePath != null && safeToDeleteNewResume)
                {
                    TryDeleteCreatedResume(newlyCreatedResumePath);
                }

                ModelState.AddModelError("", "Profile could not be updated. Please try again.");
                return View("Profile_Pageload", model);
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
                // Cleanup is best effort; preserve the original database error.
            }
            catch (UnauthorizedAccessException)
            {
                // Cleanup is best effort; preserve the original database error.
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear(); Session.Abandon();
            return RedirectToAction("Login_Pageload", "Login");
        }

        private static string EmptyToNull(string value) { return string.IsNullOrWhiteSpace(value) ? null : value.Trim(); }

        private static string NormalizeMyJobsTab(string tab)
        {
            string[] tabs = { "Applied", "Saved", "Interview", "Archived" };
            return tabs.FirstOrDefault(value => string.Equals(value, tab, StringComparison.OrdinalIgnoreCase)) ?? "Applied";
        }

        private bool JobExists(int jobId)
        {
            if (jobId <= 0) return false;
            using (var db = new CorporatePortalEntities())
                return db.Jobs.Any(job => job.JobId == jobId);
        }

        private void SetJobPreference(CorporatePortalEntities db, int jobseekerId, int jobId, string stateColumn)
        {
            if (stateColumn != "IsSaved" && stateColumn != "IsArchived")
                throw new ArgumentOutOfRangeException("stateColumn");

            db.Database.ExecuteSqlCommand(
                "SET XACT_ABORT ON; BEGIN TRANSACTION; IF EXISTS (SELECT 1 FROM dbo.JobseekerJobPreference WITH (UPDLOCK, HOLDLOCK) WHERE JobseekerId = @JobseekerId AND JobId = @JobId) BEGIN UPDATE dbo.JobseekerJobPreference SET " + stateColumn + " = 1, UpdatedAt = SYSUTCDATETIME() WHERE JobseekerId = @JobseekerId AND JobId = @JobId; END ELSE BEGIN INSERT INTO dbo.JobseekerJobPreference (JobseekerId, JobId, " + stateColumn + ") VALUES (@JobseekerId, @JobId, 1); END; COMMIT TRANSACTION;",
                new SqlParameter("@JobseekerId", SqlDbType.Int) { Value = jobseekerId },
                new SqlParameter("@JobId", SqlDbType.Int) { Value = jobId });
        }

        private void PopulateJobState(CorporatePortalEntities db, int jobseekerId)
        {
            ViewBag.SavedJobIds = new HashSet<int>(db.Database.SqlQuery<int>(
                "SELECT JobId FROM dbo.JobseekerJobPreference WHERE JobseekerId = @JobseekerId AND IsSaved = 1",
                new SqlParameter("@JobseekerId", SqlDbType.Int) { Value = jobseekerId }).ToList());
        }

        private ActionResult RedirectAfterJobState(string source, int jobId, string tab)
        {
            if (string.Equals(source, "MyJobs", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("MyJobs_Pageload", new { tab = NormalizeMyJobsTab(tab) });
            if (string.Equals(source, "Details", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("JobDetails_Pageload", new { id = jobId });
            return RedirectToAction("SearchJobs_Pageload");
        }

        private static bool HasApplied(CorporatePortalEntities db, int jobId, int jobseekerId)
        {
            return db.Database.SqlQuery<int>(
                "SELECT CASE WHEN EXISTS (SELECT 1 FROM dbo.JobApplication WHERE JobId = @JobId AND JobseekerId = @JobseekerId) THEN 1 ELSE 0 END",
                new SqlParameter("@JobId", SqlDbType.Int) { Value = jobId },
                new SqlParameter("@JobseekerId", SqlDbType.Int) { Value = jobseekerId })
                .Single() == 1;
        }

        private static bool IsDuplicateApplication(Exception exception)
        {
            for (var current = exception; current != null; current = current.InnerException)
            {
                var sqlException = current as SqlException;
                if (sqlException != null && sqlException.Errors.Cast<SqlError>().Any(error => error.Number == 2601 || error.Number == 2627))
                    return true;

                if (current.Message.IndexOf("already applied", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    current.Message.IndexOf("duplicate", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }

            return false;
        }
    }
}
