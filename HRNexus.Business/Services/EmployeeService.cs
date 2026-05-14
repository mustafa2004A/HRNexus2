using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Core;
using HRNexus.Business.Models.Employee;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Repositories.Abstractions;
using HRNexus.DataAccess.Repositories.Employee;
using EmployeeEntity = HRNexus.DataAccess.Entities.Employee.Employee;

namespace HRNexus.Business.Services;

public sealed class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IReferenceDataRepository _referenceDataRepository;
    private readonly ICurrentUserContext _currentUserContext;
    private readonly IHRNexusDbContext _dbContext;

    public EmployeeService(
        IEmployeeRepository employeeRepository,
        IReferenceDataRepository referenceDataRepository,
        ICurrentUserContext currentUserContext,
        IHRNexusDbContext dbContext)
    {
        _employeeRepository = employeeRepository;
        _referenceDataRepository = referenceDataRepository;
        _currentUserContext = currentUserContext;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<EmployeeSummaryDto>> ListAsync(
        string? search,
        bool includeDeleted,
        CancellationToken cancellationToken = default)
    {
        var employees = await _employeeRepository.ListAsync(search, includeDeleted, cancellationToken);
        return employees.Select(OperationalServiceHelpers.ToEmployeeSummaryDto).ToList();
    }

    public async Task<EmployeeDetailsDto> GetDetailsAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetDetailsAsync(employeeId, cancellationToken)
            ?? throw CreateNotFoundException(employeeId);

        return MapDetails(employee);
    }

    public async Task<EmployeeCurrentContextDto> GetCurrentContextAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await _employeeRepository.GetCurrentContextAsync(employeeId, cancellationToken)
            ?? throw CreateNotFoundException(employeeId);

        return MapCurrentContext(employee);
    }

    public async Task<EmployeeDetailsDto> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Person);
        ArgumentNullException.ThrowIfNull(request.Employee);

        await ValidatePersonReferencesAsync(request.Person, cancellationToken);
        await ValidateEmployeeCoreAsync(request.Employee, cancellationToken);

        var now = DateTime.UtcNow;
        var nextEmployeeCodeNumber = await _employeeRepository.GetNextEmployeeCodeNumberAsync(cancellationToken);
        var person = PersonService.CreatePersonEntity(request.Person);
        person.CreatedBy = _currentUserContext.UserId;
        person.CreatedDate = now;

        var employee = new EmployeeEntity
        {
            Person = person,
            EmployeeCode = FormatEmployeeCode(nextEmployeeCodeNumber),
            HireDate = request.Employee.HireDate,
            CurrentEmploymentStatusId = request.Employee.CurrentEmploymentStatusId,
            TerminationReasonId = request.Employee.TerminationReasonId,
            TerminationDate = request.Employee.TerminationDate,
            IsEligibleForRehire = request.Employee.IsEligibleForRehire,
            CreatedBy = _currentUserContext.UserId,
            CreatedDate = now
        };

        if (request.InitialJob is not null)
        {
            await ValidateInitialJobAsync(request.InitialJob, request.Employee.HireDate, cancellationToken);
            employee.JobHistories.Add(new EmployeeJobHistory
            {
                DepartmentId = request.InitialJob.DepartmentId,
                PositionId = request.InitialJob.PositionId,
                EmploymentTypeId = request.InitialJob.EmploymentTypeId,
                JobGradeId = request.InitialJob.JobGradeId,
                EmploymentStatusId = request.Employee.CurrentEmploymentStatusId,
                ManagerId = request.InitialJob.ManagerId,
                StartDate = request.InitialJob.StartDate,
                IsCurrent = true
            });
        }

        await _employeeRepository.AddAsync(employee, cancellationToken);
        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "create employee", cancellationToken);

        return await GetDetailsAsync(employee.EmployeeId, cancellationToken);
    }

    public async Task<EmployeeDetailsDto> UpdateAsync(int employeeId, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Person);
        ArgumentNullException.ThrowIfNull(request.Employee);

        await ValidatePersonReferencesAsync(request.Person, cancellationToken);
        await ValidateEmployeeCoreAsync(request.Employee, cancellationToken);

        var employee = await _employeeRepository.GetByIdForUpdateAsync(employeeId, cancellationToken)
            ?? throw CreateNotFoundException(employeeId);

        if (employee.IsDeleted || employee.Person.IsDeleted)
        {
            throw CreateNotFoundException(employeeId);
        }

        PersonService.ApplyPersonRequest(employee.Person, request.Person);
        employee.Person.ModifiedBy = _currentUserContext.UserId;
        employee.Person.ModifiedDate = DateTime.UtcNow;

        employee.HireDate = request.Employee.HireDate;
        employee.CurrentEmploymentStatusId = request.Employee.CurrentEmploymentStatusId;
        employee.TerminationReasonId = request.Employee.TerminationReasonId;
        employee.TerminationDate = request.Employee.TerminationDate;
        employee.IsEligibleForRehire = request.Employee.IsEligibleForRehire;
        employee.ModifiedBy = _currentUserContext.UserId;
        employee.ModifiedDate = DateTime.UtcNow;

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "update employee", cancellationToken);
        return await GetDetailsAsync(employeeId, cancellationToken);
    }

    public async Task<EmployeeDetailsDto> DeleteAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var existing = await GetDetailsAsync(employeeId, cancellationToken);
        var employee = await _employeeRepository.GetByIdForUpdateAsync(employeeId, cancellationToken)
            ?? throw CreateNotFoundException(employeeId);

        employee.IsDeleted = true;
        employee.DeletedBy = _currentUserContext.UserId;
        employee.DeletedDate = DateTime.UtcNow;
        employee.ModifiedBy = _currentUserContext.UserId;
        employee.ModifiedDate = employee.DeletedDate;

        await OperationalServiceHelpers.SaveChangesAsync(_dbContext, "soft-delete employee", cancellationToken);
        return existing;
    }

    private async Task ValidatePersonReferencesAsync(CreatePersonRequest request, CancellationToken cancellationToken)
    {
        if (request.DateOfBirth.HasValue && request.DateOfBirth.Value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            throw new BusinessRuleException("Date of birth cannot be in the future.");
        }

        if (request.GenderId.HasValue && !await _referenceDataRepository.GenderExistsAsync(request.GenderId.Value, cancellationToken))
        {
            throw new BusinessRuleException($"Gender {request.GenderId.Value} was not found.");
        }

        if (request.MaritalStatusId.HasValue && !await _referenceDataRepository.MaritalStatusExistsAsync(request.MaritalStatusId.Value, cancellationToken))
        {
            throw new BusinessRuleException($"Marital status {request.MaritalStatusId.Value} was not found.");
        }

        if (request.NationalityCountryId.HasValue && !await _referenceDataRepository.CountryExistsAsync(request.NationalityCountryId.Value, cancellationToken))
        {
            throw new BusinessRuleException($"Nationality country {request.NationalityCountryId.Value} was not found.");
        }
    }

    private async Task ValidateEmployeeCoreAsync(CreateEmployeeCoreRequest request, CancellationToken cancellationToken)
    {
        if (!await _referenceDataRepository.EmploymentStatusExistsAsync(request.CurrentEmploymentStatusId, cancellationToken))
        {
            throw new BusinessRuleException($"Employment status {request.CurrentEmploymentStatusId} was not found.");
        }

        if (request.TerminationReasonId.HasValue
            && !await _referenceDataRepository.TerminationReasonExistsAsync(request.TerminationReasonId.Value, cancellationToken))
        {
            throw new BusinessRuleException($"Termination reason {request.TerminationReasonId.Value} was not found.");
        }

        if (request.TerminationReasonId.HasValue != request.TerminationDate.HasValue)
        {
            throw new BusinessRuleException("Termination reason and termination date must be provided together.");
        }

        if (request.TerminationDate.HasValue && request.TerminationDate.Value < request.HireDate)
        {
            throw new BusinessRuleException("Termination date cannot be earlier than hire date.");
        }
    }

    private async Task ValidateInitialJobAsync(
        CreateInitialJobAssignmentRequest request,
        DateOnly hireDate,
        CancellationToken cancellationToken)
    {
        await ValidateJobReferenceAsync(request.DepartmentId, request.PositionId, request.EmploymentTypeId, request.JobGradeId, cancellationToken);

        if (request.ManagerId.HasValue && !await _employeeRepository.ExistsAsync(request.ManagerId.Value, cancellationToken))
        {
            throw new BusinessRuleException($"Manager employee {request.ManagerId.Value} was not found.");
        }

        if (request.StartDate < hireDate)
        {
            throw new BusinessRuleException("Initial job start date cannot be earlier than hire date.");
        }
    }

    private async Task ValidateJobReferenceAsync(
        int departmentId,
        int positionId,
        int employmentTypeId,
        int jobGradeId,
        CancellationToken cancellationToken)
    {
        if (!await _referenceDataRepository.DepartmentExistsAsync(departmentId, cancellationToken))
        {
            throw new BusinessRuleException($"Department {departmentId} was not found or is inactive.");
        }

        if (!await _referenceDataRepository.PositionExistsAsync(positionId, cancellationToken))
        {
            throw new BusinessRuleException($"Position {positionId} was not found or is inactive.");
        }

        if (!await _referenceDataRepository.EmploymentTypeExistsAsync(employmentTypeId, cancellationToken))
        {
            throw new BusinessRuleException($"Employment type {employmentTypeId} was not found.");
        }

        if (!await _referenceDataRepository.JobGradeExistsAsync(jobGradeId, cancellationToken))
        {
            throw new BusinessRuleException($"Job grade {jobGradeId} was not found.");
        }
    }

    private static EntityNotFoundException CreateNotFoundException(int employeeId)
    {
        return new EntityNotFoundException($"Employee {employeeId} was not found.");
    }

    private static string FormatEmployeeCode(int sequenceNumber)
    {
        return $"HRN-EMP-{sequenceNumber:D4}";
    }

    private static EmployeeDetailsDto MapDetails(EmployeeDetailsQueryResult employee)
    {
        return new EmployeeDetailsDto(
            employee.EmployeeId,
            employee.EmployeeCode,
            employee.PersonId,
            employee.FullName,
            employee.HireDate,
            employee.EmploymentStatusName,
            employee.TerminationDate,
            employee.IsEligibleForRehire,
            OperationalServiceHelpers.ToSafeNullableRelativeFilePath(employee.PhotoUrl),
            employee.PhotoFileStorageItemId);
    }

    private static EmployeeCurrentContextDto MapCurrentContext(EmployeeCurrentContextQueryResult employee)
    {
        return new EmployeeCurrentContextDto(
            employee.EmployeeId,
            employee.EmployeeCode,
            employee.FullName,
            employee.DepartmentName,
            employee.PositionName,
            employee.EmploymentTypeName,
            employee.EmploymentStatusName,
            employee.ManagerId,
            employee.ManagerName,
            employee.CurrentAssignmentStartDate);
    }
}
