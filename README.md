# CorporatePortal – Online Job Portal

**A Role-Based Job Recruitment and Management System built using ASP.NET MVC 5, C#, Entity Framework 6, and SQL Server.**

CorporatePortal is a web-based job portal designed to connect job seekers with employers through a centralized recruitment platform. It provides job seekers with tools to discover opportunities, manage their applications, save jobs, and track application progress, while employers can publish job openings, manage listings, and review applications.

The platform also includes an administrator module for managing users and job listings.

---

## Project Overview

The main objective of CorporatePortal is to simplify the recruitment process by providing a common platform for job seekers, employers, and administrators.

The application follows a role-based architecture with three primary user roles:

- **Job Seeker:** Searches for jobs, applies for opportunities, manages their profile, and tracks applications.
- **Employer:** Creates company profiles, posts job openings, manages listings, and reviews applicants.
- **Administrator:** Oversees registered users, employers, and job listings.

## Key Features

### 1. Job Seeker Module

Job seekers can create accounts, maintain their professional profiles, explore opportunities, and monitor their application progress.

**Registration and Profile Management**
- Job seeker registration with form validation.
- Profile management with personal and professional information.
- Upload resumes in PDF, DOC, and DOCX formats.
- Maintain qualification, skills, experience, and address details.

**Job Search and Discovery**
- Browse available job opportunities.
- Search jobs by job title and keywords.
- Filter jobs by location, experience, salary range, and required skills.
- View detailed job descriptions and company information.
- Identify job work arrangements: Hybrid, On-site, and Remote.

**Job Applications**
- Apply for jobs with an optional cover letter.
- Prevent duplicate applications.
- View previously submitted applications.
- Track application statuses, including Pending, Shortlisted, Interview, Rejected, and Hired.
- Revisit job details from the application history.

**My Jobs**

A dedicated section that organizes the job seeker's activity into four categories:

| Section | Description |
|---|---|
| Applied | View jobs for which applications have been submitted. |
| Saved | Bookmark jobs for future consideration. |
| Interview | View applications currently in the Interview stage. |
| Archived | Archive jobs and restore them when needed. |

Saved and archived preferences are persisted in the database.

### 2. Employer Module

Employers can manage their company information and handle recruitment activities.

**Company Profile Management**
- Create and update company profiles.
- Maintain company name, website, address, and description.

**Job Management**
- Post new job opportunities.
- Specify job title, description, experience requirements, salary range, location, and required skills.
- Select a work arrangement using a dropdown:
  - Hybrid
  - On-site
  - Remote
- View all posted jobs.
- Edit existing job listings.
- Delete job postings.

**Applicant Management**
- View applications submitted for the employer's jobs.
- Review applicant information and professional details.
- View application dates and cover letters.
- Update application statuses:
  - Pending
  - Shortlisted
  - Interview
  - Rejected
  - Hired

Employers can manage applications belonging to their own job postings.

### 3. Administrator Module

The administrator oversees the overall platform and maintains user and job listing records.

**Admin Dashboard**
- Access the administration dashboard.
- Navigate to user and job management sections.

**User Management**
- View registered job seekers.
- View registered employers.
- Manage and delete user records.

**Job Management**
- View job listings across employers.
- Delete individual job postings.
- Handle dependent application records while maintaining database integrity.

---

## Technology Stack

| Category | Technologies |
|---|---|
| Backend | C#, ASP.NET MVC 5 |
| Framework | .NET Framework 4.7.2 |
| Frontend | HTML5, CSS3, JavaScript |
| UI Framework | Bootstrap |
| Database | Microsoft SQL Server |
| ORM | Entity Framework 6 |
| Data Access | Database-First EDMX, ADO.NET, Parameterized SQL |
| Database Operations | SQL Stored Procedures |
| Development Environment | Visual Studio |
| Database Management | SQL Server Management Studio |
| Web Server | IIS Express |
| Version Control | Git and GitHub |

## Application Architecture

CorporatePortal follows the Model-View-Controller (MVC) architectural pattern.

**MVC Architecture**

- **Model:** Represents application entities, view models, and database-related operations.
- **View:** Razor-based interfaces that display information and collect user input.
- **Controller:** Handles HTTP requests, validates user input, coordinates database operations, and returns appropriate views.

**Database Integration**

The project uses Entity Framework 6 Database-First with an EDMX model for its primary database entities.

Additional database operations use ADO.NET and parameterized SQL queries where appropriate.

Stored procedures are used for important database operations such as registration, login, job management, job searching, and application retrieval.

---

## Database Design

The application uses Microsoft SQL Server as its relational database.

### Main Database Tables

| Table | Purpose |
|---|---|
| Login | Stores login information and user roles. |
| Jobseeker | Stores job seeker details. |
| JobseekerProfile | Stores professional information and resume paths. |
| Employer | Stores employer information. |
| CompanyDetails | Stores company profile information. |
| Jobs | Stores job postings created by employers. |
| JobApplication | Stores applications submitted by job seekers. |
| JobseekerJobPreference | Stores saved and archived job preferences. |

### Important Relationships

- One employer can create multiple job postings.
- An employer can have an associated company profile.
- A job seeker can have an associated professional profile.
- A job can receive applications from multiple job seekers.
- A job seeker can apply for multiple jobs.
- Job applications reference both the corresponding job and job seeker.
- Job seeker preferences reference the corresponding job and job seeker.

Database primary keys, foreign keys, unique constraints, and transactions are used to maintain data consistency.

