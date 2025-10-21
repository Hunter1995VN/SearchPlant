// Modern Chatbot Widget
(function() {
  // Create chatbot toggle button
  const toggleBtn = document.createElement('button');
  toggleBtn.className = 'chatbot-toggle-btn';
  toggleBtn.innerHTML = `
    <svg xmlns="http://www.w3.org/2000/svg" width="28" height="28" fill="currentColor" viewBox="0 0 16 16">
      <path d="M8 15c4.418 0 8-3.134 8-7s-3.582-7-8-7-8 3.134-8 7c0 1.76.743 3.37 1.97 4.6-.097 1.016-.417 2.13-.771 2.966-.079.186.074.394.273.362 2.256-.37 3.597-.938 4.18-1.234A9.06 9.06 0 0 0 8 15z"/>
    </svg>
  `;
  toggleBtn.title = 'Chat với AI';
  document.body.appendChild(toggleBtn);

  // Create chatbot container
  const container = document.createElement('div');
  container.className = 'chatbot-container chatbot-hidden';
  container.innerHTML = `
    <div class="chatbot-header">
      <div class="chatbot-header-left">
        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" viewBox="0 0 16 16">
          <path d="M8 15c4.418 0 8-3.134 8-7s-3.582-7-8-7-8 3.134-8 7c0 1.76.743 3.37 1.97 4.6-.097 1.016-.417 2.13-.771 2.966-.079.186.074.394.273.362 2.256-.37 3.597-.938 4.18-1.234A9.06 9.06 0 0 0 8 15z"/>
        </svg>
        <span class="chatbot-title">Tư vấn cây trồng</span>
      </div>
      <button class="chatbot-close" title="Đóng">
        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" viewBox="0 0 16 16">
          <path d="M2.146 2.854a.5.5 0 1 1 .708-.708L8 7.293l5.146-5.147a.5.5 0 0 1 .708.708L8.707 8l5.147 5.146a.5.5 0 0 1-.708.708L8 8.707l-5.146 5.147a.5.5 0 0 1-.708-.708L7.293 8 2.146 2.854Z"/>
        </svg>
      </button>
    </div>
    <div class="chatbot-messages" id="chatbotMessages">
      <div class="chatbot-welcome">
        <div class="chatbot-welcome-icon">🌱</div>
        <p>Xin chào! Tôi là trợ lý AI về cây trồng.</p>
        <p>Hãy hỏi tôi về bất kỳ loại cây nào bạn quan tâm!</p>
      </div>
    </div>
    <form class="chatbot-input" autocomplete="off">
      <input type="text" id="chatbotInput" placeholder="Nhập câu hỏi..." autocomplete="off" />
      <button type="submit">
        <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" viewBox="0 0 16 16">
          <path d="M15.854.146a.5.5 0 0 1 .11.54l-5.819 14.547a.75.75 0 0 1-1.329.124l-3.178-4.995L.643 7.184a.75.75 0 0 1 .124-1.33L15.314.037a.5.5 0 0 1 .54.11ZM6.636 10.07l2.761 4.338L14.13 2.576 6.636 10.07Zm6.787-8.201L1.591 6.602l4.339 2.76 7.494-7.493Z"/>
        </svg>
      </button>
    </form>
  `;
  document.body.appendChild(container);

  // Toggle button functionality
  toggleBtn.onclick = function() {
    container.classList.toggle('chatbot-hidden');
    toggleBtn.classList.toggle('chatbot-toggle-active');
  };

  // Close button
  container.querySelector('.chatbot-close').onclick = function() {
    container.classList.add('chatbot-hidden');
    toggleBtn.classList.remove('chatbot-toggle-active');
  };

  // Message rendering with animation and max length
  function addMessage(text, isUser) {
    const messages = container.querySelector('#chatbotMessages');
    const msgDiv = document.createElement('div');
    msgDiv.className = 'chatbot-message' + (isUser ? ' user' : ' bot');
    msgDiv.style.opacity = '0';
    msgDiv.style.transform = 'translateY(10px)';
    
    // Giới hạn độ dài tin nhắn hiển thị
    let displayText = text;
    if (text.length > 600) {
      displayText = text.substring(0, 600) + '...';
    }
    msgDiv.innerHTML = `<div class="bubble">${displayText}</div>`;
    messages.appendChild(msgDiv);
    
    // Animation fade in
    setTimeout(() => {
      msgDiv.style.transition = 'opacity 0.3s, transform 0.3s';
      msgDiv.style.opacity = '1';
      msgDiv.style.transform = 'translateY(0)';
    }, 10);
    
    setTimeout(() => {
      messages.scrollTop = messages.scrollHeight;
    }, 100);
  }

  // Handle form submit
  container.querySelector('.chatbot-input').onsubmit = function(e) {
    e.preventDefault();
    const input = container.querySelector('#chatbotInput');
    const text = input.value.trim();
    if (!text) return;
    addMessage(text, true);
    input.value = '';
    // Hiệu ứng loading dấu chấm động
    const loadingDiv = document.createElement('div');
    loadingDiv.className = 'chatbot-message';
    loadingDiv.innerHTML = '<div class="bubble"><span class="chatbot-loading-dots"><span></span><span></span><span></span></span></div>';
    const messages = container.querySelector('#chatbotMessages');
    messages.appendChild(loadingDiv);
    setTimeout(() => { messages.scrollTop = messages.scrollHeight; }, 100);

    fetch('/api/GeminiChat', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ message: text })
    })
    .then(res => res.json())
    .then(data => {
      messages.removeChild(loadingDiv);
      if (data.reply) {
        addMessage(data.reply, false);
      } else {
        addMessage('Không nhận được phản hồi từ AI.', false);
      }
    })
    .catch(() => {
      messages.removeChild(loadingDiv);
      addMessage('Lỗi kết nối đến AI.', false);
    });
  };
})();
