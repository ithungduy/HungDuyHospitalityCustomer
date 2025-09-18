using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HospitalityCustomerAPI.Models.POSEntity;

public partial class HungDuyHospitalityContext : DbContext
{
    public HungDuyHospitalityContext()
    {
    }

    public HungDuyHospitalityContext(DbContextOptions<HungDuyHospitalityContext> options)
        : base(options)
    {
    }

    public virtual DbSet<OpsCheckIn> OpsCheckIn { get; set; }

    public virtual DbSet<OpsLichSuMuaGoiDichVu> OpsLichSuMuaGoiDichVu { get; set; }

    public virtual DbSet<TblDiemBanHang> TblDiemBanHang { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:POS");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OpsCheckIn>(entity =>
        {
            entity.HasKey(e => e.Ma).HasName("PK_chk_CheckIn");

            entity.ToTable("ops_CheckIn");

            entity.Property(e => e.Ma).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.NgayCheckIn).HasColumnType("datetime");
            entity.Property(e => e.TienPhuCap).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<OpsLichSuMuaGoiDichVu>(entity =>
        {
            entity.HasKey(e => e.Ma);

            entity.ToTable("ops_LichSuMuaGoiDichVu");

            entity.Property(e => e.Ma).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.NgayKichHoat).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblDiemBanHang>(entity =>
        {
            entity.HasKey(e => new { e.Ma, e.MaChiNhanh }).HasName("PK_tbl_DiemBanHang_1");

            entity.ToTable("tbl_DiemBanHang");

            entity.Property(e => e.Ma).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Ten).HasMaxLength(200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
