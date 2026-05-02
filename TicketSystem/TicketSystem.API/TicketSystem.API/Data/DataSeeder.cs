using TicketSystem.API.Models;
using TicketSystem.API.Repositories;

namespace TicketSystem.API.Data
{
    public class DataSeeder
    {
        private readonly IUserRepository _userRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IKnowledgeArticleRepository _knowledgeRepository;

        public DataSeeder(
            IUserRepository userRepository,
            ITicketRepository ticketRepository,
            IKnowledgeArticleRepository knowledgeRepository)
        {
            _userRepository = userRepository;
            _ticketRepository = ticketRepository;
            _knowledgeRepository = knowledgeRepository;
        }

        public async Task SeedAsync()
        {
            await SeedUsersIfEmptyAsync();
            await SeedKnowledgeBaseIfEmptyAsync();
            await SeedSampleTicketsIfEmptyAsync();
        }

        private async Task SeedUsersIfEmptyAsync()
        {
            if ((await _userRepository.GetAllAsync()).Count > 0)
                return;

            var admin = new User
            {
                FullName = "System Admin",
                Email = "admin@ticketsys.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin
            };

            var agentAuth = new User
            {
                FullName = "Sarah (Auth & Accounts)",
                Email = "agent.auth@ticketsys.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Agent@123"),
                Role = UserRole.Agent,
                HandledCategories = new List<string>
                {
                    nameof(TicketCategory.AuthenticationIssue),
                    nameof(TicketCategory.AccountManagement),
                    nameof(TicketCategory.SecurityIssue)
                }
            };

            var agentBilling = new User
            {
                FullName = "Mike (Billing)",
                Email = "agent.billing@ticketsys.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Agent@123"),
                Role = UserRole.Agent,
                HandledCategories = new List<string> { nameof(TicketCategory.BillingIssue) }
            };

