(function () {
  // ================= GLOBAL INIT =================
  console.log("LearnToCode site loaded");

  // ================= USER ID =================
  if (!localStorage.getItem("ltc-user-id")) {
    const id = "user-" + Math.random().toString(36).substring(2, 10);
    localStorage.setItem("ltc-user-id", id);
  }

  // ================= THEME SAFE HOOK (optional future) =================
  document.addEventListener("DOMContentLoaded", () => {
    const body = document.body;

    // You can expand later (dark/light mode etc.)
    body.classList.add("ltc-loaded");
  });

})();