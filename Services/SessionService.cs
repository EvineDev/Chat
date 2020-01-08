using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Chat.Db;
using Microsoft.AspNetCore.Http;
using Chat.Controllers;
using Microsoft.EntityFrameworkCore;

namespace Chat.Service
{
	public class SessionService
    {
        private SessionDb session;

        public SessionDb GetSession()
        {
            if (session == null)
                throw new Exception("Session is null");

            return session;
        }

		public SessionDb TryGetSession()
		{
			return session;
		}

		public void SetSession(SessionDb newSession)
        {
            if (newSession == null)
                throw new Exception("Trying to set null session");

            if (session != null)
                throw new Exception("Session has already been set");

            session = newSession;
        }

        internal bool IsAuthenticated()
        {
            return session != null;
        }
    }
}