            var agentTech = new User
            {
                FullName = "Taylor (Technical)",
                Email = "agent.tech@ticketsys.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Agent@123"),
                Role = UserRole.Agent,
                HandledCategories = new List<string>
                {
                    nameof(TicketCategory.TechnicalIssue),
                    nameof(TicketCategory.PerformanceIssue),
                    nameof(TicketCategory.DataIssue),
                    nameof(TicketCategory.NotificationIssue)
                }
            };

            var agentGeneral = new User
            {
                FullName = "Alex (Generalist)",
                Email = "agent@ticketsys.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Agent@123"),
                Role = UserRole.Agent,
                HandledCategories = new List<string>()
            };

            var customer = new User
            {
                FullName = "John Customer",
                Email = "customer@ticketsys.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                Role = UserRole.Customer
            };

            await _userRepository.InsertAsync(admin);
            await _userRepository.InsertAsync(agentAuth);
            await _userRepository.InsertAsync(agentBilling);
            await _userRepository.InsertAsync(agentTech);
            await _userRepository.InsertAsync(agentGeneral);
            await _userRepository.InsertAsync(customer);

            Console.WriteLine("Users seeded (admin, 4 agents with routing profiles, customer).");
        }

        private async Task SeedKnowledgeBaseIfEmptyAsync()
        {
            if (await _knowledgeRepository.CountAllAsync() > 0)
                return;

            var articles = new List<KnowledgeArticle>
            {
                new()
                {
                    Title = "Reset password flow",
                    Summary =
                        "Direct customers to the Forgot Password link on the login page. Password reset links expire in 60 minutes.",
                    Body =
                        "1) Open login page 2) Click Forgot password 3) Enter registered email 4) Use link within 60 minutes. If email not received, check spam and verify address on file.",
                    Category = TicketCategory.AuthenticationIssue,
                    Keywords = new List<string> { "password", "reset", "login", "forgot" }
                },
                new()
                {
                    Title = "Two-factor authentication lockout",
                    Summary =
                        "If 2FA device is lost, verify identity and disable 2FA after security checks; send re-enrollment steps.",
                    Body =
                        "Verify account ownership via billing email or security questions. Disable 2FA from admin tools only after verification. Provide QR setup for re-enrollment.",
                    Category = TicketCategory.SecurityIssue,
                    Keywords = new List<string> { "2fa", "mfa", "locked", "authenticator" }
                },
                new()
                {
                    Title = "Duplicate subscription charge",
                    Summary =
                        "Check billing history for duplicate invoices; refund the duplicate within 5 business days if confirmed.",
                    Body =
                        "Look up transaction IDs. If two captures exist for same period, initiate refund for duplicate. Document ticket with invoice numbers.",
                    Category = TicketCategory.BillingIssue,
                    Keywords = new List<string> { "charge", "double", "invoice", "refund", "subscription" }
                },
                new()
                {
                    Title = "Plan downgrade / cancellation",
                    Summary =
                        "Cancellations take effect at end of billing period unless customer chooses immediate downgrade with proration rules.",
                    Body =
                        "Explain effective dates and data retention. Link to account billing settings for self-service when possible.",
                    Category = TicketCategory.BillingIssue,
                    Keywords = new List<string> { "cancel", "downgrade", "plan" }
                },
                new()
                {
                    Title = "Mobile app crash on launch",
                    Summary =
                        "Collect OS version, app version, and crash time. Recommend reinstall and clearing cache before escalation.",
                    Body =
                        "Ask for device model and OS. Steps: update app, restart device, reinstall. If persists, capture logs with diagnostic build.",
                    Category = TicketCategory.TechnicalIssue,
                    Keywords = new List<string> { "crash", "app", "mobile", "launch" }
                },
                new()
                {
                    Title = "Slow dashboard loading",
                    Summary =
                        "Performance issues often relate to large datasets or browser extensions. Suggest hard refresh and network check.",
                    Body =
                        "Verify API latency in status page. Ask user to disable extensions, try incognito, and test second network.",
                    Category = TicketCategory.PerformanceIssue,
                    Keywords = new List<string> { "slow", "latency", "timeout", "performance" }
                },
                new()
                {
                    Title = "Email notifications not received",
                    Summary =
                        "Confirm email on file, spam folder, and notification preferences. Resend test notification after preference fix.",
                    Body =
                        "Check bounce logs if available. Whitelist sender domain. Toggle notification settings off/on to refresh subscription.",
                    Category = TicketCategory.NotificationIssue,
                    Keywords = new List<string> { "email", "notification", "spam" }
                },
                new()
                {
                    Title = "Export personal data (GDPR-style)",
                    Summary =
                        "Data export requests are fulfilled within 30 days; verify identity before generating export package.",
                    Body =
                        "Use the privacy/export workflow. Send secure download link with expiry. Log completion on ticket.",
                    Category = TicketCategory.AccountManagement,
                    Keywords = new List<string> { "export", "privacy", "data", "gdpr" }
                },
                new()
                {
                    Title = "General support tone and escalation",
                    Summary =
                        "Acknowledge impact, set expectations on response time, and escalate to L2 when SLA is at risk.",
                    Body =
                        "Thank the customer, summarize the issue, next steps, and timeline. Escalate if production outage keywords appear.",
                    Category = TicketCategory.GeneralInquiry,
                    Keywords = new List<string> { "help", "support", "escalation" }
                }
            };

            await _knowledgeRepository.InsertManyAsync(articles);
            Console.WriteLine($"Knowledge base seeded ({articles.Count} articles).");
        }

        private async Task SeedSampleTicketsIfEmptyAsync()
        {
            if (await _ticketRepository.CountAllAsync() > 0)
                return;

            var customer = await _userRepository.GetByEmailAsync("customer@ticketsys.com");
            var agentAuth = await _userRepository.GetByEmailAsync("agent.auth@ticketsys.com");
            var agentBilling = await _userRepository.GetByEmailAsync("agent.billing@ticketsys.com");

            if (customer == null || agentAuth == null || agentBilling == null)
            {
                Console.WriteLine("⚠️ Skipping sample tickets: expected seed users not found.");
                return;
            }

            var tickets = new[]
            {
                new Ticket
                {
                    Title = "Cannot login to my account",
                    Description =
                        "I keep getting invalid password error even after resetting it via email.",
                    Status = TicketStatus.InProgress,
                    Priority = TicketPriority.High,
                    Category = TicketCategory.AuthenticationIssue,
                    CreatedById = customer.Id!,
                    CreatedByName = customer.FullName,
                    AssignedToId = agentAuth.Id,
                    AssignedToName = agentAuth.FullName
                },
                new Ticket
                {
                    Title = "Billing charge issue",
                    Description = "I was charged twice for my subscription this month.",
                    Status = TicketStatus.InProgress,
                    Priority = TicketPriority.High,
                    Category = TicketCategory.BillingIssue,
                    CreatedById = customer.Id!,
                    CreatedByName = customer.FullName,
                    AssignedToId = agentBilling.Id,
                    AssignedToName = agentBilling.FullName
                },
                new Ticket
                {
                    Title = "App crashes on startup",
                    Description = "The mobile app crashes immediately when I open it on Android 14.",
                    Status = TicketStatus.Open,
                    Priority = TicketPriority.Medium,
                    Category = TicketCategory.TechnicalIssue,
                    CreatedById = customer.Id!,
                    CreatedByName = customer.FullName
                }
            };

            await _ticketRepository.InsertManyAsync(tickets);
            Console.WriteLine($"Sample tickets seeded ({tickets.Length}).");
            PrintLoginHints();
        }

        private static void PrintLoginHints()
        {
            Console.WriteLine("   ── Test logins ──");
            Console.WriteLine("   Admin:     admin@ticketsys.com / Admin@123");
            Console.WriteLine("   Auth mgr:  agent.auth@ticketsys.com / Agent@123");
            Console.WriteLine("   Billing:   agent.billing@ticketsys.com / Agent@123");
            Console.WriteLine("   Technical: agent.tech@ticketsys.com / Agent@123");
            Console.WriteLine("   General:   agent@ticketsys.com / Agent@123");
            Console.WriteLine("   Customer:  customer@ticketsys.com / Customer@123");
        }
    }
}
