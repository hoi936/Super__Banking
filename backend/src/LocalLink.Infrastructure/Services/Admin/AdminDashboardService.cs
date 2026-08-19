using LocalLink.Application.Admin.DTOs;
using LocalLink.Application.Admin.Interfaces;
using LocalLink.Domain.Enums;
using LocalLink.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LocalLink.Infrastructure.Services.Admin;

public class AdminDashboardService : IAdminDashboardService
{
    private readonly ApplicationDbContext _context;

    public AdminDashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardDto> GetDashboardMetricsAsync(CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        var totalCustomers = await _context.Customers.CountAsync(cancellationToken);
        var activeCustomers = await _context.Customers.CountAsync(c => c.Status == CustomerStatus.Active, cancellationToken);
        var suspendedCustomers = await _context.Customers.CountAsync(c => c.Status == CustomerStatus.Suspended, cancellationToken);

        var totalAccounts = await _context.BankAccounts.CountAsync(cancellationToken);
        var activeAccounts = await _context.BankAccounts.CountAsync(a => a.Status == AccountStatus.Active, cancellationToken);
        var lockedAccounts = await _context.BankAccounts.CountAsync(a => a.Status == AccountStatus.Locked, cancellationToken);
        var totalBalance = await _context.BankAccounts.SumAsync(a => a.Balance, cancellationToken);

        var todayTransfers = await _context.Transfers
            .Where(t => t.CreatedAtUtc >= today && t.Status == TransferStatus.Completed)
            .ToListAsync(cancellationToken);
            
        var todayPayments = await _context.Payments
            .Where(p => p.CreatedAtUtc >= today && p.Status == PaymentStatus.Completed)
            .ToListAsync(cancellationToken);

        var notificationsToday = await _context.Notifications
            .CountAsync(n => n.CreatedAtUtc >= today, cancellationToken);

        return new AdminDashboardDto
        {
            Customers = new DashboardCustomersDto
            {
                Total = totalCustomers,
                Active = activeCustomers,
                Suspended = suspendedCustomers
            },
            Accounts = new DashboardAccountsDto
            {
                Total = totalAccounts,
                Active = activeAccounts,
                Locked = lockedAccounts,
                TotalBalance = totalBalance
            },
            Today = new DashboardTodayDto
            {
                Transfers = todayTransfers.Count,
                TransferVolume = todayTransfers.Sum(t => t.Amount),
                Payments = todayPayments.Count,
                PaymentVolume = todayPayments.Sum(p => p.Amount)
            },
            Notifications = new DashboardNotificationsDto
            {
                CreatedToday = notificationsToday
            },
            GeneratedAtUtc = DateTime.UtcNow
        };
    }
}
