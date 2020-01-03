using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Chat.Db;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Net.Mail;
using Chat.Dto;
using System.Text.RegularExpressions;

namespace Chat.Service
{
    public class FragmentService
    {
        public FragmentService()
        {
        }

		public string Chat(List<MessageDto> messageList, List<UserDto> userList)
		{
			return Layout(@$"
<body id='chat-body'>
    <header>
		<h1>My Little Game Chat</h1>
    </header>
	<nav>
        <form action='/logout' method='post'>
            <button>Logout</button>
        </form>
        <label for='channel-select'></label>
        <select name='channel' id='channel-select'>
            <option value='Best Pony'>Best Pony</option>
            <option value='Super Secret Place'>Super Secret Place</option>
        </select>
    </nav>
    <section id='chat-box'>{String.Join("\n", messageList.Select(m => RenderHtmlMessage(m)))}</section>
    <section id='user-list'>
        <h4>Online Users</h4>
        <div id='user-box'>{String.Join("\n", userList.Select(u => RenderHtmlUser(u)))}</div>
    </section>
    <section id='settings'>
        <h4>Settings</h4>
        <form action='/upload-avatar' method='post' enctype='multipart/form-data'>
            <label class='label-file' for='avatar'>Upload avatar</label>
            <input class='input-file' type='file' id='avatar' name='avatar' accept='image/png, image/jpeg' />
            <button>Save</button>
        </form>
    </section>
    <section id='chat-input'>
        <form action='/send-message' method='post'>
            <label for='message'></label>
            <textarea type='text' name='message' id='chat-input-textarea'></textarea>
            <button class='input-submit'>Send</button>
        </form>
    </section>
    <script>
        let cookies = Object.fromEntries(document.cookie.split('; ').map(x => x.split('=')));
        let lastMessageId = null;

        function init() {{
            setupChatboxInital();
            setupActiveUsers();
            setupChatInput();

            //setInterval(setupMessagePolling, 1000);
        }}

        function setupChatInput() {{
            let input = document.getElementById('chat-input-textarea');
            input.addEventListener('keypress', submitEnter);
            input.focus();

            function submitEnter(event) {{
                if (event.which === 13 && event.shiftKey === false) {{
                    event.preventDefault();
                    event.target.form.dispatchEvent(new Event('submit', {{ cancelable: true }}));
                }}
            }}
        }}

        function setupMessagePolling() {{
            let chat = document.getElementById('chat-box');
            fetch('/message-poll/main/' + lastMessageId)
                .then(x => {{
                    if (x.status >= 400 && x.status < 500) {{
                        chat.appendChild(document.createTextNode('Unauthorized, please log-in'));
                        throw x;
                    }} else if (x.status >= 200 && x.status < 300) {{
                        return x.text();
                    }} else {{
                        chat.appendChild(document.createTextNode('Server Error'));
                        throw x;
                    }}
                }}).then(x => {{
                    if (x == '') {{
                        return;
                    }}

                    chat.innerHTML += x;

                    let timeElements = chat.querySelectorAll('time');
                    for (let time of timeElements) {{
                        time.innerText = `(${{formatTime(time.dateTime)}})`;
                    }}

                    lastMessageId = chat.lastChild.dataset.messageId;

                    // Fix: What a beatiful world
                    setTimeout(() => {{ chat.scrollTop = 99999999; }}, 400);
                }});

        }}

        function setupChatboxInital() {{
            let chat = document.getElementById('chat-box');
			let timeElements = chat.querySelectorAll('time');
            for (let time of timeElements) {{
                time.innerText = `(${{formatTime(time.dateTime)}})`;
            }}

            lastMessageId = chat.lastChild.dataset.messageId;

            // Fix: What a beatiful world
            setTimeout(() => {{ chat.scrollTop = 99999999; }}, 400);
        }}

        function setupActiveUsers() {{
            theThing();
            //setInterval(theThing, 5000);

            function theThing() {{
                let users = document.getElementById('user-box');
                let usersNew = document.createElement('div');
                usersNew.id = 'user-box';
                fetch('/active-users/main')
                    .then(x => {{
                        if (x.status >= 400 && x.status < 500) {{
                            throw x;
                        }} else if (x.status >= 200 && x.status < 300) {{
                            return x.text();
                        }} else {{
                            usersNew.appendChild(document.createTextNode('Server Error'));
                            users.replaceWith(usersNew);
                            throw x;
                        }}
                    }}).then(x => {{
                        usersNew.innerHTML = x;
                        users.replaceWith(usersNew);
                    }});
            }}
        }}

        function formatTime(dateString) {{
            let date = new Date(dateString);
            let hour = date.getHours();
            if (hour < 10)
                hour = '0' + hour;
            let minutes = date.getMinutes();
            if (minutes < 10)
                minutes = '0' + minutes;
            let result = `${{hour}}:${{minutes}}`;

            return result;
        }}

        init();
    </script>
</body>");
		}

		public string Landing()
		{
			return Layout(@$"
<body>
    <header>
		<h1>My Little Game Chat</h1>
    </header>
	<section id='login'>
        <form action='/login' method='post'>
            <label for='email'>Email:</label>
            <input type='text' name='email' required id='user-login' />
            <label for='password'>Password:</label>
            <input type='password' name='password' autocomplete='current-password' required id='user-password' />
            <label for='remember'>Remember Me:</label>
            <input type='checkbox' name='remember' />
            <button>Login</button>
        </form>
    </section>
    <section id='register'>
		<form action='/register' method='post'>
			<label for='email'>Email:</label>
			<input type='text' name='email' required />
			<label for='username'>Username:</label>
			<input type='text' name='username' required />
			<label for='password'>Password:</label>
			<input type='password' name='password' autocomplete='new-password' required />
			<label for='capcha'>Capcha:</label>
			<input type='text' name='capcha' required />
			<button>Register</button>
		</form>
    </section>
    <script>
        let cookies = Object.fromEntries(document.cookie.split('; ').map(x => x.split('=')));

        function init() {{
			setupInput();
        }}

        function setupInput() {{
			let e = document.getElementById('user-login');
            e.focus();
            e.select();
        }}

        init();
    </script>
</body>");
		}

		public string Layout(string html)
		{
			return @$"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Wabble Chat</title>
    <link rel='stylesheet' type='text/css' href='style.css'>
</head>
{html}
</html>";
		}

		public string RenderHtmlMessage(MessageDto message)
		{
			var messageHtml = EncodeHtml(message.Message);
			var messageFinal = RenderHtmlUrl(messageHtml);

			var html = @$"
<div class='message-overall-container' data-message-id='{message.Id}'>
	<div class='message-container'>
		<span class='message-avatar-container'><img class='message-avatar' src='/api/avatar/{message.AvatarId}'></span>
		<p class='message-content'>
			<time class='message-time' datetime='{message.Created.ToString("yyyy-MM-ddTHH:mm:ssZ")}'>({message.Created.ToString("HH:mm")} UTC)</time>
			<span class='message-username'>{EncodeHtml(message.Username)}</span>:
			<span class='message-text'>{messageFinal}</span>
		</p>
	</div>
	{RenderHtmlMessageImages(message)}
</div>";

			return html;
		}

		public string RenderHtmlUser(UserDto user)
		{
			var html = $@"
<div class='userlist-container'>
	<span class='userlist-avatar-container'><img class='userlist-avatar' src='/api/avatar/{user.AvatarId}'></span>
	<span>{EncodeHtml(user.Username)}</span>
</div>";

			return html;
		}

		private string RenderHtmlUrl(string message)
		{
			var regex = new Regex("url\"(.*?)\"");
			var result = regex.Replace(message, x => {
				var urlText = x.Groups[1].ToString();
				var urlLink = urlText;
				// Fix: This is sorta ghetto, but it works as long as there isn't any :// in another part of the url.
				if (urlLink.Contains("://") == false)
					urlLink = "https://" + urlLink;

				var html = $"<a href='{EncodeUrl(urlLink)}' target='_blank' rel='noreferrer noopener'>{EncodeHtml(urlText)}</a>";
				return html;
			});

			return result;
		}

		private string RenderHtmlMessageImages(MessageDto message)
		{
			var result = "";
			var regex = new Regex("img\"(.*?)\"");
			var matchList = regex.Matches(message.Message);
			foreach (Match match in matchList)
			{
				result += $"<div class='image-container'><img class='image-message' src='{EncodeUrl(match.Groups[1].ToString())}'></div>";
			}

			return result;
		}

		private string EncodeHtml(string text)
		{
			// Fix: make good
			return text
				.Replace("&", "&amp;")
				.Replace("<", "&lt;")
				.Replace(">", "&gt;");
		}

		private string EncodeUrl(string text)
		{
			// Fix: make good
			return text
				.Replace("\"", "%22")
				.Replace("\'", "%27")
				.Replace("`", "%60")
				.Replace("\\", "\\\\");
		}
	}
}