### Important Stored Procedures

The project uses stored procedures for database operations, including:

- `sp_RegisterUser` – User registration.
- `sp_ValidateLogin` – Login validation.
- `sp_CreateAdmin` – Administrator creation.
- `sp_ManageJobseekerProfile` – Job seeker profile management.
- `sp_ManageCompanyDetails` – Company profile management.
- `sp_ManageJobs` – Job insertion, updating, and deletion.
- `sp_GetJobs` – Job search with multiple filters.
- `sp_GetEmployerJobs` – Retrieves jobs belonging to an employer.
- `sp_GetJobDetails` – Retrieves individual job details.
- `sp_ApplyForJob` – Submits job applications.
- `sp_GetEmployerApplications` – Retrieves applications for an employer's jobs.

---

## UI Design

The application uses an **Emerald & Charcoal** visual theme.

Design characteristics include:

- Emerald green primary accents.
- Charcoal typography and interface elements.
- Clean card-based layouts.
- Consistent forms and navigation.
- Responsive dashboard layouts.
- Role-specific navigation menus.
- Clear application status indicators.

---

## Project Structure

```text
CorporatePortal/
│
├── App_Start/
│   ├── BundleConfig.cs
│   ├── FilterConfig.cs
│   └── RouteConfig.cs
│
├── Content/
│   └── Site.css
│
├── Controllers/
│   ├── AdminController.cs
│   ├── EmployerController.cs
│   ├── JobSeekerController.cs
│   ├── JobSeekerRegController.cs
│   ├── employerRegController.cs
│   ├── LoginController.cs
│   └── HomeController.cs
│
├── Database/
│   └── AddJobseekerJobPreference.sql
│
├── Infrastructure/
│   └── SessionAuthorizeAttribute.cs
│
├── Models/
│   ├── Model1.edmx
│   ├── Model1.Context.cs
│   ├── RegisterViewModel.cs
│   ├── LoginViewModel.cs
│   ├── JobFormViewModel.cs
│   ├── JobseekerProfileViewModel.cs
│   ├── EmployerProfileViewModel.cs
│   ├── MyJobsViewModel.cs
│   ├── EmployerApplicationViewModel.cs
│   └── JobseekerApplicationViewModel.cs
│
├── Views/
│   ├── Admin/
│   ├── Employer/
│   ├── JobSeeker/
│   ├── JobSeekerReg/
│   ├── employerReg/
│   ├── Login/
│   └── Shared/
│
├── UploadedResumes/
├── Web.config
└── CorporatePortal.csproj
```

*The structure above highlights the main project components; additional generated and supporting files are omitted.*

---

## Installation and Setup

### Prerequisites

Make sure the following are installed:

- Visual Studio with ASP.NET and .NET Framework development support.
- .NET Framework 4.7.2 Developer Pack.
- Microsoft SQL Server or SQL Server Express.
- SQL Server Management Studio (SSMS).

### Step 1: Clone the Repository

```bash
git clone <YOUR_GITHUB_REPOSITORY_URL>
```

### Step 2: Open the Project

1. Open the downloaded repository.
2. Open `CorporatePortal.sln` in Visual Studio.
3. Restore the required NuGet packages if necessary.
4. Build the solution.

### Step 3: Configure SQL Server

1. Open SQL Server Management Studio.
2. Create or restore the `CorporatePortal` database with the required schema.
3. Ensure all required tables, relationships, constraints, and stored procedures are present.
4. Execute the preference migration script:

```text
Database/AddJobseekerJobPreference.sql
```

Run this script only when the preference table does not already exist.

### Step 4: Configure Connection Strings

Update the connection strings in `Web.config` to match your local SQL Server instance.

Example for the direct SQL connection:

```xml
<add name="JobFinderDB"
     connectionString="Data Source=YOUR_SERVER_NAME;
     Initial Catalog=CorporatePortal;
     Integrated Security=True;
     MultipleActiveResultSets=True"
     providerName="System.Data.SqlClient" />
```

Also update the provider connection inside the existing `CorporatePortalEntities` Entity Framework connection string.

Ensure both connections target the same database.

### Step 5: Run the Application

1. Set `CorporatePortal` as the startup project.
2. Select IIS Express.
3. Press `F5` or click Start.
4. The application should open in your default browser.

---

## Security and Data Integrity

The application includes:

- Session-based role-specific access control.
- Server-side validation.
- Anti-forgery validation on state-changing POST requests.
- Parameterized SQL queries to protect database operations.
- Employer ownership checks when managing jobs and applications.
- Job seeker ownership checks for personal application and preference data.
- Foreign-key constraints and unique constraints to protect relational integrity.
- Transaction handling for dependent database operations.

---

## Future Enhancements

Possible future improvements include:

- Email notifications for application status changes.
- Employer interview scheduling.
- Job recommendation based on candidate skills.
- Advanced job search and sorting options.
- Analytics dashboards for employers and administrators.
- Enhanced candidate and employer communication.

---

## Project Status

CorporatePortal has implemented its core role-based recruitment workflows, job search, job posting, application management, My Jobs functionality, and administrative features.

The solution has been built successfully, and database structure and connectivity have undergone verification. Complete end-to-end runtime testing of all user workflows remains part of final validation.

---

## Author

**Developed as an ASP.NET MVC Job Portal Project.**

**Technologies:** C# | ASP.NET MVC 5 | Entity Framework 6 | SQL Server | HTML | CSS | JavaScript

