window.learnToCode = window.learnToCode || {};

learnToCode.state = {
  userId: localStorage.getItem("ltc-user-id") || "default",
  learnMode: true,
  autoHelp: true,
  editor: null
};

// ================= GET CODE =================
learnToCode.getEditorValue = function () {
  if (learnToCode.state.editor?.getValue) {
    return learnToCode.state.editor.getValue();
  }

  const input = document.getElementById("editor-input");
  return input ? input.value : "";
};

// ================= SET CODE =================
learnToCode.setEditorValue = function (value) {
  if (learnToCode.state.editor?.setValue) {
    learnToCode.state.editor.setValue(value);
    return;
  }

  const input = document.getElementById("editor-input");
  if (input) input.value = value;
};

// ================= RENDER TUTOR =================
learnToCode.renderTutorResponse = function (response) {
  const tutor = document.getElementById("tutor-output");
  if (!tutor || !response) return;

  const suggestions = (response.suggestions || [])
    .map(x => `<li>${x}</li>`).join("");

  const steps = (response.lineByLine || [])
    .map(x => `<li>${x}</li>`).join("");

  tutor.innerHTML = `
    <strong>${response.detectedLanguage || "AI Tutor"}</strong>

    <p>${response.explanation || ""}</p>

    <p><strong>Why:</strong> ${response.whyWrong || "No issues found"}</p>

    <p><strong>Fixed Code:</strong></p>
    <pre class="code-snippet">${escapeHtml(response.correctedCode || "")}</pre>

    <div>
      <strong>Suggestions</strong>
      <ul>${suggestions || "<li>No suggestions</li>"}</ul>
    </div>

    <div>
      <strong>Step by step</strong>
      <ul>${steps || "<li>Ask for explanation</li>"}</ul>
    </div>
  `;
};

// ================= RENDER PROGRESS =================
learnToCode.renderProgress = function (progress) {
  const output = document.getElementById("progress-output");
  if (!output || !progress) return;

  output.innerHTML = `
    <strong>Submissions:</strong> ${progress.totalSubmissions || 0}<br/>
    <strong>Fixed:</strong> ${progress.fixedSubmissions || 0}<br/>
    <strong>Language:</strong> ${progress.lastLanguage || "Unknown"}
  `;
};

// ================= ANALYZE =================
learnToCode.analyze = async function (action) {
  const languageSelect = document.getElementById("language-select");

  const payload = {
    code: learnToCode.getEditorValue(),
    action: action || "explain",
    userId: learnToCode.state.userId,
    learnMode: learnToCode.state.learnMode,
    autoHelpEnabled: learnToCode.state.autoHelp,
    languageHint: languageSelect?.value || null
  };

  const tutor = document.getElementById("tutor-output");
  if (tutor) tutor.innerHTML = "<em>AI is thinking...</em>";

  const response = await fetch("/api/ai/analyze", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload)
  });

  if (!response.ok) {
    throw new Error(`Request failed: ${response.status}`);
  }

  const data = await response.json();

  learnToCode.renderTutorResponse(data);

  if (data.progress) {
    learnToCode.renderProgress(data.progress);
  }

  return data;
};

// ================= BUTTON EVENTS =================
document.querySelectorAll("[data-action]").forEach(btn => {
  btn.addEventListener("click", async () => {
    try {
      await learnToCode.analyze(btn.dataset.action);
    } catch (err) {
      document.getElementById("tutor-output").innerHTML =
        `<strong>Error:</strong> ${err.message}`;
    }
  });
});

// ================= UTIL =================
function escapeHtml(v) {
  return String(v)
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;");
}