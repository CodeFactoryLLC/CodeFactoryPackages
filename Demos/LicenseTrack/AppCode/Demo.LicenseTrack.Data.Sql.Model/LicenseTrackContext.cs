using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Demo.LicenseTrack.Data.Sql.Model;

public partial class LicenseTrackContext : DbContext
{
    public LicenseTrackContext(DbContextOptions<LicenseTrackContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblCustomer> TblCustomers { get; set; }

    public virtual DbSet<TblLicense> TblLicenses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblCustomer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblCusto__3214EC070C92737E");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        modelBuilder.Entity<TblLicense>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__TblLicen__3214EC07BF23D964");

            entity.Property(e => e.Id).ValueGeneratedNever();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
