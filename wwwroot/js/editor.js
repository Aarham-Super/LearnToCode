(function () {
  const codeInput = document.getElementById("editor-input");
  const tutorOutput = document.getElementById("tutor-output");
  const progressOutput = document.getElementById("progress-output");
  const languageSelect = document.getElementById("language-select");

  function getCode() {
    return window.learnToCode?.state?.editor?.getValue()
      || codeInput?.value
      || "";
  }

  async function callAI(action) {
    const code = getCode();
    const language = languageSelect?.value || "auto";

    const response = await fetch("/api/ai/analyze", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        code: code,
        action: action,
        languageHint: language
      })
    });

    if (!response.ok) {
      throw new Error("AI request failed");
    }

    return await response.json();
  }

  function bindToolbar() {
    document.querySelectorAll("[data-action]").forEach(button => {
      button.addEventListener("click", async () => {
        const action = button.getAttribute("data-action");

        try {
          const result = await callAI(action);

          if (tutorOutput) {
            tutorOutput.innerHTML = `
              <h3>${result.detectedLanguage}</h3>
              <p><strong>Explanation:</strong> ${result.explanation}</p>
              <p><strong>Why wrong:</strong> ${result.whyWrong}</p>
              <pre>${result.correctedCode}</pre>
            `;
          }

          if (progressOutput && result.tutorSummary) {
            progressOutput.innerHTML = result.tutorSummary;
          }

        } catch (error) {
          if (tutorOutput) {
            tutorOutput.innerHTML = `
              <strong>AI Error</strong>
              <p>${error.message}</p>
            `;
          }
        }
      });
    });
  }

  function initializeFallbackEditor() {
    if (!codeInput) return;

    codeInput.addEventListener("input", () => {
      if (window.learnToCode?.state) {
        learnToCode.state.lastEditedValue = codeInput.value;
      }
    });
  }

  function initializeMonaco() {
    if (!window.require || !document.getElementById("monaco-editor")) {
      initializeFallbackEditor();
      return;
    }

    require.config({
      paths: { vs: "https://cdn.jsdelivr.net/npm/monaco-editor@0.52.2/min/vs" }
    });

    require(["vs/editor/editor.main"], function () {
      const container = document.getElementById("monaco-editor");
      const initialValue = codeInput ? codeInput.value : "";

      window.learnToCode = window.learnToCode || {};
      window.learnToCode.state = window.learnToCode.state || {};

      learnToCode.state.editor = monaco.editor.create(container, {
        value: initialValue,
        language: "csharp",
        theme: "vs-dark",
        fontSize: 15,
        minimap: { enabled: false },
        automaticLayout: true,
        scrollBeyondLastLine: false
      });

      learnToCode.state.editor.onDidChangeModelContent(() => {
        if (codeInput) {
          codeInput.value = learnToCode.state.editor.getValue();
        }
      });
    });
  }

  function syncLanguageSelection() {
    if (!languageSelect) return;

    languageSelect.addEventListener("change", () => {
      const value = languageSelect.value.toLowerCase();

      if (window.monaco && learnToCode.state.editor) {
        monaco.editor.setModelLanguage(
          learnToCode.state.editor.getModel(),
          mapLanguage(value)
        );
      }
    });
  }

  function mapLanguage(value) {
    if (value.includes("html")) return "html";
    if (value.includes("css")) return "css";
    if (value.includes("javascript")) return "javascript";
    if (value.includes("python")) return "python";
    if (value === "c++") return "cpp";
    if (value === "c#") return "csharp";
    if (value === "f#") return "fsharp";
    if (value.includes("sql")) return "sql";
    if (value.includes("acl")) return "csharp";
    return "csharp";
  }

  bindToolbar();
  syncLanguageSelection();
  initializeMonaco();
})();