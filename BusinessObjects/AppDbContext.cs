using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BusinessObjects
{
    public class AppDbContext : IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentReview> DocumentReviews { get; set; }
        public DbSet<DocumentTag> DocumentTags { get; set; }
        public DbSet<Major> Majors { get; set; }
        public DbSet<PointTransaction> PointTransactions { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<University> Universities { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<UserFollower> UserFollowers { get; set; }
        public DbSet<RecentViewed> RecentVieweds { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Users>()
                .HasOne(u => u.University)
                .WithMany()
                .HasForeignKey(u => u.UniversityId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.Course)
                .WithMany(c => c.Documents)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Document>()
                .HasOne(d => d.Uploader)              
                .WithMany(u => u.UploadedDocuments)   
                .HasForeignKey(d => d.UploaderId)
                .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.Document)
                .WithMany()
                .HasForeignKey(uf => uf.DocumentId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Major>()
                .HasIndex(m => new { m.Code, m.UniversityId })
                .IsUnique();

            modelBuilder.Entity<Course>()
                .HasIndex(c => new { c.CourseCode, c.MajorId })
                .IsUnique();

            modelBuilder.Entity<SubscriptionPlan>()
                .HasIndex(p => p.Name)
                .IsUnique();

            // Use static CreatedAt values (not DateTime.UtcNow)
            modelBuilder.Entity<SubscriptionPlan>().HasData(
                new SubscriptionPlan
                {
                    PlanId = 1,
                    Name = "FREE",
                    DisplayName = "Free Plan",
                    Price = 0.00m,
                    Currency = "USD",
                    DurationMonths = 12,
                    MaxDownloadsPerMonth = 10,
                    PremiumContentAccess = false,
                    PrioritySupport = false,
                    Description = "Basic access with limited downloads",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
                },
                new SubscriptionPlan
                {
                    PlanId = 2,
                    Name = "PREMIUM",
                    DisplayName = "Premium Plan",
                    Price = 200m,
                    Currency = "USD",
                    DurationMonths = 1,
                    MaxDownloadsPerMonth = null, // NULL = unlimited
                    PremiumContentAccess = true,
                    PrioritySupport = true,
                    Description = "Unlimited access with AI features",
                    IsActive = true,
                    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            modelBuilder.Entity<DocumentTag>()
                .HasIndex(t => t.Name)
                .IsUnique();

            modelBuilder.Entity<DocumentTag>().HasData(
                new DocumentTag
                {
                    TagId = 1,
                    Name = "Lecture Notes",
                    Color = "#3B82F6",
                    Description = "Class lecture notes",
                    IsSystemTag = true,
                    UsageCount = 0,
                    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
                },
                new DocumentTag
                {
                    TagId = 2,
                    Name = "Study Guide",
                    Color = "#10B981",
                    Description = "Study guides and summaries",
                    IsSystemTag = true,
                    UsageCount = 0,
                    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
                },
                new DocumentTag
                {
                    TagId = 3,
                    Name = "Past Exam",
                    Color = "#F59E0B",
                    Description = "Previous examination papers",
                    IsSystemTag = true,
                    UsageCount = 0,
                    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
                },
                new DocumentTag
                {
                    TagId = 4,
                    Name = "Assignment",
                    Color = "#EF4444",
                    Description = "Homework and assignments",
                    IsSystemTag = true,
                    UsageCount = 0,
                    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
                },
                new DocumentTag
                {
                    TagId = 5,
                    Name = "Quiz",
                    Color = "#8B5CF6",
                    Description = "Quizzes and short tests",
                    IsSystemTag = true,
                    UsageCount = 0,
                    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
                },
                new DocumentTag
                {
                    TagId = 6,
                    Name = "Research Paper",
                    Color = "#6B7280",
                    Description = "Academic research papers",
                    IsSystemTag = true,
                    UsageCount = 0,
                    CreatedAt = new DateTime(2025, 01, 01, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            modelBuilder.Entity<UserFavorite>()
                .HasKey(uf => new { uf.UserId, uf.DocumentId });

            modelBuilder.Entity<UserFollower>()
            .HasKey(uf => new { uf.FollowerId, uf.FollowingId });

            modelBuilder.Entity<UserFollower>()
                .HasOne(uf => uf.Follower)
                .WithMany()
                .HasForeignKey(uf => uf.FollowerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UserFollower>()
                .HasOne(uf => uf.Following)
                .WithMany()
                .HasForeignKey(uf => uf.FollowingId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
