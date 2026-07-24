using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TryNextPost.Domain.Entities;
using TryNextPost.Infrastructure.Identity;

namespace TryNextPost.Infrastructure.AppDbContexts
{
    public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        // 🔹 DbSets
        public DbSet<Address> Addresses => Set<Address>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<ReverseQcDetail> ReverseQcDetails => Set<ReverseQcDetail>();
        public DbSet<ReverseQcImage> ReverseQcImages=> Set<ReverseQcImage>();
        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<ShipmentTracking> ShipmentTrackings => Set<ShipmentTracking>();
        public DbSet<Courier> Couriers => Set<Courier>();
        public DbSet<NDR> NDRS => Set<NDR>();
        public DbSet<RTO> RTOS => Set<RTO>();
        public DbSet<CourierServiceability> CourierServiceabilities => Set<CourierServiceability>();
        public DbSet<Webhook> Webhooks => Set<Webhook>();
        public DbSet<Wallet> Wallets => Set<Wallet>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<WalletRecharge> WalletRecharges => Set<WalletRecharge>();
        public DbSet<CODSettlement> CODSettlements => Set<CODSettlement>();
        public DbSet<SellerBankAccount> SellerBankAccounts => Set<SellerBankAccount>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<Zone> Zones => Set<Zone>();
        public DbSet<PincodeZoneMapping> PincodeZoneMappings => Set<PincodeZoneMapping>();
        public DbSet<CourierRateCard> CourierRateCards => Set<CourierRateCard>();
        public DbSet<ShipmentCharges> ShipmentCharges => Set<ShipmentCharges>();
        public DbSet<CourierSettlement> CourierSettlements => Set<CourierSettlement>();
        public DbSet<CourierSettlementLine> CourierSettlementLines => Set<CourierSettlementLine>();
        public DbSet<WeightDiscrepancy> WeightDiscrepancies => Set<WeightDiscrepancy>();
        public DbSet<ProductWeightFreeze> ProductWeightFreezes => Set<ProductWeightFreeze>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<Seller> Sellers => Set<Seller>();
        public DbSet<SellerEmployee> SellerEmployees => Set<SellerEmployee>();
        public DbSet<EmployeePermission> EmployeePermissions => Set<EmployeePermission>();
        public DbSet<CompanyInfo> Companies => Set<CompanyInfo>();
        public DbSet<SellerKYC> SellerKYC { get; set; }
        public DbSet<SellerDocument> SellerDocument { get; set; }
        public DbSet<Otp> Otps { get; set; }

