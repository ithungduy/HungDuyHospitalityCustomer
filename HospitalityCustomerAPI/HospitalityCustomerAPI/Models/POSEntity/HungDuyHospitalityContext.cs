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

    public virtual DbSet<CatPhuongXa> CatPhuongXa { get; set; }

    public virtual DbSet<CatQuocGia> CatQuocGia { get; set; }

    public virtual DbSet<CatTinhThanh> CatTinhThanh { get; set; }

    public virtual DbSet<OpsCheckIn> OpsCheckIn { get; set; }

    public virtual DbSet<OpsLichSuMuaGoiDichVu> OpsLichSuMuaGoiDichVu { get; set; }

    public virtual DbSet<OpsOpenDoor> OpsOpenDoor { get; set; }

    public virtual DbSet<TblDiemBanHang> TblDiemBanHang { get; set; }

    public virtual DbSet<TblKhachHang> TblKhachHang { get; set; }

    public virtual DbSet<TblPhongBan> TblPhongBan { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:POS");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CatPhuongXa>(entity =>
        {
            entity.HasKey(e => e.Ma);

            entity.ToTable("cat_PhuongXa");

            entity.Property(e => e.Ma).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Ten).HasMaxLength(100);
        });

        modelBuilder.Entity<CatQuocGia>(entity =>
        {
            entity.HasKey(e => e.Ma);

            entity.ToTable("cat_QuocGia");

            entity.Property(e => e.Ma).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Ten).HasMaxLength(100);
        });

        modelBuilder.Entity<CatTinhThanh>(entity =>
        {
            entity.HasKey(e => e.Ma);

            entity.ToTable("cat_TinhThanh");

            entity.Property(e => e.Ma).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Ten).HasMaxLength(100);
        });

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
            entity.Property(e => e.NgayHetHan).HasColumnType("datetime");
            entity.Property(e => e.NgayKichHoat).HasColumnType("datetime");
            entity.Property(e => e.NhanVienPt).HasColumnName("NhanVienPT");
        });

        modelBuilder.Entity<OpsOpenDoor>(entity =>
        {
            entity.HasKey(e => e.Ma).HasName("PK_ops_MoCua");

            entity.ToTable("ops_OpenDoor");

            entity.Property(e => e.Ma).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<TblDiemBanHang>(entity =>
        {
            entity.HasKey(e => new { e.Ma, e.MaChiNhanh }).HasName("PK_tbl_DiemBanHang_1");

            entity.ToTable("tbl_DiemBanHang");

            entity.Property(e => e.Ma).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ControlPin)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.IpOpenDoor)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Ten).HasMaxLength(200);
        });

        modelBuilder.Entity<TblKhachHang>(entity =>
        {
            entity.HasKey(e => e.Ma);

            entity.ToTable("tbl_KhachHang");

            entity.Property(e => e.Ma).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Cccd)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CCCD");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.DiaChi).HasMaxLength(200);
            entity.Property(e => e.DongYtuanThuNoiQuy).HasColumnName("DongYTuanThuNoiQuy");
            entity.Property(e => e.HanMucCongNo).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.IsCompany).HasColumnName("isCompany");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Msnv)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("MSNV");
            entity.Property(e => e.NgaySinh).HasColumnType("datetime");
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Ten).HasMaxLength(50);
            entity.Property(e => e.TienSuBenhLy).HasMaxLength(1000);
        });

        modelBuilder.Entity<TblPhongBan>(entity =>
        {
            entity.HasKey(e => e.Ma);

            entity.ToTable("tbl_PhongBan");

            entity.Property(e => e.Ma).HasDefaultValueSql("(newsequentialid())");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.MoTa).HasMaxLength(500);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Ten).HasMaxLength(500);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
