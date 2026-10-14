using System;
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
        public ActionResult SearchJobs_Pageload(string jobTitle, string experienceRequired, string salaryRange, string location)
        {
            using (var db = new CorporatePortalEntities())
            {
                var jobs = db.sp_GetJobs(EmptyToNull(jobTitle), EmptyToNull(experienceRequired),
                    EmptyToNull(salaryRange), EmptyToNull(location)).ToList();
                return View(jobs);
            }
        }

        [HttpGet]
        public ActionResult JobDetails_Pageload(int id)
        {
            using (var db = new CorporatePortalEntities())
            {
                var job = db.sp_GetJobDetails(id).FirstOrDefault();
                if (job == null) return HttpNotFound();
                return View(job);
            }
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
                        model.ResumeFile.SaveAs(Path.Combine(folder, fileName));
                        resumePath = "~/UploadedResumes/" + fileName;
                    }
                    db.sp_ManageJobseekerProfile(jobseekerId, resumePath, model.Qualification,
                        model.Skills, model.Experience, model.Address).FirstOrDefault();
                }
                TempData["Success"] = "Profile updated.";
                return RedirectToAction("Profile_Pageload");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Profile could not be updated. Please try again.");
                return View("Profile_Pageload", model);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            Session.Clear(); Session.Abandon();
            return RedirectToAction("Login_Pageload", "Login");
        }

        private static string EmptyToNull(string value) { return string.IsNullOrWhiteSpace(value) ? null : value.Trim(); }
    }
}
