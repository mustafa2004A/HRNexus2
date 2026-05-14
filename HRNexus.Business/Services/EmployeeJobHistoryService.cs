using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Employee;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Repositories.Abstractions;
using HRNexus.DataAccess.Repositories.Employee;

namespace HRNexus.Business.Services;

public sealed class EmployeeJobHistoryService : IEmployeeJobHistoryService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IEmployeeJobHistoryRepository _employeeJobHistoryRepository;
    private readonly IReferenceDataRepository _referenceDataRepository;
    private readonly IHRNexusDbContext _dbContext;

    public EmployeeJobHistoryService(
        IEmployeeRepository employeeRepository,
        IEmployeeJobHistoryRepository employeeJobHistoryRepository,
        IReferenceDataRepository referenceDataRepository,
        IHRNexusDbContext dbContext)
    {
        _employeeRepository = employeeRepository;
        _employeeJobHistoryRepository = employeeJobHistoryRepository;
        _referenceDataRepository = referenceDataRepository;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EmployeeJobHistoryItemDto>> GetEmployeeJobHistoryAsync(
        int employeeId,
        CancellationToken cancellationToken = default)
    {
        await EnsureEmployeeExistsAsync(employeeId, cancellationToken);

        var jobHistory = await _employeeJobHistoryRepository.GetByEmployeeAsync(employeeId, cancellationToken);
        return jobHistory.Select(MapJobHistoryItem).ToList();
    }

    public async Task<EmployeeJobHistoryDto> GetByIdAsync(
        int employeeId,
        int jobHistoryId,
        CancellationToken cancellationToken = default)
    {
        await EnsureEmployeeExistsAsync(employeeId, cancellationToken);
        var jobHistory = await _employeeJobHistoryRepository.GetByIdAsync(employeeId, jobHistoryId, cancellationToken)
            ?? throw JobHistoryNotFound(jobHistoryId);

        return OperationalServiceHelpers.ToEmployeeJobHistoryDto(jobHistory);
    }

    public async Task<EmployeeJobHistoryDto> CreateAsync(
        int employeeId,
        CreateEmployeeJobHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsureEmployeeExistsAsync(employeeId, cancellationToken);
        await ValidateRequestAsync(employeeId, request, cancellationToken);

        if (request.IsCurrent)
        {
            await DemoteCurrentAsync(employeeId, request.StartDate, null, cancellationToken);
        }

        var jobHistory = new EmployeeJobHistory
        {
            EmployeeId = employeeId,
            DepartmentId = request.DepartmentId,
            PositionId = request.PositionId,
            EmploymentTypeId = request.EmploymentTypeId,
            JobGradeId = request.JobGradeId,
            EmploymentStatusId = request.EmploymentStatusId,
            ManagerId = request.ManagerId,
            StartDate = request.StartDate,
            EndDate = request.IsCurrent ? null : request.EndDate,
            IsCurrent = request.IsCurrent
        };

        await _employeeJobHistoryRepository.AddAsync(jobHistory, cancellationToken);
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "create employee job history", cancellationToken);

        return await GetByIdAsync(employeeId, jobHistory.JobHistoryId, cancellationToken);
    }

    public async Task<EmployeeJobHistoryDto> UpdateAsync(
        int employeeId,
        int jobHistoryId,
        UpdateEmployeeJobHistoryRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await EnsureEmployeeExistsAsync(employeeId, cancellationToken);
        await ValidateRequestAsync(employeeId, request, cancellationToken);

        var jobHistory = await _employeeJobHistoryRepository.GetByIdForUpdateAsync(employeeId, jobHistoryId, cancellationToken)
            ?? throw JobHistoryNotFound(jobHistoryId);

        if (jobHistory.IsCurrent && !request.IsCurrent)
        {
            throw new BusinessRuleException("Current job history cannot be changed to non-current directly. Create or update another row as current first.");
        }

        if (request.IsCurrent)
        {
            await DemoteCurrentAsync(employeeId, request.StartDate, jobHistoryId, cancellationToken);
        }

        jobHistory.DepartmentId = request.DepartmentId;
        jobHistory.PositionId = request.PositionId;
        jobHistory.EmploymentTypeId = request.EmploymentTypeId;
        jobHistory.JobGradeId = request.JobGradeId;
        jobHistory.EmploymentStatusId = request.EmploymentStatusId;
        jobHistory.ManagerId = request.ManagerId;
        jobHistory.StartDate = request.StartDate;
        jobHistory.EndDate = request.IsCurrent ? null : request.EndDate;
        jobHistory.IsCurrent = request.IsCurrent;

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "update employee job history", cancellationToken);
        return await GetByIdAsync(employeeId, jobHistoryId, cancellationToken);
    }

    public async Task<EmployeeJobHistoryDto> DeleteAsync(
        int employeeId,
        int jobHistoryId,
        CancellationToken cancellationToken = default)
    {
        await EnsureEmployeeExistsAsync(employeeId, cancellationToken);
        var existing = await GetByIdAsync(employeeId, jobHistoryId, cancellationToken);
        var jobHistory = await _employeeJobHistoryRepository.GetByIdForUpdateAsync(employeeId, jobHistoryId, cancellationToken)
            ?? throw JobHistoryNotFound(jobHistoryId);

        if (jobHistory.IsCurrent)
        {
            throw new BusinessRuleException("Current job history cannot be deleted.");
        }

        _employeeJobHistoryRepository.Remove(jobHistory);
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "delete employee job history", cancellationToken);
        return existing;
    }

    private async Task DemoteCurrentAsync(
        int employeeId,
        DateOnly newCurrentStartDate,
        int? exceptJobHistoryId,
        CancellationToken cancellationToken)
    {
        var currentJob = await _employeeJobHistoryRepository.GetCurrentForUpdateAsync(employeeId, exceptJobHistoryId, cancellationToken);

        if (currentJob is null)
        {
            return;
        }

        currentJob.IsCurrent = false;

        var dayBeforeNewCurrent = newCurrentStartDate.AddDays(-1);
        if (!currentJob.EndDate.HasValue && dayBeforeNewCurrent >= currentJob.StartDate)
        {
            currentJob.EndDate = dayBeforeNewCurrent;
        }
    }

    private async Task ValidateRequestAsync(
        int employeeId,
        CreateEmployeeJobHistoryRequest request,
        CancellationToken cancellationToken)
    {
        if (request.EndDate.HasValue && request.EndDate.Value < request.StartDate)
        {
            throw new BusinessRuleException("Job history end date cannot be earlier than start date.");
        }

        if (request.IsCurrent && request.EndDate.HasValue)
        {
            throw new BusinessRuleException("Current job history cannot have an end date.");
        }

        if (request.ManagerId == employeeId)
        {
            throw new BusinessRuleException("An employee cannot be their own manager.");
        }

        if (request.ManagerId.HasValue && !await _employeeRepository.ExistsAsync(request.ManagerId.Value, cancellationToken))
        {
            throw new BusinessRuleException($"Manager employee {request.ManagerId.Value} was not found.");
        }

        if (!await _referenceDataRepository.DepartmentExistsAsync(request.DepartmentId, cancellationToken))
        {
            throw new BusinessRuleException($"Department {request.DepartmentId} was not found or is inactive.");
        }

        if (!await _referenceDataRepository.PositionExistsAsync(request.PositionId, cancellationToken))
        {
            throw new BusinessRuleException($"Position {request.PositionId} was not found or is inactive.");
        }

        if (!await _referenceDataRepository.EmploymentTypeExistsAsync(request.EmploymentTypeId, cancellationToken))
        {
            throw new BusinessRuleException($"Employment type {request.EmploymentTypeId} was not found.");
        }

        if (!await _referenceDataRepository.JobGradeExistsAsync(request.JobGradeId, cancellationToken))
        {
            throw new BusinessRuleException($"Job grade {request.JobGradeId} was not found.");
        }

        if (!await _referenceDataRepository.EmploymentStatusExistsAsync(request.EmploymentStatusId, cancellationToken))
        {
            throw new BusinessRuleException($"Employment status {request.EmploymentStatusId} was not found.");
        }
    }

    private async Task EnsureEmployeeExistsAsync(int employeeId, CancellationToken cancellationToken)
    {
        if (!await _employeeRepository.ExistsAsync(employeeId, cancellationToken))
        {
            throw new EntityNotFoundException($"Employee {employeeId} was not found.");
        }
    }

    private static EntityNotFoundException JobHistoryNotFound(int jobHistoryId)
    {
        return new EntityNotFoundException($"Employee job history {jobHistoryId} was not found.");
    }

    private static EmployeeJobHistoryItemDto MapJobHistoryItem(EmployeeJobHistoryItemQueryResult jobHistory)
    {
        return new EmployeeJobHistoryItemDto(
            jobHistory.JobHistoryId,
            jobHistory.DepartmentName,
            jobHistory.PositionName,
            jobHistory.EmploymentTypeName,
            jobHistory.EmploymentStatusName,
            jobHistory.ManagerName,
            jobHistory.IsCurrent,
            jobHistory.StartDate,
            jobHistory.EndDate);
    }
}
