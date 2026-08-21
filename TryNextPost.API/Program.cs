using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TryNextPost.API.Middlewares;
using TryNextPost.Application.Common;
using TryNextPost.Application.Common.Settings;
using TryNextPost.Application.IServices;
using TryNextPost.Application.IServices.Class;
using TryNextPost.Application.IServices.Class.Admin;
using TryNextPost.Application.IServices.Class.Billing;
using TryNextPost.Application.IServices.Class.Dashboard;
using TryNextPost.Application.IServices.Class.Default;
using TryNextPost.Application.IServices.Class.Ndr;
using TryNextPost.Application.IServices.Class.Order;
using TryNextPost.Application.IServices.Class.RateCard;
using TryNextPost.Application.IServices.Class.Report;
using TryNextPost.Application.IServices.Class.SellerKYC;
using TryNextPost.Application.IServices.Class.Settlement;
using TryNextPost.Application.IServices.Class.Shipment;
using TryNextPost.Application.IServices.Class.Wallet;
using TryNextPost.Application.IServices.Class.Weight;
using TryNextPost.Application.IServices.Interface;
using TryNextPost.Application.IServices.Interface.Courier;
using TryNextPost.Application.IServices.Interface.Default;
using TryNextPost.Application.IServices.Interface.IEmployee;
using TryNextPost.Application.IServices.Interface.IAdmin;
using TryNextPost.Application.IServices.Interface.IBilling;
using TryNextPost.Application.IServices.Interface.IDashboard;
using TryNextPost.Application.IServices.Interface.IEmployee;
using TryNextPost.Application.IServices.Interface.INdr;
using TryNextPost.Application.IServices.Interface.IOrder;
using TryNextPost.Application.IServices.Interface.IPayment;
using TryNextPost.Application.IServices.Interface.IPayment;
using TryNextPost.Application.IServices.Interface.IRateCard;
using TryNextPost.Application.IServices.Interface.IReport;
using TryNextPost.Application.IServices.Interface.ISettlement;
using TryNextPost.Application.IServices.Interface.IShipment;
using TryNextPost.Application.IServices.Interface.IWallet;
using TryNextPost.Application.Services.Interface;
using TryNextPost.Application.Validators.Order;
using TryNextPost.Domain.Common;
using TryNextPost.Domain.IRepository;
using TryNextPost.Domain.IRepository.Report;
using TryNextPost.Infrastructure.AppDbContexts;
using TryNextPost.Infrastructure.CourierAdapters;
using TryNextPost.Infrastructure.CourierAdapters;
using TryNextPost.Infrastructure.DI;
using TryNextPost.Infrastructure.Identity;
using TryNextPost.Infrastructure.Repository;
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


builder.Services.Configure<SmsSettings>(
builder.Configuration.GetSection("SmsSettings"));
builder.Services.AddHttpClient<ISmsService, SmsService>();

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
builder.Services.AddScoped<INdrRepository, NdrRepository>();
builder.Services.AddScoped<IRtoRepository, RtoRepository>();
builder.Services.AddScoped<ICourierRepository, CourierRepository>();
builder.Services.AddScoped<IZoneRepository, ZoneRepository>();
builder.Services.AddScoped<ICourierRateCardRepository, CourierRateCardRepository>();
builder.Services.AddScoped<IShipmentChargesRepository, ShipmentChargesRepository>();
builder.Services.AddScoped<ICourierSettlementRepository, CourierSettlementRepository>();
builder.Services.AddScoped<IRateCalculationService, RateCalculationService>();
builder.Services.AddScoped<ICourierSettlementService, CourierSettlementService>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<IWalletRechargeRepository, WalletRechargeRepository>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<ICODSettlementRepository, CODSettlementRepository>();
builder.Services.AddScoped<ISellerBankAccountRepository, SellerBankAccountRepository>();
builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
builder.Services.AddScoped<ITdsCertificateRepository, TdsCertificateRepository>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<ITdsCertificateService, TdsCertificateService>();
builder.Services.AddScoped<ICourierAdminService, CourierAdminService>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.Configure<RazorpaySettings>(
builder.Configuration.GetSection(RazorpaySettings.SectionName));
builder.Services.AddHttpClient<IRazorpayPaymentGateway, RazorpayPaymentGateway>();

