# CorporatePortal

CorporatePortal is an ASP.NET MVC web application for job seekers, employers, and administrators. It supports job search and applications, employer job postings and application management, and administrative management of portal records.

## Features

- **Job seekers:** register and sign in, manage a profile, search and filter job listings, view job details, apply to jobs, and save or archive listings.
- **Employers:** register and sign in, manage a company profile, post and edit jobs, and review applications and their statuses.
- **Administrators:** review and manage job seeker, employer, and job records.
- **Responsive interface:** shared site styling with Bootstrap and custom Emerald & Charcoal design elements.

## Technology

- ASP.NET MVC 5 with Razor views
- .NET Framework 4.7.2
- Entity Framework 6 and SQL Server
- Bootstrap 3, jQuery, and custom CSS

## Requirements

- Windows with Visual Studio (ASP.NET and web development workload)
- .NET Framework 4.7.2 developer/targeting pack
- SQL Server instance and a `CorporatePortal` database configured for the application

## Getting started

1. Clone the repository and open `CorporatePortal.sln` in Visual Studio.
2. Restore the NuGet packages. The solution uses `CorporatePortal/packages.config`.
3. Configure the `CorporatePortalEntities` Entity Framework connection string in `CorporatePortal/Web.config` for your SQL Server instance and database. The checked-in connection string is machine-specific; replace it with your local database settings and do not commit credentials or private connection details.
4. Ensure the database has the schema and stored procedures referenced by the Entity Framework model. The repository does not include a complete database initialization script.
5. For the Saved/Archived jobs feature, review and run `CorporatePortal/Database/AddJobseekerJobPreference.sql` against the configured database. The script expects the existing `dbo.Jobseeker` and `dbo.Jobs` tables and their `JobseekerId` and `JobId` keys.
6. Set `CorporatePortal` as the startup project and run it with IIS Express from Visual Studio.

## Project structure

```text
CorporatePortal.sln
CorporatePortal/
  Controllers/     MVC controllers for home, login, job seekers, employers, and administration
  Models/          Entity Framework model and view models
  Views/           Razor views grouped by feature
  Content/         Stylesheets and image assets
  Scripts/         JavaScript and client-side validation libraries
  Database/        Supplemental SQL scripts
```

## Configuration notes

- Database access is defined by the `CorporatePortalEntities` connection string in `CorporatePortal/Web.config`.
- Authentication uses the application's session-based flow.
- Uploaded resumes are stored under `UploadedResumes/`; that directory is excluded from Git.

## License

No license is currently specified. Contact the repository owner before reusing or distributing this project.
