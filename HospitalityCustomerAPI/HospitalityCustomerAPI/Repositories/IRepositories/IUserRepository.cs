using HospitalityCustomerAPI.Models.HCAEntity;

namespace HospitalityCustomerAPI.Repositories.IRepositories
{
    public interface IUserRepository
    {
        SysUser? GetItem(Guid id);
        SysUser? GetItemByKhachHang(Guid MaKhachHang);
        IEnumerable<SysUser> GetData();
        SysUser? GetItemByPhone(string phone);
        (int, SysUser?) Authenticate(string username, string password);
        int Add(SysUser entity);
        int ResetPassword(string username, string oldPassword, string newPassword, Guid userId);
        int ResetForgotPassword(string username, string newPassword);
        int Update(SysUser entity, Guid userId);
        int Delete(string username);
    }
}