builder.Services.AddScoped<IShipmentService, ShipmentService>();
builder.Services.AddScoped<INdrService, NdrService>();
builder.Services.AddScoped<IWeightDiscrepancyRepository, WeightDiscrepancyRepository>();
builder.Services.AddScoped<IProductWeightFreezeRepository, ProductWeightFreezeRepository>();
builder.Services.AddScoped<IWeightDiscrepancyService, WeightDiscrepancyService>();
builder.Services.AddScoped<IWeightFreezeService, WeightFreezeService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<ISellerKycRepository, SellerKycRepostiory>();
builder.Services.AddScoped<ISellerKycServices, SellerKycServices>();
builder.Services.AddScoped<IExportHistoryRepository, ExportHistoryRepository>();
builder.Services.AddScoped<ICustomReportService, CustomReportService>();

// ✅ FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateForwardOrderRequestValidator>();

builder.Services.AddScoped<IOtpRepository, OtpRepository>();

// ✅ Courier Aggregator (IMPORTANT)
builder.Services.Configure<CourierSettings>(
    builder.Configuration.GetSection(CourierSettings.SectionName));

builder.Services.AddHttpClient();
builder.Services.AddHttpClient(nameof(DelhiveryAdapter));


builder.Services.AddScoped<ICourierAdapterFactory, CourierAdapterFactory>();
builder.Services.AddScoped<ICourierAdapter, AmazonShippingAdapter>();
builder.Services.Configure<AmazonShippingSettings>(builder.Configuration.GetSection("AmazonShipping"));
builder.Services.AddHttpClient<IAmazonAuthService, AmazonAuthService>();
builder.Services.AddHttpClient<IAmazonShippingService, AmazonShippingService>();
builder.Services.AddScoped<ICreditNoteRepository, CreditNoteRepository>();
builder.Services.AddScoped<ICreditNoteService, CreditNoteService>();
builder.Services.AddScoped<ICodSettlementService, CodSettlementService>();
builder.Services.AddHttpClient<IPincodeService, PincodeService>();
builder.Services.AddInfrastructure();
//builder.Services.AddScoped<XpressbeesAdapter>();
builder.Services.AddScoped<ICourierPickupLocationService, CourierPickupLocationService>();
builder.Services.AddScoped<ICourierPickupLocationRepository, CourierPickupLocationRepository>();

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

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var sid = context.Principal?.FindFirst("sid")?.Value;
            if (string.IsNullOrEmpty(sid) || !int.TryParse(sid, out var sessionId))
                return;

            var sessionRepository = context.HttpContext.RequestServices
                .GetRequiredService<IUserSessionRepository>();

            var session = await sessionRepository.GetByIdAsync(sessionId);
            if (session == null || !session.IsActive || session.ExpiryAt < DateTime.UtcNow)
            {
                context.Fail(SystemMessage.SessionRevoked);
            }
        }
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
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://147.93.31.120:8081",
                "http://147.93.31.120",
             "http://147.93.31.120:8082"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });

    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
#endregion

builder.Services.AddHttpClient<ISurepassService, SurepassService>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["Surepass:BaseUrl"]!);
});
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
//#region Seeder (background — do not block Swagger / Kestrel startup)

app.Lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        try
        {
            using var scope = app.Services.CreateScope();
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

            var rateCardLogger = services.GetRequiredService<ILoggerFactory>()
                .CreateLogger("RateCardSeeder");
            try
            {
                await RateCardSeeder.SeedAsync(db, rateCardLogger);
            }
            catch (Exception ex)
            {
                rateCardLogger.LogWarning(ex,
                    "Rate card seed skipped. Apply migration AddRateCardAndSettlement if missing.");
            }

            var weightLogger = services.GetRequiredService<ILoggerFactory>()
                .CreateLogger("WeightSeeder");
            try
            {
                await WeightSeeder.SeedAsync(db, weightLogger);
            }
            catch (Exception ex)
            {
                weightLogger.LogWarning(ex,
                    "Weight seed skipped. Apply migration AddWeightManagement if missing.");
            }
        }
        catch (Exception ex)
        {
            var startupLogger = app.Services.GetRequiredService<ILoggerFactory>()
                .CreateLogger("StartupSeeder");
            startupLogger.LogError(ex, "Background startup seeding failed.");
        }
    });
});

#endregion

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseMiddleware<ExceptionMiddleware>();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
