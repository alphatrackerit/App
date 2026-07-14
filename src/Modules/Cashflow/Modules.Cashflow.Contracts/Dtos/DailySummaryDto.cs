namespace FSH.Modules.Cashflow.Contracts.Dtos;

/// <summary>One project's daily cash-flow series, with its denormalized catalog names.</summary>
public sealed record DailySummaryProjectDto(
    Guid ProjectId,
    string ProjectName,
    string ClientName,
    string CompanyName,
    string CountryName,
    string StatusName,
    Guid? StatusId,
    IReadOnlyList<DailySummaryDayDto> Days);

/// <summary>A single day for a project: totals, running balance, and the predominant-supplier color.</summary>
public sealed record DailySummaryDayDto(
    DateTime Date,
    decimal TotalIncomes,
    decimal TotalPayments,
    decimal Result,        // TotalIncomes - TotalPayments
    decimal Accumulated,   // running sum of Result for this project
    string ColorHex,       // color of the day's highest-VisualPriority supplier; "#ffffff00" if none
    int VisualPriority,    // that supplier's VisualPriority; int.MaxValue if none
    IReadOnlyList<IncomeDetailDto> IncomeDetails,
    IReadOnlyList<PaymentDetailDto> PaymentDetails);

public sealed record IncomeDetailDto(
    Guid Id,
    decimal Amount,
    decimal Percentage,
    string Status,
    string? Description,
    bool Confirmed,
    bool Validated);

public sealed record PaymentDetailDto(
    Guid Id,
    decimal Amount,
    decimal Percentage,
    string Status,
    string SupplierName,
    string? Description,
    bool Confirmed,
    bool Validated);
