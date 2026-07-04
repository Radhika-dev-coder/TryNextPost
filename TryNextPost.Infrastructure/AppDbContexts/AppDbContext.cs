using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TryNextPost.Domain.Entities;
using TryNextPost.Domain.Enums;
using TryNextPost.Infrastructure.Identity;

namespace TryNextPost.Infrastructure.AppDbContexts
{
    public class AppDbContext : IdentityDbContext<ApplicationUser,ApplicaitonRole,string>
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // =========================
            // 🔥 USER → ADDRESS
            // =========================
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Addresses)
                .WithOne()
                .HasForeignKey("UserId");

            // =========================
            // 🔥 ORDER → ORDER ITEMS
            // =========================
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderItems)
                .WithOne(oi => oi.Order)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // 🔥 ORDER → USER (SELLER)
            // =========================
            modelBuilder.Entity<Order>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(o => o.SellerId)
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
            // 🔥 SHIPMENT → ADDRESS
            // =========================
            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.PickupAddress)
                .WithMany()
                .HasForeignKey(s => s.PickupAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Shipment>()
                .HasOne(s => s.DeliveryAddress)
                .WithMany()
                .HasForeignKey(s => s.DeliveryAddressId)
                .OnDelete(DeleteBehavior.Restrict);

            // =========================
            // 🔥 SHIPMENT → TRACKING
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
                .WithMany()
                .HasForeignKey(n => n.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // 🔥 SHIPMENT → RTO
            // =========================
            modelBuilder.Entity<RTO>()
                .HasOne(r => r.Shipment)
                .WithMany()
                .HasForeignKey(r => r.ShipmentId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // 🔥 COURIER → SERVICEABILITY
            // =========================
            modelBuilder.Entity<CourierServiceability>(entity =>
            {
                entity.HasIndex(cs => cs.Pincode);
                entity.HasIndex(cs => cs.CourierId);

                entity.HasOne(cs => cs.Courier)
                      .WithMany(c => c.Serviceabilities)
                      .HasForeignKey(cs => cs.CourierId);
            });

            // =========================
            // 🔥 WALLET → TRANSACTIONS
            // =========================
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Wallet)
                .WithMany(w => w.Transactions)
                .HasForeignKey(t => t.WalletId)
                .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // 🔥 COD → SHIPMENT
            // =========================
            modelBuilder.Entity<CODSettlement>()
                .HasOne(c => c.Shipment)
                .WithMany()
                .HasForeignKey(c => c.ShipmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Wallet>()
    .HasIndex(w => w.UserId);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.WalletId);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.TxnReference);

            modelBuilder.Entity<CODSettlement>()
                .HasIndex(c => c.ShipmentId);

            modelBuilder.Entity<CODSettlement>()
                .HasIndex(c => c.SellerId);
        }

    
}
}
