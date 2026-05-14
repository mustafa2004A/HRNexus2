using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Context;
using HRNexus.DataAccess.Repositories.Core;
using HRNexus.DataAccess.Repositories.Dashboard;
using HRNexus.DataAccess.Repositories.Abstractions;
using HRNexus.DataAccess.Repositories.Employee;
using HRNexus.DataAccess.Repositories.Leave;
using HRNexus.DataAccess.Repositories.Lookup;
using HRNexus.DataAccess.Repositories.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HRNexus.DataAccess.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<HRNexusDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

        services.AddScoped<IHRNexusDbContext>(provider => provider.GetRequiredService<HRNexusDbContext>());
        services.AddScoped(typeof(ILookupCrudRepository<>), typeof(LookupCrudRepository<>));
        services.AddScoped<IReferenceDataRepository, ReferenceDataRepository>();

        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<IFileStorageRepository, FileStorageRepository>();
        services.AddScoped<IPersonRepository, PersonRepository>();
        services.AddScoped<IPersonContactRepository, PersonContactRepository>();
        services.AddScoped<IAddressRepository, AddressRepository>();
        services.AddScoped<IPersonIdentifierRepository, PersonIdentifierRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IEmployeeJobHistoryRepository, EmployeeJobHistoryRepository>();
        services.AddScoped<IEmployeeDocumentRepository, EmployeeDocumentRepository>();
        services.AddScoped<IEmployeeFamilyMemberRepository, EmployeeFamilyMemberRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISecurityAdminRepository, SecurityAdminRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IActivityTypeRepository, ActivityTypeRepository>();
        services.AddScoped<IUserActivityLogRepository, UserActivityLogRepository>();
        services.AddScoped<ILeaveTypeRepository, LeaveTypeRepository>();
        services.AddScoped<IRequestStatusRepository, RequestStatusRepository>();
        services.AddScoped<ILeaveBalanceRepository, LeaveBalanceRepository>();
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<IHolidayRepository, HolidayRepository>();
        services.AddScoped<ILeaveAttachmentRepository, LeaveAttachmentRepository>();

        return services;
    }
}
