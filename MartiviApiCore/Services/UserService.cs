using System;
using System.Collections.Generic;
using System.Linq;
using MaleApi.Data;
using MaleApiCore.Helpers;
using MaleApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MaleApi.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User GetById(int id);
        User Create(User user, string password);
        void Update(User user, string password = null);
        void UpdatePassword (User user, string password);
        void Delete(int id);
    }

    public class UserService : IUserService
    {
        private MaleDbContext _context;

        public UserService(MaleDbContext context)
        {
            _context = context;
        }

        public User Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = _context.Users.FirstOrDefault(x => x.Username == username);

            // check if username exists
            if (user == null)
                return null;

            // check if password is correct
            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            // authentication successful
            return user;
        }

        public IEnumerable<User> GetAll()
        {
            return _context.Users.Include(usr=>usr.UserAddresses).ThenInclude(usradr=>usradr.Coordinates);
        }

        public User GetById(int id)
        {
            return _context.Users.Include(usr=>usr.UserAddresses).ThenInclude(usradr=>usradr.Coordinates).FirstOrDefault(user => user.UserId == id);
            //return _context.Users.Include("Messages").FirstOrDefault(user => user.UserId == id);
        }

        public User Create(User user, string password)
        {
            // validation
            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password is required");

            if (_context.Users.Any(x => x.Username == user.Username))
                throw new AppException("Username \"" + user.Username + "\" is already taken");

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            
            _context.Users.Add(user);
            _context.SaveChanges();

            return user;
        }

        public void Update(User userParam, string password = null)
        {
            var user = _context.Users.Include(usr=>usr.UserAddresses).ThenInclude(usradr=>usradr.Coordinates).FirstOrDefault(u => u.UserId == userParam.UserId);

            var pUser = userParam.UserAddresses.FirstOrDefault(u => u.IsPrimary);
            if (pUser != null)
            {
                foreach(var u in user.UserAddresses)
                {
                    if (u.UserAddressId == pUser.UserAddressId) u.IsPrimary = true;
                    else u.IsPrimary = false;
                }
            }

           
           

            if (user == null)
                throw new AppException("User not found");

            // update username if it has changed
            if (!string.IsNullOrWhiteSpace(userParam.Username) && userParam.Username != user.Username)
            {
                // throw error if the new username is already taken
                if (_context.Users.Any(x => x.Username == userParam.Username))
                    throw new AppException("Username " + userParam.Username + " is already taken");

                user.Username = userParam.Username;
            }

            // update user properties if provided
            if (!string.IsNullOrWhiteSpace(userParam.FirstName))
                user.FirstName = userParam.FirstName;

            if (!string.IsNullOrWhiteSpace(userParam.LastName))
                user.LastName = userParam.LastName;

            if (!string.IsNullOrWhiteSpace(userParam.ProfileImageUrl))
                user.ProfileImageUrl = userParam.ProfileImageUrl;



            // update password if provided
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }

            _context.Users.Update(user);
            _context.SaveChanges();
        }
        public void UpdatePassword(User user, string password)
        {
            if (!string.IsNullOrWhiteSpace(password))
            {
                byte[] passwordHash, passwordSalt;
                CreatePasswordHash(password, out passwordHash, out passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
            }
            else throw new Exception("No password provided.");
            _context.SaveChanges();
        }
        public void Delete(int id)
        {

            var user = _context.Users.Include(usr=>usr.Messages).Include(usr=>usr.UserAddresses).ThenInclude(usradr=>usradr.Coordinates).FirstOrDefault(u => id == u.UserId);
            if (user != null)
            {
                var userOrders = _context.Orders.Include(ord=>ord.OrderedProducts).Include(ord=>ord.User).ThenInclude(usr=>usr.UserAddresses).ThenInclude(usradr=>usradr.Coordinates).Where(order => order.User.UserId == user.UserId);
                //var CanceleduserOrders = _context.CanceledOrders.Include(ord => ord.OrderedProducts).Include(ord => ord.User).ThenInclude(usr => usr.UserAddresses).ThenInclude(usradr => usradr.Coordinates).Where(order => order.User.UserId == user.UserId);
                //var CompleteduserOrders = _context.CompletedOrders.Include(ord => ord.OrderedProducts).Include(ord => ord.User).ThenInclude(usr => usr.UserAddresses).ThenInclude(usradr => usradr.Coordinates).Where(order => order.User.UserId == user.UserId);

                _context.Orders.RemoveRange(userOrders);
                //_context.CanceledOrders.RemoveRange(CanceleduserOrders);
                //_context.CompletedOrders.RemoveRange(CompleteduserOrders);
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        // private helper methods

        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
    }
}