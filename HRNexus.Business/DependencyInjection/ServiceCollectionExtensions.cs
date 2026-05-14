using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Leave;
using HRNexus.Business.Models.Lookup;
using HRNexus.Business.Services;
using HRNexus.DataAccess.Entities.Core;
using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Entities.Leave;
using HRNexus.DataAccess.Entities.Security;
using Microsoft.Extensions.DependencyInjection;
using ModuleEntity = HRNexus.DataAccess.Entities.Security.Module;

namespace HRNexus.Business.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<IPersonContactService, PersonContactService>();
        services.AddScoped<IAddressService, AddressService>();
        services.AddScoped<IPersonIdentifierService, PersonIdentifierService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IEmployeeJobHistoryService, EmployeeJobHistoryService>();
        services.AddScoped<IEmployeeDocumentService, EmployeeDocumentService>();
        services.AddScoped<IEmployeeFamilyMemberService, EmployeeFamilyMemberService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ISecurityAdminService, SecurityAdminService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IUserActivityLogService, UserActivityLogService>();
        services.AddScoped<PasswordVerificationService>();
        services.AddScoped<IPasswordVerificationService>(provider => provider.GetRequiredService<PasswordVerificationService>());
        services.AddScoped<IPasswordHashingService>(provider => provider.GetRequiredService<PasswordVerificationService>());
        services.AddScoped<IDevelopmentPasswordBootstrapService, DevelopmentPasswordBootstrapService>();
        services.AddScoped<ILeaveRequestService, LeaveRequestService>();
        services.AddScoped<ILeaveBalanceService, LeaveBalanceService>();
        services.AddScoped<IHolidayService, HolidayService>();
        services.AddScoped<ILeaveAttachmentService, LeaveAttachmentService>();

        services.AddLookupCrud<Country, CountryDto, CreateCountryRequest, UpdateCountryRequest, CountryLookupDefinition>();
        services.AddLookupCrud<City, CityDto, CreateCityRequest, UpdateCityRequest, CityLookupDefinition>();
        services.AddLookupCrud<Gender, GenderDto, CreateGenderRequest, UpdateGenderRequest, GenderLookupDefinition>();
        services.AddLookupCrud<MaritalStatus, MaritalStatusDto, CreateMaritalStatusRequest, UpdateMaritalStatusRequest, MaritalStatusLookupDefinition>();
        services.AddLookupCrud<ContactType, ContactTypeDto, CreateContactTypeRequest, UpdateContactTypeRequest, ContactTypeLookupDefinition>();
        services.AddLookupCrud<AddressType, AddressTypeDto, CreateAddressTypeRequest, UpdateAddressTypeRequest, AddressTypeLookupDefinition>();
        services.AddLookupCrud<IdentifierType, IdentifierTypeDto, CreateIdentifierTypeRequest, UpdateIdentifierTypeRequest, IdentifierTypeLookupDefinition>();

        services.AddLookupCrud<EmploymentType, EmploymentTypeDto, CreateEmploymentTypeRequest, UpdateEmploymentTypeRequest, EmploymentTypeLookupDefinition>();
        services.AddLookupCrud<EmploymentStatus, EmploymentStatusDto, CreateEmploymentStatusRequest, UpdateEmploymentStatusRequest, EmploymentStatusLookupDefinition>();
        services.AddLookupCrud<Department, DepartmentDto, CreateDepartmentRequest, UpdateDepartmentRequest, DepartmentLookupDefinition>();
        services.AddLookupCrud<Position, PositionDto, CreatePositionRequest, UpdatePositionRequest, PositionLookupDefinition>();
        services.AddLookupCrud<JobGrade, JobGradeDto, CreateJobGradeRequest, UpdateJobGradeRequest, JobGradeLookupDefinition>();
        services.AddLookupCrud<TerminationReason, TerminationReasonDto, CreateTerminationReasonRequest, UpdateTerminationReasonRequest, TerminationReasonLookupDefinition>();
        services.AddLookupCrud<RelationshipType, RelationshipTypeDto, CreateRelationshipTypeRequest, UpdateRelationshipTypeRequest, RelationshipTypeLookupDefinition>();
        services.AddLookupCrud<DocumentType, DocumentTypeDto, CreateDocumentTypeRequest, UpdateDocumentTypeRequest, DocumentTypeLookupDefinition>();

        services.AddLookupCrud<LeaveType, LeaveTypeDto, CreateLeaveTypeRequest, UpdateLeaveTypeRequest, LeaveTypeLookupDefinition>();
        services.AddLookupCrud<Holiday, HolidayDto, CreateHolidayRequest, UpdateHolidayRequest, HolidayLookupDefinition>();
        services.AddLookupCrud<RequestStatus, RequestStatusDto, CreateRequestStatusRequest, UpdateRequestStatusRequest, RequestStatusLookupDefinition>();

        services.AddLookupCrud<Role, RoleDto, CreateRoleRequest, UpdateRoleRequest, RoleLookupDefinition>();
        services.AddLookupCrud<ModuleEntity, SecurityModuleDto, CreateSecurityModuleRequest, UpdateSecurityModuleRequest, SecurityModuleLookupDefinition>();
        services.AddLookupCrud<Permission, PermissionDto, CreatePermissionRequest, UpdatePermissionRequest, PermissionLookupDefinition>();
        services.AddLookupCrud<AccountStatus, AccountStatusDto, CreateAccountStatusRequest, UpdateAccountStatusRequest, AccountStatusLookupDefinition>();
        services.AddLookupCrud<ActivityType, ActivityTypeDto, CreateActivityTypeRequest, UpdateActivityTypeRequest, ActivityTypeLookupDefinition>();

        return services;
    }

    private static IServiceCollection AddLookupCrud<TEntity, TDto, TCreateRequest, TUpdateRequest, TDefinition>(
        this IServiceCollection services)
        where TEntity : class
        where TDefinition : class, ILookupCrudDefinition<TEntity, TDto, TCreateRequest, TUpdateRequest>
    {
        services.AddScoped<ILookupCrudDefinition<TEntity, TDto, TCreateRequest, TUpdateRequest>, TDefinition>();
        services.AddScoped<ILookupCrudService<TDto, TCreateRequest, TUpdateRequest>, LookupCrudService<TEntity, TDto, TCreateRequest, TUpdateRequest>>();

        return services;
    }
}