        public DbSet<UserSession> UserSessions => Set<UserSession>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // 🔥 USER → ADDRESS
            // =========================
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Addresses)
                .WithOne()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // 🔥 ORDER → ORDER ITEMS
            // =========================
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // ORDER → REVERSE QC DETAIL (one-to-one)
            // =========================
            modelBuilder.Entity<Order>()
                .HasOne(o => o.ReverseQcDetail)
                .WithOne(qc => qc.Order)
                .HasForeignKey<ReverseQcDetail>(qc => qc.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ek order ke liye sirf ek QC detail
            modelBuilder.Entity<ReverseQcDetail>()
                .HasIndex(qc => qc.OrderId)
                .IsUnique();

            // =========================
            // REVERSE QC DETAIL → IMAGES (one-to-many)
            // =========================
            modelBuilder.Entity<ReverseQcDetail>()
                .HasMany(qc => qc.Images)
                .WithOne(image => image.ReverseQcDetail)
                .HasForeignKey(image => image.ReverseQcDetailId)
                .OnDelete(DeleteBehavior.Cascade);

            // Same position par duplicate image order na ho
            modelBuilder.Entity<ReverseQcImage>()
                .HasIndex(image => new
                {
                    image.ReverseQcDetailId,
                    image.DisplayOrder
                })
                .IsUnique();

            // =========================
            // 🔥 ORDER → SELLER
            // =========================
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Seller)
                .WithMany()
                .HasForeignKey(o => o.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // 🔥 ORDER → PICKUP ADDRESS
            // =========================
            modelBuilder.Entity<Order>()
                .HasOne(o => o.PickupAddress)
                .WithMany()
                .HasForeignKey(o => o.PickupAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // 🔥 SHIPMENT → ORDER
            // =========================
            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Order)
                .WithMany()
                .HasForeignKey(s => s.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // 🔥 SHIPMENT → COURIER
            // =========================
            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.Courier)
                .WithMany(c => c.Shipments)
                .HasForeignKey(s => s.CourierId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // 🔥 SHIPMENT → PICKUP ADDRESS
            // =========================
            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.PickupAddress)
                .WithMany()
                .HasForeignKey(s => s.PickupAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // 🔥 SHIPMENT → TRACKING (one-to-MANY)
            // =========================
            modelBuilder.Entity<ShipmentTracking>()
                .HasOne(st => st.Shipment)
                .WithMany(s => s.TrackingHistory)
                .HasForeignKey(st => st.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // 🔥 SHIPMENT → NDR
            // =========================
            modelBuilder.Entity<NDR>()
                .HasOne(n => n.Shipment)
                .WithMany(s => s.NDRs)
                .HasForeignKey(n => n.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // 🔥 SHIPMENT → RTO
            // =========================
            modelBuilder.Entity<RTO>()
                .HasOne(r => r.Shipment)
                .WithMany(s => s.RTOs)
                .HasForeignKey(r => r.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // 🔥 COURIER → SERVICEABILITY
            // =========================
            modelBuilder.Entity<CourierServiceability>(entity =>
            {
                entity.HasIndex(cs => cs.Pincode);
                entity.HasIndex(cs => cs.CourierId);
                entity.HasIndex(cs => new { cs.CourierId, cs.Pincode }).IsUnique();

                entity.HasOne(cs => cs.Courier)
                      .WithMany(c => c.Serviceabilities)
                      .HasForeignKey(cs => cs.CourierId);
            });

            // =========================
            // 🔥 WALLET → TRANSACTIONS (one-to-MANY)
            // =========================
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<WalletRecharge>()
                .HasOne(r => r.Wallet)
                .WithMany(w => w.Recharges)
                .HasForeignKey(r => r.WalletId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Wallet>()
                .HasOne(w => w.Seller)
                .WithMany()
                .HasForeignKey(w => w.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SellerEmployee>()
                .HasOne(e => e.Seller)
                .WithMany()
                .HasForeignKey(e => e.SellerId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeePermission>()
                .HasOne(p => p.Employee)
                .WithMany(e => e.Permissions)
                .HasForeignKey(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EmployeePermission>()
                .HasIndex(p => new { p.EmployeeId, p.PermissionCode })
                .IsUnique();

            // =========================
            // 🔥 COD → SHIPMENT
            // =========================
            modelBuilder.Entity<CODSettlement>()
                .HasOne(c => c.Shipment)
                .WithMany()
                .HasForeignKey(c => c.ShipmentId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // 🔥 COD → SELLER
            // =========================
            modelBuilder.Entity<CODSettlement>()
                .HasOne<Seller>()
                .WithMany()
                .HasForeignKey(c => c.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // 🔥 SELLER → COMPANY
            // =========================
            modelBuilder.Entity<Seller>()
                .HasOne(s => s.Company)
                .WithMany(c => c.Sellers)
                .HasForeignKey(s => s.CompanyId).IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // 🔥 SELLER → APPLICATIONUSER (1:1)
            // =========================
            modelBuilder.Entity<Seller>()
                .HasOne<ApplicationUser>()
                .WithOne(u => u.Seller)                 // ✅ navigation explicitly specify karo
                .HasForeignKey<Seller>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // 🔥 ROLE-PERMISSION (surrogate Id as PK, composite as unique constraint)
            // =========================
            modelBuilder.Entity<RolePermission>()
                .HasOne<ApplicationRole>()
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolePermission>()
                .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                .IsUnique();

            //---------------UserSession------------------//

            modelBuilder.Entity<UserSession>()
           .HasOne<ApplicationUser>()
           .WithMany()
           .HasForeignKey(us => us.UserId)
           .OnDelete(DeleteBehavior.Cascade);

            //-----------//
            modelBuilder.Entity<Seller>()
            .HasOne(s => s.DefaultPickupAddress)
            .WithMany()
            .HasForeignKey(s => s.DefaultPickupAddressId)
            .OnDelete(DeleteBehavior.Restrict);


            // =========================
            // 🔥 INDEXES
            // =========================

            // --- Seller ---
            modelBuilder.Entity<Seller>().HasIndex(s => s.UserId).IsUnique();
            modelBuilder.Entity<Seller>().HasIndex(s => s.GstNumber).IsUnique();
            modelBuilder.Entity<Seller>().HasIndex(s => s.CompanyId);

            // --- Wallet ---
            modelBuilder.Entity<Wallet>().HasIndex(w => w.SellerId).IsUnique();
            modelBuilder.Entity<Wallet>().HasIndex(w => w.UserId);

            // --- SellerEmployee ---
            modelBuilder.Entity<SellerEmployee>().HasIndex(e => e.UserId).IsUnique();
            modelBuilder.Entity<SellerEmployee>().HasIndex(e => e.SellerId);
            modelBuilder.Entity<SellerEmployee>().HasIndex(e => e.Email);

            // --- Transaction ---
            modelBuilder.Entity<Transaction>().HasIndex(t => t.WalletId);
            modelBuilder.Entity<Transaction>().HasIndex(t => t.TxnReference).IsUnique();

            // --- WalletRecharge (Razorpay) ---
            modelBuilder.Entity<WalletRecharge>(entity =>
            {
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.Currency).HasMaxLength(10).IsRequired();
                entity.Property(x => x.GatewayOrderId).HasMaxLength(100).IsRequired();
                entity.Property(x => x.GatewayPaymentId).HasMaxLength(100);
                entity.Property(x => x.Receipt).HasMaxLength(100).IsRequired();
                entity.Property(x => x.Amount).HasPrecision(18, 2);
                entity.HasIndex(x => x.GatewayOrderId).IsUnique();
                entity.HasIndex(x => x.GatewayPaymentId)
                    .IsUnique()
                    .HasFilter("[GatewayPaymentId] IS NOT NULL");
                entity.HasIndex(x => x.WalletId);
                entity.HasIndex(x => x.UserId);
                entity.HasIndex(x => x.Status);
            });

            // --- CODSettlement ---
            modelBuilder.Entity<CODSettlement>().HasIndex(c => c.ShipmentId).IsUnique();
            modelBuilder.Entity<CODSettlement>().HasIndex(c => c.SellerId);

            // --- SellerBankAccount ---
            modelBuilder.Entity<SellerBankAccount>(entity =>
            {
                entity.HasOne(a => a.Seller)
                    .WithMany()
                    .HasForeignKey(a => a.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(a => a.SellerId);
                entity.Property(a => a.AccountHolderName).HasMaxLength(150);
                entity.Property(a => a.AccountNumber).HasMaxLength(50);
                entity.Property(a => a.IfscCode).HasMaxLength(20);
                entity.Property(a => a.BankName).HasMaxLength(150);
                entity.Property(a => a.BranchName).HasMaxLength(100);
                entity.Property(a => a.AccountType).HasMaxLength(30);
            });

            // --- Invoice ---
            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasOne(i => i.Seller)
                    .WithMany()
                    .HasForeignKey(i => i.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(i => i.SellerId);
                entity.HasIndex(i => i.InvoiceNumber).IsUnique();
                entity.Property(i => i.InvoiceNumber).HasMaxLength(50);
                entity.Property(i => i.ServiceType).HasMaxLength(50);
                entity.Property(i => i.Amount).HasPrecision(18, 2);
                entity.Property(i => i.ShippingChargesAmount).HasPrecision(18, 2);
                entity.Property(i => i.RechargeAmount).HasPrecision(18, 2);
            });

            // --- Order ---
            modelBuilder.Entity<Order>().HasIndex(o => o.OrderRef).IsUnique();
            modelBuilder.Entity<Order>().HasIndex(o => o.ShippingPincode);

            // --- Shipment ---
            modelBuilder.Entity<Shipment>(entity =>
            {
                entity.Property(x => x.AwbNumber).HasMaxLength(100);
                entity.Property(x => x.CourierReference).HasMaxLength(100);
                entity.Property(x => x.ServiceCode).HasMaxLength(100);
                entity.Property(x => x.ChargedAmount).HasPrecision(18, 2);
                entity.HasIndex(s => s.AwbNumber).IsUnique().HasFilter("[AwbNumber] IS NOT NULL");
                entity.HasIndex(s => s.DeliveryPincode);
            });

            // --- ShipmentTracking ---
            modelBuilder.Entity<ShipmentTracking>().HasIndex(st => st.ShipmentId);

            // --- Address ---
            modelBuilder.Entity<Address>().HasIndex(a => a.Pincode);

            // --- NDR ---
            modelBuilder.Entity<NDR>().HasIndex(n => n.Status);

            // --- Webhook ---
            modelBuilder.Entity<Webhook>().HasIndex(w => w.Status);

            // --- Permission ---
            modelBuilder.Entity<Permission>().HasIndex(p => p.Name).IsUnique();

            modelBuilder.Entity<CODSettlement>()
    .Property(x => x.CodAmount)
    .HasPrecision(18, 2);

            modelBuilder.Entity<CODSettlement>()
                .Property(x => x.CollectedAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Courier>(entity =>
            {
                entity.Property(x => x.MaxWeightLimit).HasPrecision(18, 2);
                entity.Property(x => x.CourierCode).HasMaxLength(50).IsRequired();
                entity.HasIndex(x => x.CourierCode).IsUnique();
            });

            modelBuilder.Entity<OrderItem>()
                .Property(x => x.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Shipment>()
                .Property(x => x.Breadth)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Shipment>()
                .Property(x => x.Height)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Shipment>()
                .Property(x => x.Length)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Shipment>()
                .Property(x => x.Weight)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(x => x.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Wallet>()
                .Property(x => x.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

            // =========================
            // ZONE + PINCODE MAPPING
            // =========================
            modelBuilder.Entity<Zone>(entity =>
            {
                entity.Property(x => x.ZoneCode).HasMaxLength(10).IsRequired();
                entity.Property(x => x.ZoneName).HasMaxLength(100).IsRequired();
                entity.HasIndex(x => x.ZoneCode).IsUnique();
            });

            modelBuilder.Entity<PincodeZoneMapping>(entity =>
            {
                entity.Property(x => x.PincodePrefix).HasMaxLength(2).IsRequired();
                entity.HasIndex(x => x.PincodePrefix).IsUnique();
                entity.HasOne(m => m.Zone)
                    .WithMany(z => z.PincodeMappings)
                    .HasForeignKey(m => m.ZoneId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // COURIER RATE CARD
            // =========================
            modelBuilder.Entity<CourierRateCard>(entity =>
            {
                entity.Property(x => x.WeightFromGrams).HasPrecision(18, 2);
                entity.Property(x => x.WeightToGrams).HasPrecision(18, 2);
                entity.Property(x => x.CourierCost).HasPrecision(18, 2);
                entity.Property(x => x.SellerCharge).HasPrecision(18, 2);
                entity.Property(x => x.ServiceCode).HasMaxLength(50).IsRequired();
                entity.HasIndex(x => new { x.CourierId, x.FromZoneId, x.ToZoneId, x.WeightFromGrams, x.WeightToGrams, x.ServiceCode });
                entity.HasOne(r => r.Courier)
                    .WithMany()
                    .HasForeignKey(r => r.CourierId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(r => r.FromZone)
                    .WithMany()
                    .HasForeignKey(r => r.FromZoneId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(r => r.ToZone)
                    .WithMany()
                    .HasForeignKey(r => r.ToZoneId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // SHIPMENT CHARGES
            // =========================
            modelBuilder.Entity<ShipmentCharges>(entity =>
            {
                entity.Property(x => x.SellerCharge).HasPrecision(18, 2);
                entity.Property(x => x.CourierCost).HasPrecision(18, 2);
                entity.Property(x => x.Margin).HasPrecision(18, 2);
                entity.Property(x => x.CodCharge).HasPrecision(18, 2);
                entity.Property(x => x.ChargeableWeightGrams).HasPrecision(18, 2);
                entity.HasIndex(x => x.ShipmentId).IsUnique();
                entity.HasOne(c => c.Shipment)
                    .WithMany()
                    .HasForeignKey(c => c.ShipmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // COURIER B2B SETTLEMENT
            // =========================
            modelBuilder.Entity<CourierSettlement>(entity =>
            {
                entity.Property(x => x.TotalCourierCost).HasPrecision(18, 2);
                entity.Property(x => x.TotalSellerCharge).HasPrecision(18, 2);
                entity.Property(x => x.TotalMargin).HasPrecision(18, 2);
                entity.Property(x => x.PaymentReference).HasMaxLength(200);
                entity.Property(x => x.Notes).HasMaxLength(500);
                entity.HasIndex(x => x.CourierId);
                entity.HasIndex(x => new { x.CourierId, x.PeriodFrom, x.PeriodTo });
                entity.HasOne(s => s.Courier)
                    .WithMany()
                    .HasForeignKey(s => s.CourierId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CourierSettlementLine>(entity =>
            {
                entity.Property(x => x.CourierCost).HasPrecision(18, 2);
                entity.Property(x => x.SellerCharge).HasPrecision(18, 2);
                entity.Property(x => x.Margin).HasPrecision(18, 2);
                entity.Property(x => x.AwbNumber).HasMaxLength(100);
                entity.HasIndex(x => new { x.CourierSettlementId, x.ShipmentId }).IsUnique();
                entity.HasIndex(x => x.ShipmentId);
                entity.HasOne(l => l.CourierSettlement)
                    .WithMany(s => s.Lines)
                    .HasForeignKey(l => l.CourierSettlementId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(l => l.Shipment)
                    .WithMany()
                    .HasForeignKey(l => l.ShipmentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // WEIGHT DISCREPANCY
            // =========================
            modelBuilder.Entity<WeightDiscrepancy>(entity =>
            {
                entity.Property(x => x.AwbNumber).HasMaxLength(100);
                entity.Property(x => x.CourierName).HasMaxLength(150);
                entity.Property(x => x.ProductName).HasMaxLength(250);
                entity.Property(x => x.DisputeRemarks).HasMaxLength(500);
                entity.Property(x => x.ClosedRemarks).HasMaxLength(500);
                entity.Property(x => x.EnteredWeightGrams).HasPrecision(18, 2);
                entity.Property(x => x.AppliedWeightGrams).HasPrecision(18, 2);
                entity.Property(x => x.WeightCharges).HasPrecision(18, 2);
                entity.HasIndex(x => x.SellerId);
                entity.HasIndex(x => x.Status);
                entity.HasIndex(x => x.AwbNumber);
                entity.HasIndex(x => x.WeightAppliedDate);
                entity.HasOne(w => w.Seller)
                    .WithMany()
                    .HasForeignKey(w => w.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(w => w.Shipment)
                    .WithMany()
                    .HasForeignKey(w => w.ShipmentId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(w => w.Order)
                    .WithMany()
                    .HasForeignKey(w => w.OrderId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // =========================
            // PRODUCT WEIGHT FREEZE
            // =========================
            modelBuilder.Entity<ProductWeightFreeze>(entity =>
            {
                entity.Property(x => x.ProductId).HasMaxLength(100).IsRequired();
                entity.Property(x => x.ProductName).HasMaxLength(250).IsRequired();
                entity.Property(x => x.Sku).HasMaxLength(100);
                entity.Property(x => x.ActionRemarks).HasMaxLength(500);
                entity.Property(x => x.ActionedBy).HasMaxLength(450);
                entity.Property(x => x.LengthCm).HasPrecision(18, 2);
                entity.Property(x => x.BreadthCm).HasPrecision(18, 2);
                entity.Property(x => x.HeightCm).HasPrecision(18, 2);
                entity.Property(x => x.WeightGrams).HasPrecision(18, 2);
                entity.HasIndex(x => x.SellerId);
                entity.HasIndex(x => x.Status);
                entity.HasIndex(x => x.ProductId);
                entity.HasIndex(x => new { x.SellerId, x.ProductId });
                entity.HasOne(w => w.Seller)
                    .WithMany()
                    .HasForeignKey(w => w.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}