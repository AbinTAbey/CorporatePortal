using System;
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
            if (!ModelState.IsValid) return View(model);
            try
            {
                using (var db = new CorporatePortalEntities())
                    db.sp_ManageJobs("Insert", (int)Session["EmployerId"], null, model.JobTitle,
                        model.JobType, model.ExperienceRequired, model.Location, model.SalaryRange,
                        model.RequiredSkills, model.JobDescription).FirstOrDefault();
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
        public ActionResult EditJob_Pageload(int id)
        {
            int employerId = (int)Session["EmployerId"];
            using (var db = new CorporatePortalEntities())
            {
                var job = db.Jobs.SingleOrDefault(x => x.JobId == id && x.EmployerId == employerId);
                if (job == null) return HttpNotFound();
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
            if (!ModelState.IsValid) return View(model);
            try
            {
                using (var db = new CorporatePortalEntities())
                {
                    if (!db.Jobs.Any(x => x.JobId == model.JobId.Value && x.EmployerId == employerId)) return HttpNotFound();
                    db.sp_ManageJobs("Update", employerId, model.JobId, model.JobTitle, model.JobType,
                        model.ExperienceRequired, model.Location, model.SalaryRange, model.RequiredSkills,
                        model.JobDescription).FirstOrDefault();
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

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult DeleteJob(int id)
        {
            int employerId = (int)Session["EmployerId"];
            try
            {
                using (var db = new CorporatePortalEntities())
                {
                    if (!db.Jobs.Any(x => x.JobId == id && x.EmployerId == employerId)) return HttpNotFound();
                    db.sp_ManageJobs("Delete", employerId, id, null, null, null, null, null, null, null).FirstOrDefault();
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
