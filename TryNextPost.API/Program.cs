using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TryNextPost.API.Middlewares;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Application.IServices;
using TryNextPost.Application.IServices.Class;
using TryNextPost.Application.IServices.Class.Default;
using TryNextPost.Application.IServices.Class.Order;
using TryNextPost.Application.IServices.Class.Shipment;
using TryNextPost.Application.IServices.Class.Wallet;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.IServices.Interface.Default;
using TryNextPost.Application.IServices.Interface.IOrder;
using TryNextPost.Application.IServices.Interface.IShipment;
using TryNextPost.Application.IServices.Interface.IEmployee;
using TryNextPost.Application.IServices.Interface.IWallet;
using TryNextPost.Application.IServices.Interface.IPayment;
using TryNextPost.Application.Services.Interface;
using TryNextPost.Application.Validators.Order;
using TryNextPost.Domain.IRepository;
using TryNextPost.Infrastructure.AppDbContexts;
using TryNextPost.Infrastructure.Identity;
using TryNextPost.Infrastructure.Repository;
using TryNextPost.Application.IServices.Interface.Courier;
using TryNextPost.Infrastructure.CourierAdapters;
using TryNextPost.Infrastructure.Seeder;
using TryNextPost.Infrastructure.Service;

var builder = WebApplication.CreateBuilder(args);

#region Config
builder.Services.AddDbContext<AppDbContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("Con")));
#endregion

#region Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();
#endregion

#region DI

builder.Services.AddScoped<IIdentityService, IdentityService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
builder.Services.AddScoped<ISellerRepository, SellerRepository>();
builder.Services.AddScoped<ISellerEmployeeRepository, SellerEmployeeRepository>();
builder.Services.AddScoped<ISellerContextService, SellerContextService>();
builder.Services.AddScoped<IEmployeeService, TryNextPost.Application.IServices.Class.Employee.EmployeeService>();

// ✅ FINAL SMS CONFIG (BEST VERSION)
builder.Services.Configure<SmsSettings>(
    builder.Configuration.GetSection("SmsSettings"));
builder.Services.AddHttpClient<ISmsService, SmsService>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<ICourierRepository, CourierRepository>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IWalletRechargeRepository, WalletRechargeRepository>();
builder.Services.AddScoped<IWalletService, WalletService>();

builder.Services.Configure<RazorpaySettings>(
    builder.Configuration.GetSection(RazorpaySettings.SectionName));
builder.Services.AddHttpClient<IRazorpayPaymentGateway, RazorpayPaymentGateway>();

builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<ISellerKycRepository, SellerKycRepostiory>();
builder.Services.AddScoped<ISellerKycServices, SellerKycServices>();

// ✅ FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateForwardOrderRequestValidator>();

builder.Services.AddScoped<IOtpRepository, OtpRepository>();

// ✅ Courier Aggregator (IMPORTANT)
builder.Services.Configure<CourierSettings>(
    builder.Configuration.GetSection(CourierSettings.SectionName));

builder.Services.AddHttpClient();
builder.Services.AddHttpClient(nameof(DelhiveryAdapter));

builder.Services.AddScoped<ICourierAdapter, DelhiveryAdapter>();
builder.Services.AddScoped<ICourierAdapter, BlueDartAdapter>();
builder.Services.AddScoped<ICourierAdapter, XpressbeesAdapter>();
builder.Services.AddScoped<ICourierAdapter, DtdcAdapter>();
builder.Services.AddScoped<ICourierAdapter, EkartAdapter>();
builder.Services.AddScoped<ICourierAdapter, IndiaPostAdapter>();
builder.Services.AddScoped<ICourierAdapter, ShadowfaxAdapter>();
builder.Services.AddScoped<ICourierAdapterFactory, CourierAdapterFactory>();

#endregion

#region JWT

var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("JWT Key is missing in configuration");
}

var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
    };
});

#endregion

#region Authorization

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SellerAccess", policy =>
        policy.RequireRole("Seller", "SellerEmployee", "SuperAdmin"));

    options.AddPolicy("AdminAccess", policy =>
        policy.RequireRole("Admin", "SuperAdmin"));
});

#endregion

#region Swagger

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer",
        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Enter: Bearer {your_token}"
        });

    options.AddSecurityRequirement(
        new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        });
});

#endregion

#region CORS

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

#endregion

builder.Services.AddMemoryCache();
builder.Services.AddControllers();

var app = builder.Build();

#region Seeder

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

    await IdentitySeeder.SeedAsync(userManager, roleManager);

    var db = services.GetRequiredService<AppDbContext>();
    await PermissionSeeder.SeedAsync(db);

    var logger = services.GetRequiredService<ILoggerFactory>()
                         .CreateLogger("CourierSeeder");

    try
    {
        await CourierSeeder.SeedAsync(db, logger);
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex,
            "Courier seed skipped. Apply migration AddCourierCode if missing.");
    }
}

#endregion

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json",
            "TryNextPost API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();