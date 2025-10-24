using HospitalityCustomerAPI.Models.HCAEntity;
using HospitalityCustomerAPI.Models.POSEntity;
using HospitalityCustomerAPI.Repositories;
using HospitalityCustomerAPI.Repositories.IRepositories;
using HospitalityCustomerAPI.Repository;
using HospitalityCustomerAPI.Repository.IRepository;
using HospitalityCustomerAPI.Services;
using HospitalityCustomerAPI.Services.IServices;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//scaffold-dbcontext 'Name=ConnectionStrings:HCA' microsoft.entityframeworkcore.sqlserver -outputdir Models/HCAEntity -f -NoPluralize
//scaffold-dbcontext 'Name=ConnectionStrings:POS' microsoft.entityframeworkcore.sqlserver -outputdir Models/POSEntity -f -NoPluralize -tables ops_LichSuMuaGoiDichVu, ops_CheckIn, tbl_DiemBanHang, tbl_KhachHang, cat_PhuongXa, cat_QuocGia, cat_TinhThanh, ops_OpenDoor, tbl_PhongBan

builder.Services.AddDbContext<HungDuyHospitalityCustomerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HCA")));
builder.Services.AddDbContext<HungDuyHospitalityContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("POS")));

// ================== Add Repository ==================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISmsOtpRepository, SmsOtpRepository>();
builder.Services.AddScoped<INewsRepository, NewsRepository>();
builder.Services.AddScoped<IKhachHangRepository, KhachHangRepository>();
builder.Services.AddScoped<ICheckInRepository, CheckInRepository>();
builder.Services.AddScoped<ILichSuMuaGoiDichVuRepository, LichSuMuaGoiDichVuRepository>();
builder.Services.AddScoped<ILichSuMuaGoiDichVuPOSRepository, LichSuMuaGoiDichVuPOSRepository>();
builder.Services.AddScoped<IDiemBanHangPOSRepository, DiemBanHangPOSRepository>();
builder.Services.AddScoped<IQuocGiaRepository, QuocGiaRepository>();
builder.Services.AddScoped<ITinhThanhRepository, TinhThanhRepository>();
builder.Services.AddScoped<IPhuongXaRepository, PhuongXaRepository>();

// ================== Add Services ==================
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();
builder.Services.Configure<EspOptions>(builder.Configuration.GetSection("EspOptions"));
builder.Services.AddTransient<IEspClient, EspClient>();


builder.Services.AddHttpClient<INotificationService, NotificationService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//dòng app run này chiến xài để debug đừng xóa
//app.Run("http://0.0.0.0:5000");
app.Run();
