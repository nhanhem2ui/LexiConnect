using BusinessObjects;
using DataAccess;
using LexiConnect.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repositories;

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
builder.Services.AddScoped<IGenericDAO<Major>, MajorDAO>();
builder.Services.AddScoped<IGenericDAO<PointTransaction>, PointTransactionDAO>();
builder.Services.AddScoped<IGenericDAO<SubscriptionPlan>, SubscriptionPlanDAO>();
builder.Services.AddScoped<IGenericDAO<University>, UniversityDAO>();
builder.Services.AddScoped<IGenericDAO<UserFavorite>, UserFavoriteDAO>();
builder.Services.AddScoped<IGenericDAO<UserFollower>, UserFollowerDAO>();
builder.Services.AddScoped<IGenericDAO<RecentViewed>, RecentViewedDAO>();
builder.Services.AddScoped<IGenericDAO<Users>, UserDAO>();

//Repository
builder.Services.AddScoped<IGenericRepository<Country>, CountryRepository>();
builder.Services.AddScoped<IGenericRepository<Course>, CourseRepository>();
builder.Services.AddScoped<IGenericRepository<Document>, DocumentRepository>();
builder.Services.AddScoped<IGenericRepository<DocumentReview>, DocumentReviewRepository>();
builder.Services.AddScoped<IGenericRepository<DocumentTag>, DocumentTagRepository>();
builder.Services.AddScoped<IGenericRepository<Major>, MajorRepository>();
builder.Services.AddScoped<IGenericRepository<PointTransaction>, PointTransactionRepository>();
builder.Services.AddScoped<IGenericRepository<SubscriptionPlan>, SubscriptionPlanRepository>();
builder.Services.AddScoped<IGenericRepository<University>, UniversityRepository>();
builder.Services.AddScoped<IGenericRepository<UserFavorite>, UserFavoriteRepository>();
builder.Services.AddScoped<IGenericRepository<UserFollower>, UserFollowerRepository>();
builder.Services.AddScoped<IGenericRepository<RecentViewed>, RecentViewedRepository>();
builder.Services.AddScoped<IGenericRepository<Users>, UsersRepository>();

builder.Services.AddScoped<AppDbContext>();
builder.Services.AddScoped<ISender, EmailSender>();

// Configure Identity
builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
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

builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(2);
    options.SlidingExpiration = true;
    options.LoginPath = "/Auth/Signin";
    options.LogoutPath = "/Auth/Signout";
    options.AccessDeniedPath = "/Auth/AccessDenied";
});

// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? "";
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
    options.CallbackPath = "/signin-google";
    options.SaveTokens = true;
});

static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
{
    string[] roleNames = { "Admin", "User", "Moderator" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await SeedRoles(roleManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Introduction}/{id?}");

app.Run();