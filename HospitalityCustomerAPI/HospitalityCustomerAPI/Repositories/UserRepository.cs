using HospitalityCustomerAPI.Common;
using HospitalityCustomerAPI.Models.HCAEntity;
using HospitalityCustomerAPI.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using static HospitalityCustomerAPI.Common.Enum;

namespace HospitalityCustomerAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly HungDuyHospitalityCustomerContext _context;
        public UserRepository(HungDuyHospitalityCustomerContext context)
        {
            _context = context;
        }

        public int Add(SysUser entity)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                entity.Password = Utility.GetSHA512(AppConstants.HCA_PASSCODE + entity.Username + entity.Password);
                entity.CreatedDate = DateTime.Now;
                entity.Status = Status.Active.isActive();

                _context.Add(entity);
                _context.SaveChanges();
                transaction.Commit();
                return ActionStatus.Success.toInt();
            }
            catch
            {
                transaction.Rollback();
                return ActionStatus.Error.toInt();
            }
        }

        public (int, SysUser?) Authenticate(string username, string password)
        {
            var result = _context.Set<SysUser>().FirstOrDefault(t =>
                        (t.Username ?? "").Equals(username) &&
                        (t.Password ?? "").Equals(Utility.GetSHA512(AppConstants.HCA_PASSCODE + username + password)) &&
                        !(t.Deleted ?? false));

            return ((int)(result != null ? ((result.Status ?? false) ? ActionStatus.Success : ActionStatus.Disable) : ActionStatus.Error), result);
        }

        public int Delete(string username)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var user = _context.Set<SysUser>()
                                   .FirstOrDefault(t => t.Username == username && !(t.Deleted ?? false));
                if (user == null)
                {
                    return ActionStatus.NotExit.toInt();
                }

                user.Deleted = true;
                user.ModifiedDate = DateTime.Now;

                _context.Update(user);
                _context.SaveChanges();

                transaction.Commit();
                return ActionStatus.Success.toInt();
            }
            catch
            {
                transaction.Rollback();
                return ActionStatus.Error.toInt();
            }
        }

        public IEnumerable<SysUser> GetData()
        {
            return _context.Set<SysUser>()
                   .Where(t => !(t.Deleted ?? false))
                   .OrderByDescending(t => t.CreatedDate)
                   .ToList();
        }

        public SysUser? GetItem(Guid id)
        {
            return _context.Set<SysUser>().FirstOrDefault(t => t.Ma == id && !(t.Deleted ?? false));
        }

        public SysUser? GetItemByPhone(string phone)
        {
            return _context.Set<SysUser>().FirstOrDefault(t => t.Username == phone && !(t.Deleted ?? false));
        }
        public int ResetForgotPassword(string username, string newPassword)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var user = _context.Set<SysUser>().FirstOrDefault(t => t.Username == username && !(t.Deleted ?? false));
                if (user == null)
                {
                    return ActionStatus.NotExit.toInt();
                }

                user.Password = Utility.GetSHA512(AppConstants.HCA_PASSCODE + username + newPassword);
                user.ModifiedDate = DateTime.Now;

                _context.Update(user);
                _context.SaveChanges();
                transaction.Commit();

                return ActionStatus.Success.toInt();
            }
            catch
            {
                transaction.Rollback();
                return ActionStatus.Error.toInt();
            }
        }

        public int ResetPassword(string username, string oldPassword, string newPassword, Guid userId)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var user = _context.Set<SysUser>().FirstOrDefault(t => t.Username == username && !(t.Deleted ?? false));
                if (user == null)
                {
                    return ActionStatus.NotExit.toInt();
                }

                if (user.Password != Utility.GetSHA512(AppConstants.HCA_PASSCODE + username + oldPassword))
                {
                    return ActionStatus.NotMatch.toInt();
                }

                user.Password = Utility.GetSHA512(AppConstants.HCA_PASSCODE + username + newPassword);
                user.UserModified = userId;
                user.ModifiedDate = DateTime.Now;

                _context.Update(user);
                _context.SaveChanges();
                transaction.Commit();

                return ActionStatus.Success.toInt();
            }
            catch
            {
                transaction.Rollback();
                return ActionStatus.Error.toInt();
            }
        }

        public int Update(SysUser entity, Guid userId)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var user = _context.Set<SysUser>().FirstOrDefault(t => t.Ma == entity.Ma && !(t.Deleted ?? false));
                if (user == null)
                {
                    return ActionStatus.NotExit.toInt();
                }

                user.FullName = entity.FullName;
                user.HinhAnh = entity.HinhAnh;
                user.NgaySinh = entity.NgaySinh ?? user.NgaySinh;
                user.GioiTinh = entity.GioiTinh;
                user.Fcm = entity.Fcm;
                user.Token = entity.Token;
                user.Status = entity.Status;
                user.UserModified = userId;
                user.ModifiedDate = DateTime.Now;

                _context.Update(user);
                _context.SaveChanges();
                transaction.Commit();

                return ActionStatus.Success.toInt();
            }
            catch
            {
                transaction.Rollback();
                return ActionStatus.Error.toInt();
            }
        }

    }
}
