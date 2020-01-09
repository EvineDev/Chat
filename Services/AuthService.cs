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

namespace Chat.Service
{
	public class AuthService
	{
        public static readonly string AUTH_SESSION = ".Auth.Session";

        private readonly DatabaseContext dbContext;
        private readonly HttpContext context;
		private readonly SessionService sessionService;

		public AuthService(IHttpContextAccessor httpContextAccessor, DatabaseContext dbContext, SessionService sessionService)
        {
            this.dbContext = dbContext;
            this.context = httpContextAccessor.HttpContext;
			this.sessionService = sessionService;

		}

        public void Logout()
        {
			// Fix: auth before logout
			var session = sessionService.TryGetSession();
			if (session == null)
				return;

			//context.Request.Cookies.Where(x => x.Key).Single
			context.Response.Cookies.Delete(AUTH_SESSION);
            //dbContext.Sessions.Where().Remove();
            //dbContext.SaveChanges();
        }

		public void LogoutAll()
		{
			// Fix: auth before logout

			//context.Request.Cookies.Where(x => x.Key).Single
			context.Response.Cookies.Delete(AUTH_SESSION);
			//dbContext.Sessions.Where().Remove();
			//dbContext.SaveChanges();
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

		private void GenerateLogoutToken(UserDb session)
		{
			var key = GetRandom(32);
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

    public class SessionDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("session-key")]
        public string SessionKey { get; set; }

        [JsonPropertyName("refresh-key")]
        public string RefreshKey { get; set; }
    }
}
