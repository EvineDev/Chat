using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Db;
using Microsoft.AspNetCore.Http;
using Chat.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Security.Cryptography;
using Chat.Dto;

namespace Chat.Service
{
	public class AuthService
	{
        public static readonly string AUTH_SESSION = ".Auth.Session";
		public static readonly string AUTH_LOGOUT_TOKEN = ".Auth.LogoutToken";

		private readonly DatabaseContext dbContext;
        private readonly HttpContext context;
		private readonly SessionService sessionService;

		public AuthService(IHttpContextAccessor httpContextAccessor, DatabaseContext dbContext, SessionService sessionService)
        {
            this.dbContext = dbContext;
            this.context = httpContextAccessor.HttpContext;
			this.sessionService = sessionService;

		}

        public void Logout(SessionDb sessionDb)
        {
			context.Response.Cookies.Delete(AUTH_SESSION);
			dbContext.Sessions.Remove(sessionDb);
			// Fix: Currently we have to null out any refrences to sessions. Is there a way to say that references might be invalid?
			var messages = dbContext.Messages.Include(x => x.Session).Where(x => x.Session.Id == sessionDb.Id).ToList();
			dbContext.SaveChanges();
		}

		public void LogoutAll(UserDb userDb)
		{
			context.Response.Cookies.Delete(AUTH_SESSION);
			context.Response.Cookies.Delete(AUTH_LOGOUT_TOKEN);
			var logoutTokens = dbContext.LogoutToken.Include(x => x.User).Where(x => x.User.Id == userDb.Id).ToList();
			var sessions = dbContext.Sessions.Include(x => x.User).Where(x => x.User.Id == userDb.Id).ToList();
			// Fix: Currently we have to null out any refrences to sessions. Is there a way to say that references might be invalid?
			var messages = dbContext.Messages.Include(x => x.Session.User).Where(x => x.Session.User.Id == userDb.Id).ToList();
			messages.ForEach(x => x.Session = null);

			dbContext.Sessions.RemoveRange(sessions);
			dbContext.LogoutToken.RemoveRange(logoutTokens);
			dbContext.SaveChanges();
		}

		public bool Register(string email, string username, string password, string capcha)
        {
			// Fix: Add proper capcha
			if (capcha != "pony-capcha")
				return false;

            var existingUser = dbContext.Users.Where(x => x.Email == email).FirstOrDefault();
            if (existingUser != null)
                return false;

            var salt = GetRandom(32);
            var passwordHashed = GetPasswordHashed(password, salt);

            var user = new UserDb
            {
                Username = username,
                Email = email,
                Password = passwordHashed,
                Salt = salt,
            };

            dbContext.Users.Add(user);
            dbContext.SaveChanges();

            var newSessionDb = GenerateSession(user);

            return true;
        }

        public bool Login(AccessDto accessDto)
        {
            var user = dbContext.Users.Where(x => x.Email == accessDto.Email).FirstOrDefault();
            if (user == null)
                return false;

            var passwordHashed = GetPasswordHashed(accessDto.Password, user.Salt);
            if (passwordHashed.SequenceEqual(user.Password) == false)
                return false;

            var newSessionDb = GenerateSession(user);
            return true;
        }

        public SessionDb AuthenticateSession()
        {
            var sessionBase64 = context.Request.Cookies[AUTH_SESSION];
            if (sessionBase64 == null)
                return null;
            var sessionJsonBytes = Convert.FromBase64String(sessionBase64);
            var sessionJson = System.Text.Encoding.UTF8.GetString(sessionJsonBytes);

            var sessionDto = JsonSerializer.Deserialize<SessionDto>(sessionJson);
            var sessionDb = dbContext.Sessions
                .Include(x => x.User)
                .Where(x => x.Id == sessionDto.Id)
                .FirstOrDefault();
            if (sessionDb == null)
                return null;

            // Fix: Think on this compare
            if (sessionDb.SessionKey == null || sessionDb.Created > DateTime.UtcNow.AddHours(1))
            {
                var refreshKeyDto = Convert.FromBase64String(sessionDto.RefreshKey);
                var refreshHashDb = GetRefreshKeyHashed(refreshKeyDto, sessionDb.RefreshSalt);
                if (refreshHashDb == null)
                    return null;
                if (refreshHashDb.SequenceEqual(sessionDb.RefreshKey) == false)
                    return null;

                var newSessionDb = GenerateSession(sessionDb.User);
                return newSessionDb;
            }
            else
            {
                var sessionKeyDto = Convert.FromBase64String(sessionDto.SessionKey);
                if (sessionDb.SessionKey.SequenceEqual(sessionKeyDto) == false)
                {
                    // Delete the key if it fails to authenticate.
                    // This should only happen if someone is attemting to spoof the session key.
                    sessionDb.SessionKey = null;
                    dbContext.SaveChanges();
                    return null;
                }
            }

            return sessionDb;
        }

