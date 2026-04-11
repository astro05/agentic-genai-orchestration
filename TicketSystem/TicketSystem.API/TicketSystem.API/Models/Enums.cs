namespace TicketSystem.API.Models;

public enum UserRole
{
    Admin = 0,
    Agent = 1,
    Customer = 2
}

public enum TicketStatus
{
    Open = 0,
    InProgress = 1,
    Resolved = 2
}

public enum TicketPriority
{
    Low = 0,
    Medium = 1,
    High = 2
}

public enum TicketCategory
{
    AuthenticationIssue = 0,
    BillingIssue = 1,
    TechnicalIssue = 2,
    PerformanceIssue = 3,
    FeatureRequest = 4,
    GeneralInquiry = 5,
    AccountManagement = 6,
    NotificationIssue = 7,
    DataIssue = 8,
    SecurityIssue = 9,
    UncategorizedIssue = 10
}