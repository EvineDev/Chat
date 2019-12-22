using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Db;
using Chat.Dto;
using Microsoft.EntityFrameworkCore;

namespace Chat.Service
{
	public class ChatService
	{
        private DatabaseContext dbContext;
        private SessionService authService;

        public ChatService(DatabaseContext dbContext, SessionService authService)
        {
            this.dbContext = dbContext;
            this.authService = authService;
        }

		public List<MessageDto> GetHistory(string board)
		{
            var limit = DateTime.UtcNow.AddHours(-36);

            var result = dbContext.Messages
                .Include(x => x.Session.User)
                .Where(x => x.Board == board)
                .Where(x => x.Created >= limit)
                .OrderByDescending(x => x.Created)
                .Take(200)
                .OrderBy(x => x.Created) // TakeLast is not supported
                .Select(x => new MessageDto { UserId = x.Session.User.Id, Username = x.Session.User.Username, Board = x.Board, Message = x.Message, Created = x.Created })
                .ToList();

            foreach (var m in result)
            {
                // Fix: Is the a better way to specify this?
                m.Created = DateTime.SpecifyKind(m.Created, DateTimeKind.Utc);
            }

            return result;
		}

        public BinaryDto GetAvatar(Guid id)
        {
            var avatar = dbContext.Users
                .Where(x => x.Id == id)
                .Select(x => new BinaryDto { ContentType = x.ContentType, Data = x.Avatar })
                .FirstOrDefault();

            return avatar;
        }

        public void SendMessage(string board, string message)
		{
            dbContext.Messages.Add(new MessageDb
			{
                Session = authService.GetSession(),
				Board = board,
				Message = message,
                Created = DateTime.UtcNow,
			});

            dbContext.SaveChanges();
		}

        public List<UserDto> GetActiveUsers(string board)
        {
            var newerThan = DateTime.UtcNow.AddMinutes(-7);

            var result = dbContext.Sessions
				// Fix: Temporality we just select all users
				//.Where(x => x.Created >= newerThan)
				.Select(x => x.User)
                .Distinct()
                .Select(x => new UserDto { UserId = x.Id, Username = x.Username })
                .ToList();

            return result;
        }
	}
}