		public void GenerateLogoutToken(UserDb user)
		{
			var key = GetRandom(32);
			var keySalt = GetRandom(16);
			var keyHash = GetRefreshKeyHashed(key, keySalt);

			var logoutToken = new LogoutTokenDb
			{
				Created = DateTime.UtcNow,
				TokenKey = keyHash,
				TokenSalt = keySalt,
				User = user,
			};

			dbContext.LogoutToken.Add(logoutToken);
			dbContext.SaveChanges();

			var logoutTokenDto = new LogoutTokenDto
			{
				Id = logoutToken.Id,
				TokenKey = Convert.ToBase64String(key),
			};

			var authOptions = new CookieOptions
			{
				MaxAge = new TimeSpan(0, 30, 0),
			};

			var sessionJson = JsonSerializer.Serialize(logoutTokenDto);
			var sessionJsonBytes = System.Text.Encoding.UTF8.GetBytes(sessionJson);
			var sessionBase64 = Convert.ToBase64String(sessionJsonBytes);
			context.Response.Cookies.Append(AUTH_LOGOUT_TOKEN, sessionBase64, authOptions);
		}

		public LogoutTokenDb AuthenticateLogoutToken()
		{
			var logoutTokenBase64 = context.Request.Cookies[AUTH_LOGOUT_TOKEN];
			if (logoutTokenBase64 == null)
				return null;
			var logoutTokenJsonBytes = Convert.FromBase64String(logoutTokenBase64);
			var logoutTokenJson = System.Text.Encoding.UTF8.GetString(logoutTokenJsonBytes);

			var logoutTokenDto = JsonSerializer.Deserialize<LogoutTokenDto>(logoutTokenJson);
			var logoutTokenDb = dbContext.LogoutToken
				.Include(x => x.User)
				.Where(x => x.Id == logoutTokenDto.Id)
				.FirstOrDefault();
			if (logoutTokenDb == null)
				return null;

			var tokenKeyDto = Convert.FromBase64String(logoutTokenDto.TokenKey);
			var tokenHashDb = GetRefreshKeyHashed(tokenKeyDto, logoutTokenDb.TokenSalt);
			if (tokenHashDb == null)
				return null;
			if (tokenHashDb.SequenceEqual(logoutTokenDb.TokenKey) == false)
				return null;

			return logoutTokenDb;
		}

		public void RemoveLogoutToken(LogoutTokenDb logoutToken)
		{
			// Fix: Delete from Db
			context.Response.Cookies.Delete(AUTH_LOGOUT_TOKEN);
			
			dbContext.LogoutToken.Remove(logoutToken);
			dbContext.SaveChanges();
		}


		private SessionDb GenerateSession(UserDb user)
        {
            var newRefreshKey = GetRandom(47);
            var newRefreshSalt = GetRandom(17);
            var newSessionKey = GetRandom(32);

            var newSessionDb = new SessionDb
            {
                User = user,
                Created = DateTime.UtcNow,
                SessionKey = newSessionKey,
                RefreshKey = GetRefreshKeyHashed(newRefreshKey, newRefreshSalt),
                RefreshSalt = newRefreshSalt,
            };

            dbContext.Sessions.Add(newSessionDb);
            dbContext.SaveChanges();

            var newSessionDto = new SessionDto
            {
                Id = newSessionDb.Id,
                SessionKey = Convert.ToBase64String(newSessionKey),
                RefreshKey = Convert.ToBase64String(newRefreshKey),
            };

            var authOptions = new CookieOptions
            {
                MaxAge = TimeSpan.FromDays(61),
                SameSite = SameSiteMode.Strict,
                Secure = true,
                HttpOnly = true,
            };

            var sessionJson = JsonSerializer.Serialize(newSessionDto);
            var sessionJsonBytes = System.Text.Encoding.UTF8.GetBytes(sessionJson);
            var sessionBase64 = Convert.ToBase64String(sessionJsonBytes);
            context.Response.Cookies.Append(AUTH_SESSION, sessionBase64, authOptions);

            return newSessionDb;
        }

        private byte[] GetRandom(int count)
        {
            var result = new byte[count];
            using var rng = new RNGCryptoServiceProvider();
            rng.GetBytes(result);

            return result;
        }

        private byte[] GetPasswordHashed(string password, byte[] salt)
        {
            using var rfc2898 = new Rfc2898DeriveBytes(password, salt, 1000);
            var result = rfc2898.GetBytes(128); // Fix: What impact does getbytes have, is 128 bytes enough?
            return result;
        }

        private byte[] GetRefreshKeyHashed(byte[] refreshKey, byte[] salt)
        {
            using var rfc2898 = new Rfc2898DeriveBytes(refreshKey, salt, 1000);
            var result = rfc2898.GetBytes(128);
            return result;
        }
    }
}
