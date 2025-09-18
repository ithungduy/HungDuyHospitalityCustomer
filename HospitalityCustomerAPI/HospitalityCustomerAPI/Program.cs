using HospitalityCustomerAPI.Models.HCAEntity;
using HospitalityCustomerAPI.Models.POSEntity;
using HospitalityCustomerAPI.Repositories;
using HospitalityCustomerAPI.Repositories.IRepositories;
using HospitalityCustomerAPI.Services;
using HospitalityCustomerAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//scaffold-dbcontext 'Name=ConnectionStrings:HCA' microsoft.entityframeworkcore.sqlserver -outputdir Models/HCAEntity -f -NoPluralize
//scaffold-dbcontext 'Name=ConnectionStrings:POS' microsoft.entityframeworkcore.sqlserver -outputdir Models/POSEntity -f -NoPluralize -tables ops_LichSuMuaGoiDichVu, ops_CheckIn, tbl_DiemBanHang

builder.Services.AddDbContext<HungDuyHospitalityCustomerContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("HCA")));
builder.Services.AddDbContext<HungDuyHospitalityContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("POS")));

// ================== Add Repository ==================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ISmsOtpRepository, SmsOtpRepository>();

// ================== Add Services ==================
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IGoogleAuthService, GoogleAuthService>();

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

app.Run();
