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
        public DbSet<Shipment> Shipments => Set<Shipment>();
        public DbSet<ShipmentTracking> ShipmentTrackings => Set<ShipmentTracking>();
        public DbSet<Courier> Couriers => Set<Courier>();
        public DbSet<NDR> NDRS => Set<NDR>();
        public DbSet<RTO> RTOS => Set<RTO>();
        public DbSet<CourierServiceability> CourierServiceabilities => Set<CourierServiceability>();
        public DbSet<Webhook> Webhooks => Set<Webhook>();
        public DbSet<Wallet> Wallets => Set<Wallet>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
        public DbSet<CODSettlement> CODSettlements => Set<CODSettlement>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<Seller> Sellers => Set<Seller>();
        public DbSet<CompanyInfo> Companies => Set<CompanyInfo>();

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
            // 🔥 ORDER → SELLER
            // =========================
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Seller)
                .WithMany()
                .HasForeignKey(o => o.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // 🔥 ORDER → BILLING ADDRESS
            // =========================
            modelBuilder.Entity<Order>()
                .HasOne<Address>()
                .WithMany()
                .HasForeignKey(o => o.BillingAddressId)
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


            // =========================
            // 🔥 INDEXES
            // =========================

            // --- Seller ---
            modelBuilder.Entity<Seller>().HasIndex(s => s.UserId).IsUnique();
            modelBuilder.Entity<Seller>().HasIndex(s => s.GstNumber).IsUnique();
            modelBuilder.Entity<Seller>().HasIndex(s => s.CompanyId);

            // --- Wallet ---
            modelBuilder.Entity<Wallet>().HasIndex(w => w.UserId).IsUnique();

            // --- Transaction ---
            modelBuilder.Entity<Transaction>().HasIndex(t => t.WalletId);
            modelBuilder.Entity<Transaction>().HasIndex(t => t.TxnReference).IsUnique();

            // --- CODSettlement ---
            modelBuilder.Entity<CODSettlement>().HasIndex(c => c.ShipmentId).IsUnique();
            modelBuilder.Entity<CODSettlement>().HasIndex(c => c.SellerId);

            // --- Order ---
            modelBuilder.Entity<Order>().HasIndex(o => o.OrderRef).IsUnique();
            modelBuilder.Entity<Order>().HasIndex(o => o.ShippingPincode);

            // --- Shipment ---
            modelBuilder.Entity<Shipment>().HasIndex(s => s.AwbNumber).IsUnique();
            modelBuilder.Entity<Shipment>().HasIndex(s => s.DeliveryPincode);

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

            modelBuilder.Entity<Courier>()
                .Property(x => x.MaxWeightLimit)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(x => x.TotalAmount)
                .HasPrecision(18, 2);

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
        }
    }
}