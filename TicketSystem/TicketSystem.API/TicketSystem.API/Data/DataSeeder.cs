using MongoDB.Driver;
using TicketSystem.API.Models;
using TicketSystem.API.Settings;

namespace TicketSystem.API.Data
{
    public class DataSeeder
    {
        private readonly IMongoCollection<User> _users;
        private readonly IMongoCollection<Ticket> _tickets;

        public DataSeeder(MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);
            _users = database.GetCollection<User>(settings.UsersCollection);
            _tickets = database.GetCollection<Ticket>(settings.TicketsCollection);
        }

        public async Task SeedAsync()
        {
            if (await _users.CountDocumentsAsync(_ => true) > 0) return;

            // Seed Admin
            var admin = new User
            {
                FullName = "System Admin",
                Email = "admin@ticketsys.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin
            };

            // Seed Agent
            var agent = new User
            {
                FullName = "Support Agent",
                Email = "agent@ticketsys.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Agent@123"),
                Role = UserRole.Agent
            };

            // Seed Customer
            var customer = new User
            {
                FullName = "John Customer",
                Email = "customer@ticketsys.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Customer@123"),
                Role = UserRole.Customer
            };

            await _users.InsertManyAsync(new[] { admin, agent, customer });

            // Seed sample tickets
            var tickets = new[]
            {
                new Ticket
                {
                    Title = "Cannot login to my account",
                    Description = "I keep getting invalid password error even after resetting it.",
                    Status = TicketStatus.Open,
                    Priority = TicketPriority.High,
                    Category = TicketCategory.AuthenticationIssue,
                    CreatedById = customer.Id!,
                    CreatedByName = customer.FullName
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
                    AssignedToId = agent.Id,
                    AssignedToName = agent.FullName
                },
                new Ticket
                {
                    Title = "App crashes on startup",
                    Description = "The mobile app crashes immediately when I open it.",
                    Status = TicketStatus.Open,
                    Priority = TicketPriority.Medium,
                    Category = TicketCategory.TechnicalIssue,
                    CreatedById = customer.Id!,
                    CreatedByName = customer.FullName
                }
            };

            await _tickets.InsertManyAsync(tickets);
            Console.WriteLine("✅ Database seeded successfully.");
            Console.WriteLine("   Admin:    admin@ticketsys.com / Admin@123");
            Console.WriteLine("   Agent:    agent@ticketsys.com / Agent@123");
            Console.WriteLine("   Customer: customer@ticketsys.com / Customer@123");
        }
    }
}