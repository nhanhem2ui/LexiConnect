using BusinessObjects;
using DataAccess;
using LexiConnect.Libraries;
using LexiConnect.Models;
using LexiConnect.Services.Firebase;
using LexiConnect.Services.Gemini;
using LexiConnect.Services.VnPay;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Repositories;
using Services;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//DAO
builder.Services.AddScoped<IGenericDAO<Country>, CountryDAO>();
builder.Services.AddScoped<IGenericDAO<Course>, CourseDAO>();
builder.Services.AddScoped<IGenericDAO<Document>, DocumentDAO>();
builder.Services.AddScoped<IGenericDAO<DocumentReview>, DocumentReviewDAO>();
builder.Services.AddScoped<IGenericDAO<DocumentTag>, DocumentTagDAO>();
builder.Services.AddScoped<IGenericDAO<DocumentLike>, DocumentLikeDAO>();
builder.Services.AddScoped<IGenericDAO<Major>, MajorDAO>();
builder.Services.AddScoped<IGenericDAO<PointTransaction>, PointTransactionDAO>();
builder.Services.AddScoped<IGenericDAO<SubscriptionPlan>, SubscriptionPlanDAO>();
builder.Services.AddScoped<IGenericDAO<University>, UniversityDAO>();
builder.Services.AddScoped<IGenericDAO<UserFavorite>, UserFavoriteDAO>();
builder.Services.AddScoped<IGenericDAO<UserFollower>, UserFollowerDAO>();
builder.Services.AddScoped<IGenericDAO<RecentViewed>, RecentViewedDAO>();
builder.Services.AddScoped<IGenericDAO<PaymentRecord>, PaymentRecordDAO>();
builder.Services.AddScoped<IGenericDAO<Users>, UserDAO>();
builder.Services.AddScoped<IGenericDAO<UserFollowCourse>, UserFollowCourseDAO>();
builder.Services.AddScoped<IGenericDAO<Chat>, ChatDAO>();

//Repository
builder.Services.AddScoped<IGenericRepository<Country>, CountryRepository>();
builder.Services.AddScoped<IGenericRepository<Course>, CourseRepository>();
builder.Services.AddScoped<IGenericRepository<Document>, DocumentRepository>();
builder.Services.AddScoped<IGenericRepository<DocumentReview>, DocumentReviewRepository>();
builder.Services.AddScoped<IGenericRepository<DocumentTag>, DocumentTagRepository>();
builder.Services.AddScoped<IGenericRepository<DocumentLike>, DocumentLikeRepository>();
builder.Services.AddScoped<IGenericRepository<Major>, MajorRepository>();
builder.Services.AddScoped<IGenericRepository<PointTransaction>, PointTransactionRepository>();
builder.Services.AddScoped<IGenericRepository<SubscriptionPlan>, SubscriptionPlanRepository>();
builder.Services.AddScoped<IGenericRepository<University>, UniversityRepository>();
builder.Services.AddScoped<IGenericRepository<UserFavorite>, UserFavoriteRepository>();
builder.Services.AddScoped<IGenericRepository<UserFollower>, UserFollowerRepository>();
builder.Services.AddScoped<IGenericRepository<RecentViewed>, RecentViewedRepository>();
builder.Services.AddScoped<IGenericRepository<PaymentRecord>, PaymentRecordRepository>();
builder.Services.AddScoped<IGenericRepository<Users>, UsersRepository>();
builder.Services.AddScoped<IGenericRepository<UserFollowCourse>, UserFollowCourseRepository>();
builder.Services.AddScoped<IGenericRepository<Chat>, ChatRepository>();

//Service
builder.Services.AddScoped<IGenericService<Country>, CountryService>();
builder.Services.AddScoped<IGenericService<Course>, CourseService>();
builder.Services.AddScoped<IGenericService<Document>, DocumentService>();
builder.Services.AddScoped<IGenericService<DocumentReview>, DocumentReviewService>();
builder.Services.AddScoped<IGenericService<DocumentTag>, DocumentTagService>();
builder.Services.AddScoped<IGenericService<DocumentLike>, DocumentLikeService>();
builder.Services.AddScoped<IGenericService<Major>, MajorService>();
builder.Services.AddScoped<IGenericService<PointTransaction>, PointTransactionService>();
builder.Services.AddScoped<IGenericService<SubscriptionPlan>, SubscriptionPlanService>();
builder.Services.AddScoped<IGenericService<University>, UniversityService>();
builder.Services.AddScoped<IGenericService<UserFavorite>, UserFavoriteService>();
builder.Services.AddScoped<IGenericService<UserFollower>, UserFollowerService>();
builder.Services.AddScoped<IGenericService<RecentViewed>, RecentViewedService>();
builder.Services.AddScoped<IGenericService<PaymentRecord>, PaymentRecordService>();
builder.Services.AddScoped<IGenericService<Users>, UserService>();
builder.Services.AddScoped<IGenericService<UserFollowCourse>, UserFollowCourseService>();
builder.Services.AddScoped<IGenericService<Chat>, ChatService>();

builder.Services.AddScoped<ISender, EmailSender>();

//External
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddSingleton<IFirebaseStorageService, FirebaseStorageService>();
builder.Services.AddHttpClient<IGeminiService, GeminiService>();

// Configure Identity
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.ClaimsIdentity.UserNameClaimType = ClaimTypes.Name;
    options.Password.RequiredLength = 6;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = true;
    options.SignIn.RequireConfirmedPhoneNumber = false;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
    opt.TokenLifespan = TimeSpan.FromHours(2));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Adjust timeout as needed
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Required for GDPR compliance
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.ExpireTimeSpan = TimeSpan.FromDays(2);
    options.SlidingExpiration = true;
    options.LoginPath = "/Auth/Signin";
    options.LogoutPath = "/Auth/Signout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
});

//SignalR
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
    options.HandshakeTimeout = TimeSpan.FromSeconds(15);
});

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    // Use cookie authentication by default (Identity)
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    options.CallbackPath = "/signin-google";
    options.SaveTokens = true;
});

// Configure form options for file uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

// Kestrel server limits
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
});

var app = builder.Build();

//// make ASP.NET respect X-Forwarded headers from proxy
//app.UseForwardedHeaders(new ForwardedHeadersOptions
//{
//    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
//});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseStatusCodePagesWithReExecute("/Home/NotFoundPage");

app.UseForwardedHeaders();
app.UseCookiePolicy();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Introduction}/{id?}");

app.MapHub<ChatHub>("/ChatHub");

app.Run();