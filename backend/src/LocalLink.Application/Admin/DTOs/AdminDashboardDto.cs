namespace LocalLink.Application.Admin.DTOs;

public class AdminDashboardDto
{
    public DashboardCustomersDto Customers { get; set; } = new();
    public DashboardAccountsDto Accounts { get; set; } = new();
    public DashboardTodayDto Today { get; set; } = new();
    public DashboardNotificationsDto Notifications { get; set; } = new();
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
}

public class DashboardCustomersDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Suspended { get; set; }
}

public class DashboardAccountsDto
{
    public int Total { get; set; }
    public int Active { get; set; }
    public int Locked { get; set; }
    public decimal TotalBalance { get; set; }
}

public class DashboardTodayDto
{
    public int Transfers { get; set; }
    public decimal TransferVolume { get; set; }
    public int Payments { get; set; }
    public decimal PaymentVolume { get; set; }
}

public class DashboardNotificationsDto
{
    public int CreatedToday { get; set; }
}
