using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Entities.Leave;
using HRNexus.DataAccess.Entities.Security;
using Microsoft.EntityFrameworkCore;
using EmployeeEntity = HRNexus.DataAccess.Entities.Employee.Employee;

namespace HRNexus.DataAccess.Context;

public sealed class HRNexusDbContext : DbContext, IHRNexusDbContext
{
    public HRNexusDbContext(DbContextOptions<HRNexusDbContext> options)
        : base(options)
    {
    }

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Gender> Genders => Set<Gender>();
    public DbSet<MaritalStatus> MaritalStatuses => Set<MaritalStatus>();
    public DbSet<ContactType> ContactTypes => Set<ContactType>();
    public DbSet<AddressType> AddressTypes => Set<AddressType>();
    public DbSet<IdentifierType> IdentifierTypes => Set<IdentifierType>();
    public DbSet<Person> People => Set<Person>();
    public DbSet<PersonContact> PersonContacts => Set<PersonContact>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<PersonIdentifier> PersonIdentifiers => Set<PersonIdentifier>();
    public DbSet<FileStorageItem> FileStorageItems => Set<FileStorageItem>();

    public DbSet<EmploymentStatus> EmploymentStatuses => Set<EmploymentStatus>();
    public DbSet<EmploymentType> EmploymentTypes => Set<EmploymentType>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<JobGrade> JobGrades => Set<JobGrade>();
    public DbSet<TerminationReason> TerminationReasons => Set<TerminationReason>();
    public DbSet<RelationshipType> RelationshipTypes => Set<RelationshipType>();
    public DbSet<DocumentType> DocumentTypes => Set<DocumentType>();
    public DbSet<EmployeeEntity> Employees => Set<EmployeeEntity>();
    public DbSet<EmployeeJobHistory> EmployeeJobHistories => Set<EmployeeJobHistory>();
    public DbSet<EmployeeDocument> EmployeeDocuments => Set<EmployeeDocument>();
    public DbSet<EmployeeFamilyMember> EmployeeFamilyMembers => Set<EmployeeFamilyMember>();

    public DbSet<AccountStatus> AccountStatuses => Set<AccountStatus>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<PermissionAudit> PermissionAudits => Set<PermissionAudit>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ActivityType> ActivityTypes => Set<ActivityType>();
    public DbSet<UserActivityLog> UserActivityLogs => Set<UserActivityLog>();

    public DbSet<LeaveType> LeaveTypes => Set<LeaveType>();
    public DbSet<RequestStatus> RequestStatuses => Set<RequestStatus>();
    public DbSet<LeaveBalance> LeaveBalances => Set<LeaveBalance>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<LeaveAttachment> LeaveAttachments => Set<LeaveAttachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HRNexusDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
