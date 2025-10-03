using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HospitalityCustomerAPI.Models.HCAEntity;

public partial class HungDuyHospitalityCustomerContext : DbContext
{
    public HungDuyHospitalityCustomerContext()
    {
    }

    public HungDuyHospitalityCustomerContext(DbContextOptions<HungDuyHospitalityCustomerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<NwsLoaiTinTuc> NwsLoaiTinTuc { get; set; }

    public virtual DbSet<NwsTinTuc> NwsTinTuc { get; set; }

    public virtual DbSet<NwsVideoAds> NwsVideoAds { get; set; }

    public virtual DbSet<OpsCheckIn> OpsCheckIn { get; set; }

    public virtual DbSet<OpsLichSuMuaGoiDichVu> OpsLichSuMuaGoiDichVu { get; set; }

    public virtual DbSet<SysAppVersion> SysAppVersion { get; set; }

    public virtual DbSet<SysNotifications> SysNotifications { get; set; }

    public virtual DbSet<SysSmsOtp> SysSmsOtp { get; set; }

    public virtual DbSet<SysUser> SysUser { get; set; }

    public virtual DbSet<TblHangHoa> TblHangHoa { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:HCA");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NwsLoaiTinTuc>(entity =>
        {
            entity.HasKey(e => e.Ma).HasName("PK_nws_LoaiTinTuc_1");

            entity.ToTable("nws_LoaiTinTuc");

            entity.Property(e => e.Ma).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Ten).HasMaxLength(500);
        });

        modelBuilder.Entity<NwsTinTuc>(entity =>
        {
            entity.HasKey(e => e.Ma).HasName("PK_nws_TinTuc_1");

            entity.ToTable("nws_TinTuc");

            entity.Property(e => e.Ma).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.HinhAnh).HasMaxLength(500);
            entity.Property(e => e.Link).HasMaxLength(1000);
            entity.Property(e => e.MoTaNgan).HasMaxLength(2000);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.TenLoai).HasMaxLength(1000);
            entity.Property(e => e.Title).HasMaxLength(1000);
        });

        modelBuilder.Entity<NwsVideoAds>(entity =>
        {
            entity.HasKey(e => e.Ma).HasName("PK_nws_VideoAds_1");

            entity.ToTable("nws_VideoAds");

            entity.Property(e => e.Ma).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.Link).HasMaxLength(200);
            entity.Property(e => e.LinkWeb).HasMaxLength(200);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.Ten).HasMaxLength(500);
            entity.Property(e => e.Thumbnail).HasMaxLength(500);
        });

        modelBuilder.Entity<OpsCheckIn>(entity =>
        {
            entity.HasKey(e => e.Ma).HasName("PK_chk_CheckIn");

            entity.ToTable("ops_CheckIn");

            entity.Property(e => e.Ma).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.MaCheckInPos).HasColumnName("MaCheckInPOS");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.NgayCheckIn).HasColumnType("datetime");
            entity.Property(e => e.TienPhuCap).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<OpsLichSuMuaGoiDichVu>(entity =>
        {
            entity.HasKey(e => e.Ma);

            entity.ToTable("ops_LichSuMuaGoiDichVu");

            entity.Property(e => e.Ma).ValueGeneratedNever();
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.DonGia).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<SysAppVersion>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("sys_AppVersion");

            entity.Property(e => e.Appver).HasMaxLength(64);
            entity.Property(e => e.Description).HasMaxLength(2000);
        });

        modelBuilder.Entity<SysNotifications>(entity =>
        {
            entity.HasKey(e => e.Ma);

            entity.ToTable("sys_Notifications");

            entity.Property(e => e.Ma).ValueGeneratedNever();
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.FcmToken).HasMaxLength(200);
            entity.Property(e => e.Message).HasMaxLength(200);
            entity.Property(e => e.MessageId).HasMaxLength(200);
            entity.Property(e => e.NotificationType).HasMaxLength(200);
            entity.Property(e => e.ReadAt).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.UserPhone)
                .HasMaxLength(20)
                .IsFixedLength();
        });

        modelBuilder.Entity<SysSmsOtp>(entity =>
        {
            entity.HasKey(e => e.Ma);

            entity.ToTable("sys_SmsOtp");

            entity.Property(e => e.Ma).ValueGeneratedNever();
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.GiaTriGiaoDich).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Otp)
                .HasMaxLength(200)
                .IsFixedLength();
            entity.Property(e => e.Sdt)
                .HasMaxLength(20)
                .IsFixedLength();
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
        });

        modelBuilder.Entity<SysUser>(entity =>
        {
            entity.HasKey(e => e.Ma);

            entity.ToTable("sys_User");

            entity.Property(e => e.Ma).HasDefaultValueSql("(newid())");
            entity.Property(e => e.Cccd)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CCCD");
            entity.Property(e => e.CodeKhachHang)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.DeletedDate).HasColumnType("datetime");
            entity.Property(e => e.Fcm)
                .HasMaxLength(200)
                .HasColumnName("FCM");
            entity.Property(e => e.FullName).HasMaxLength(300);
            entity.Property(e => e.HinhAnh)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.HoChieu).HasMaxLength(100);
            entity.Property(e => e.ModifiedDate).HasColumnType("datetime");
            entity.Property(e => e.NgaySinh).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(300);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SoNha).HasMaxLength(500);
            entity.Property(e => e.Token)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<TblHangHoa>(entity =>
        {
            entity.HasKey(e => e.Ma);

            entity.ToTable("tbl_HangHoa");

            entity.Property(e => e.Ma).ValueGeneratedNever();
            entity.Property(e => e.MaDvt).HasColumnName("MaDVT");
            entity.Property(e => e.MaVach)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Ten).HasMaxLength(200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
