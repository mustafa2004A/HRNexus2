using HRNexus.Business.Exceptions;
using HRNexus.Business.Interfaces;
using HRNexus.Business.Models.Leave;
using HRNexus.Business.Validation;
using HRNexus.DataAccess.Abstractions;
using HRNexus.DataAccess.Entities.Employee;
using HRNexus.DataAccess.Entities.Leave;
using HRNexus.DataAccess.Repositories.Abstractions;

namespace HRNexus.Business.Services;

public sealed class LeaveBalanceService : ILeaveBalanceService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILeaveTypeRepository _leaveTypeRepository;
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly IHRNexusDbContext _dbContext;

    public LeaveBalanceService(
        IEmployeeRepository employeeRepository,
        ILeaveTypeRepository leaveTypeRepository,
        ILeaveBalanceRepository leaveBalanceRepository,
        IHRNexusDbContext dbContext)
    {
        _employeeRepository = employeeRepository;
        _leaveTypeRepository = leaveTypeRepository;
        _leaveBalanceRepository = leaveBalanceRepository;
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<LeaveBalanceDto>> ListBalancesAsync(
        int? employeeId = null,
        int? leaveTypeId = null,
        int? balanceYear = null,
        CancellationToken cancellationToken = default)
    {
        var balances = await _leaveBalanceRepository.ListAsync(employeeId, leaveTypeId, balanceYear, cancellationToken);
        return balances.Select(MapBalance).ToList();
    }

    public async Task<LeaveBalanceDto> GetBalanceAsync(int leaveBalanceId, CancellationToken cancellationToken = default)
    {
        var balance = await _leaveBalanceRepository.GetByIdAsync(leaveBalanceId, cancellationToken)
            ?? throw new EntityNotFoundException($"Leave balance {leaveBalanceId} was not found.");

        return MapBalance(balance);
    }

    public async Task<IReadOnlyList<LeaveBalanceDto>> GetEmployeeBalancesAsync(int employeeId, int? balanceYear = null, CancellationToken cancellationToken = default)
    {
        var employee = await GetEmployeeAsync(employeeId, cancellationToken);
        var balances = await _leaveBalanceRepository.GetByEmployeeAsync(employee.EmployeeId, balanceYear, cancellationToken);

        return balances
            .Select(balance => MapBalance(balance, employee.EmployeeCode, employee.Person.FullName))
            .ToList();
    }

    public async Task<LeaveBalanceDto> UpsertBalanceAsync(UpsertLeaveBalanceRequest request, CancellationToken cancellationToken = default)
    {
        LeaveValidation.EnsureValid(request);

        var employee = await GetEmployeeAsync(request.EmployeeId, cancellationToken);
        var leaveType = await _leaveTypeRepository.GetByIdAsync(request.LeaveTypeId, cancellationToken)
            ?? throw new EntityNotFoundException($"Leave type {request.LeaveTypeId} was not found.");

        var remainingDays = request.EntitledDays - request.UsedDays;

        var balance = await _leaveBalanceRepository.GetByEmployeeLeaveTypeYearAsync(
            request.EmployeeId,
            request.LeaveTypeId,
            request.BalanceYear,
            asTracking: true,
            cancellationToken);

        if (balance is null)
        {
            balance = new LeaveBalance
            {
                EmployeeId = request.EmployeeId,
                LeaveTypeId = request.LeaveTypeId,
                BalanceYear = request.BalanceYear,
                EntitledDays = request.EntitledDays,
                UsedDays = request.UsedDays,
                RemainingDays = remainingDays,
                LastUpdated = DateTime.UtcNow
            };

            await _leaveBalanceRepository.AddAsync(balance, cancellationToken);
        }
        else
        {
            balance.EntitledDays = request.EntitledDays;
            balance.UsedDays = request.UsedDays;
            balance.RemainingDays = remainingDays;
            balance.LastUpdated = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        balance.LeaveType = leaveType;

        return MapBalance(balance, employee.EmployeeCode, employee.Person.FullName);
    }

    private async Task<Employee> GetEmployeeAsync(int employeeId, CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken)
            ?? throw new EntityNotFoundException($"Employee {employeeId} was not found.");

        if (employee.IsDeleted)
        {
            throw new BusinessRuleException($"Employee {employeeId} is deleted and cannot be used for leave balances.");
        }

        return employee;
    }

    private static LeaveBalanceDto MapBalance(LeaveBalance balance, string employeeCode, string employeeName)
    {
        return new LeaveBalanceDto(
            balance.LeaveBalanceId,
            balance.EmployeeId,
            employeeCode,
            employeeName,
            balance.LeaveTypeId,
            balance.LeaveType.LeaveTypeName,
            balance.LeaveType.LeaveTypeCode,
            balance.BalanceYear,
            balance.EntitledDays,
            balance.UsedDays,
            balance.RemainingDays,
            balance.LastUpdated);
    }

    private static LeaveBalanceDto MapBalance(LeaveBalance balance)
    {
        return MapBalance(balance, balance.Employee.EmployeeCode, balance.Employee.Person.FullName);
    }
}
