let cookies = Object.fromEntries(document.cookie.split('; ').map(x => x.split('=')));
let lastMessageId = null;

init();

function init() {
    let body = document.querySelector("head");
    let page = body.dataset.page;
    if (page == "chat") {
        init_chat();
    } else if (page == "landing") {
		init_landing();
    } else {
        console.error("Unknown page: " + page);
    }
}

function init_chat() {
    setupChatboxInital();
    setupActiveUsers();
    setupChatInput();

    //setInterval(setupMessagePolling, 1000);
}

function init_landing() {
	setupInput();
}

function setupInput() {
    let e = document.getElementById('user-login');
    e.focus();
    e.select();
}

function setupChatInput() {
    let input = document.getElementById('chat-input-textarea');
    input.addEventListener('keypress', submitEnter);
    input.focus();

    function submitEnter(event) {
        if (event.which === 13 && event.shiftKey === false) {
            event.preventDefault();
            event.target.form.dispatchEvent(new Event('submit', { cancelable: true }));
        }
    }
}

function setupMessagePolling() {
    let chat = document.getElementById('chat-box');
    fetch('/message-poll/main/' + lastMessageId)
        .then(x => {
            if (x.status >= 400 && x.status < 500) {
                chat.appendChild(document.createTextNode('Unauthorized, please log-in'));
                throw x;
            } else if (x.status >= 200 && x.status < 300) {
                return x.text();
            } else {
                chat.appendChild(document.createTextNode('Server Error'));
                throw x;
            }
        }).then(x => {
            if (x == '') {
                return;
            }

            chat.innerHTML += x;

            let timeElements = chat.querySelectorAll('time');
            for (let time of timeElements) {
                time.innerText = `(${formatTime(time.dateTime)})`;
            }

            lastMessageId = chat.lastChild.dataset.messageId;

            // Fix: What a beatiful world
            setTimeout(() => { chat.scrollTop = 99999999; }, 400);
        });

}

function setupChatboxInital() {
    let chat = document.getElementById('chat-box');
    let timeElements = chat.querySelectorAll('time');
    for (let time of timeElements) {
        time.innerText = `(${formatTime(time.dateTime)})`;
    }

    lastMessageId = chat.lastChild.dataset.messageId;

    // Fix: What a beatiful world
    setTimeout(() => { chat.scrollTop = 99999999; }, 400);
}

function setupActiveUsers() {
    theThing();
    //setInterval(theThing, 5000);

    function theThing() {
        let users = document.getElementById('user-box');
        let usersNew = document.createElement('div');
        usersNew.id = 'user-box';
        fetch('/active-users/main')
            .then(x => {
                if (x.status >= 400 && x.status < 500) {
                    throw x;
                } else if (x.status >= 200 && x.status < 300) {
                    return x.text();
                } else {
                    usersNew.appendChild(document.createTextNode('Server Error'));
                    users.replaceWith(usersNew);
                    throw x;
                }
            }).then(x => {
                usersNew.innerHTML = x;
                users.replaceWith(usersNew);
            });
    }
}

function formatTime(dateString) {
    let date = new Date(dateString);
    let hour = date.getHours();
    if (hour < 10)
        hour = '0' + hour;
    let minutes = date.getMinutes();
    if (minutes < 10)
        minutes = '0' + minutes;
    let result = `${hour}:${minutes}`;

    return result;
}