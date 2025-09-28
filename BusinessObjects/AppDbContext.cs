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

        public DbSet<DocumentLike> DocumentLikes { get; set; }

        public DbSet<Major> Majors { get; set; }
        public DbSet<PointTransaction> PointTransactions { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<University> Universities { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<UserFollower> UserFollowers { get; set; }
        public DbSet<RecentViewed> RecentVieweds { get; set; }
        public DbSet<PaymentRecord> PaymentRecords { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =============== USER RELATIONSHIPS ===============
            modelBuilder.Entity<Users>()
                .HasOne(u => u.University)
                .WithMany()
                .HasForeignKey(u => u.UniversityId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Users>()
                .HasOne(u => u.Major)
                .WithMany()
                .HasForeignKey(u => u.MajorId)
                .OnDelete(DeleteBehavior.SetNull); // SetNull since MajorId is nullable

            modelBuilder.Entity<Users>()
                .HasOne(u => u.SubscriptionPlan)
                .WithMany()
                .HasForeignKey(u => u.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.SetNull); // SetNull since SubscriptionPlanId is nullable

            // =============== DOCUMENT RELATIONSHIPS ===============
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

            modelBuilder.Entity<Document>()
                .HasOne(d => d.ApprovedByUser)
                .WithMany()
                .HasForeignKey(d => d.ApprovedBy)
                .OnDelete(DeleteBehavior.SetNull); // SetNull since ApprovedBy is nullable

            // Document <-> DocumentTag many-to-many relationship
            modelBuilder.Entity<Document>()
                .HasMany(d => d.Tags)
                .WithMany()
                .UsingEntity(j => j.ToTable("DocumentDocumentTags"));

            modelBuilder.Entity<DocumentLike>()
                .HasOne(d => d.Document)
                .WithOne()
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<DocumentLike>()
                .HasOne(d => d.User)
                .WithOne()
                .OnDelete(DeleteBehavior.NoAction);

            // =============== ACADEMIC STRUCTURE RELATIONSHIPS ===============
            modelBuilder.Entity<Major>()
                .HasOne(m => m.University)
                .WithMany()
                .HasForeignKey(m => m.UniversityId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Major)
                .WithMany()
                .HasForeignKey(c => c.MajorId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<University>()
                .HasOne(u => u.Country)
                .WithMany()
                .HasForeignKey(u => u.CountryId)
                .OnDelete(DeleteBehavior.NoAction);

            // =============== REVIEW AND TRANSACTION RELATIONSHIPS ===============
            modelBuilder.Entity<DocumentReview>()
                .HasOne(dr => dr.Document)
                .WithMany()
                .HasForeignKey(dr => dr.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DocumentReview>()
                .HasOne(dr => dr.Reviewer)
                .WithMany()
                .HasForeignKey(dr => dr.ReviewerId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<PointTransaction>()
                .HasOne(pt => pt.User)
                .WithMany()
                .HasForeignKey(pt => pt.UserId)
                .OnDelete(DeleteBehavior.Cascade);


            // =============== USER ACTIVITY RELATIONSHIPS ===============
            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.User)
                .WithMany()
                .HasForeignKey(uf => uf.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserFavorite>()
                .HasOne(uf => uf.Document)
                .WithMany()
                .HasForeignKey(uf => uf.DocumentId)
                .OnDelete(DeleteBehavior.NoAction);

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

            modelBuilder.Entity<RecentViewed>()
                .HasOne(rv => rv.Document)
                .WithMany()
                .HasForeignKey(rv => rv.DocumentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecentViewed>()
                .HasOne(rv => rv.Course)
                .WithMany()
                .HasForeignKey(rv => rv.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RecentViewed>()
                .HasOne(rv => rv.User)
                .WithMany()
                .HasForeignKey(rv => rv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PaymentRecord>()
                .HasOne(rv => rv.User)
                .WithMany()
                .HasForeignKey(rv => rv.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // =============== INDEXES ===============
            modelBuilder.Entity<Major>()
                .HasIndex(m => new { m.Code, m.UniversityId })
                .IsUnique();

            modelBuilder.Entity<Course>()
                .HasIndex(c => new { c.CourseCode, c.MajorId })
                .IsUnique();

            modelBuilder.Entity<SubscriptionPlan>()
                .HasIndex(p => p.Name)
                .IsUnique();

            modelBuilder.Entity<DocumentTag>()
                .HasIndex(t => t.Name)
                .IsUnique();

            modelBuilder.Entity<UserFavorite>()
                .HasIndex(uf => new { uf.UserId, uf.DocumentId })
                .IsUnique();

            modelBuilder.Entity<UserFollower>()
                .HasIndex(uf => new { uf.FollowerId, uf.FollowingId })
                .IsUnique();

            // =============== DATA SEEDING ===============
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
        }
    }
}
