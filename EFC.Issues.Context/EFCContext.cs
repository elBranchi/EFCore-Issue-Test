using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace EFC.Issues.Context
{
    public partial class EFCContext : DbContext
    {
        public EFCContext()
        {
        }

        public EFCContext(DbContextOptions<EFCContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CLIENT> CLIENT { get; set; }
        public virtual DbSet<CLIENT_CONTACT> CLIENT_CONTACT { get; set; }
        public virtual DbSet<ORDER> ORDER { get; set; }
        public virtual DbSet<ORDER_DETAIL> ORDER_DETAIL { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
//            if (!optionsBuilder.IsConfigured)
//            {
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
//                optionsBuilder.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=db_efc_issues;Integrated Security=True");
//            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("ProductVersion", "2.2.2-servicing-10034");

            modelBuilder.Entity<CLIENT>(entity =>
            {
                entity.HasKey(e => e.CLIENT_ID);

                entity.Property(e => e.CLIENT_ID)
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .ValueGeneratedNever()
                    .IsFixedLength();

                entity.Property(e => e.CLIENT_NAME)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<CLIENT_CONTACT>(entity =>
            {
                entity.HasKey(e => new { e.CLIENT_ID, e.CONTACT_ID });

                entity.Property(e => e.CLIENT_ID)
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.CONTACT_EMAIL)
                    .HasMaxLength(120)
                    .IsUnicode(false);

                entity.Property(e => e.CONTACT_NAME)
                    .IsRequired()
                    .HasMaxLength(40)
                    .IsUnicode(false);

                entity.Property(e => e.CONTACT_PHONE)
                    .HasMaxLength(30)
                    .IsUnicode(false);

                entity.HasOne(d => d.CLIENT_)
                    .WithMany(p => p.CLIENT_CONTACT)
                    .HasForeignKey(d => d.CLIENT_ID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CL_CONTACT_CLIENT");
            });

            modelBuilder.Entity<ORDER>(entity =>
            {
                entity.HasKey(e => e.ORDER_ID);

                entity.Property(e => e.ORDER_ID).ValueGeneratedNever();

                entity.Property(e => e.CLIENT_ID)
                    .IsRequired()
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.Property(e => e.CREATION_DATE).HasColumnType("datetime");

                entity.HasOne(d => d.CLIENT_)
                    .WithMany(p => p.ORDER)
                    .HasForeignKey(d => d.CLIENT_ID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ORDER_CLIENT");
            });

            modelBuilder.Entity<ORDER_DETAIL>(entity =>
            {
                entity.HasKey(e => e.ORDER_ID);

                entity.Property(e => e.ORDER_ID).ValueGeneratedNever();

                entity.Property(e => e.BILLING_TYPE)
                    .IsRequired()
                    .HasMaxLength(2)
                    .IsUnicode(false);

                entity.Property(e => e.CLIENT_ID)
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .IsFixedLength();

                entity.HasOne(d => d.ORDER_)
                    .WithOne(p => p.ORDER_DETAIL)
                    .HasForeignKey<ORDER_DETAIL>(d => d.ORDER_ID)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_ORDER_DETAIL");

                entity.HasOne(d => d.CLIENT_BILLING_CONTACT)
                    .WithMany(p => p.ORDER_DETAIL_AS_BILLING_CONTACT)
                    .HasForeignKey(d => new { d.CLIENT_ID, d.BILLING_CONTACT_ID })
                    .HasConstraintName("FK_BILLING_CONTACT");

                entity.HasOne(d => d.CLIENT_SHIPPING_CONTACT)
                    .WithMany(p => p.ORDER_DETAIL_AS_SHIPPING_CONTACT)
                    .HasForeignKey(d => new { d.CLIENT_ID, d.SHIPPING_CONTACT_ID })
                    .HasConstraintName("FK_SHIPPING_CONTACT");
            });
        }
    }
}
