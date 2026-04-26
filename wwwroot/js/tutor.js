(function () {
  const sendButton = document.getElementById("send-question");
  const input = document.getElementById("tutor-question");
  const messages = document.getElementById("chat-messages");

  async function askTutor(question) {
    const code = question.includes("code") ? question : learnToCode.getEditorValue();
    const payload = {
      code,
      action: "tutor",
      userId: learnToCode.state.userId,
      learnMode: true,
      autoHelpEnabled: true,
      topic: question,
      languageHint: guessLanguage(question)
    };

    const response = await fetch("/api/ai/analyze", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify(payload)
    });

    if (!response.ok) {
      throw new Error(`Tutor request failed with status ${response.status}`);
    }

    return await response.json();
  }

  function appendMessage(text, role) {
    if (!messages) {
      return;
    }

    const bubble = document.createElement("div");
    bubble.className = `chat-bubble chat-bubble--${role}`;
    bubble.textContent = text;
    messages.appendChild(bubble);
    messages.scrollTop = messages.scrollHeight;
  }

  function guessLanguage(question) {
    const text = question.toLowerCase();
    if (text.includes("sql")) return "sql";
    if (text.includes("acl")) return "acl";
    if (text.includes("html")) return "html";
    if (text.includes("css")) return "css";
    if (text.includes("python")) return "python";
    if (text.includes("javascript") || text.includes("js")) return "javascript";
    if (text.includes("c++")) return "cpp";
    if (text.includes("c#")) return "csharp";
    if (text.includes("f#")) return "fsharp";
    return "";
  }

  if (sendButton && input) {
    sendButton.addEventListener("click", async () => {
      const question = input.value.trim();
      if (!question) {
        return;
      }

      appendMessage(question, "user");
      input.value = "";

      try {
        const data = await askTutor(question);
        appendMessage(data.explanation || data.tutorSummary || "I am ready to help.", "assistant");
        if (data.progress) {
          learnToCode.renderProgress(data.progress);
        }
      } catch (error) {
        appendMessage(error.message, "assistant");
      }
    });
  }
})();
