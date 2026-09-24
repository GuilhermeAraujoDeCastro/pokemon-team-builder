// Scripts das telas: tema claro/escuro, montar o time na ordem dos cliques e editar os slots.
(function () {
  "use strict";

  // ---------- tema ----------
  var toggle = document.getElementById("theme-toggle");
  function syncIcon() {
    if (toggle) toggle.textContent = document.documentElement.getAttribute("data-bs-theme") === "dark" ? "☀" : "🌙";
  }
  if (toggle) {
    syncIcon();
    toggle.addEventListener("click", function () {
      var next = document.documentElement.getAttribute("data-bs-theme") === "dark" ? "light" : "dark";
      document.documentElement.setAttribute("data-bs-theme", next);
      try { localStorage.setItem("tbp-theme", next); } catch (e) { }
      syncIcon();
    });
  }

  // ---------- filtro de busca do catalogo (criar e editar) ----------
  var search = document.getElementById("roster-search");
  if (search) {
    search.addEventListener("input", function () {
      var term = search.value.trim().toLowerCase();
      document.querySelectorAll("[data-roster-name]").forEach(function (item) {
        var text = (item.getAttribute("data-roster-name") + " " + item.getAttribute("data-roster-types")).toLowerCase();
        item.hidden = term !== "" && text.indexOf(term) === -1;
      });
    });
  }

  // ---------- criar time: a ordem dos cliques vira a ordem dos slots ----------
  var picker = document.getElementById("team-picker");
  if (picker) {
    var max = Number(picker.getAttribute("data-max")) || 6;
    var hidden = document.getElementById("picked-inputs");
    var counter = document.getElementById("picked-count");
    var order = JSON.parse(picker.getAttribute("data-selected") || "[]").map(Number);

    function render() {
      hidden.innerHTML = "";
      order.forEach(function (id) {
        var input = document.createElement("input");
        input.type = "hidden";
        input.name = "pokemonIds";
        input.value = String(id);
        hidden.appendChild(input);
      });
      picker.querySelectorAll(".roster-item").forEach(function (item) {
        var position = order.indexOf(Number(item.getAttribute("data-id")));
        item.classList.toggle("selected", position !== -1);
        item.setAttribute("aria-pressed", String(position !== -1));
        item.querySelector(".slot-number").textContent = position === -1 ? "" : String(position + 1);
      });
      counter.textContent = order.length + "/" + max;
    }

    picker.addEventListener("click", function (event) {
      var item = event.target.closest(".roster-item");
      if (!item) return;
      var id = Number(item.getAttribute("data-id"));
      var position = order.indexOf(id);
      if (position !== -1) order.splice(position, 1);
      else if (order.length < max) order.push(id);
      render();
    });
    render();
  }

  // ---------- editar time: adicionar e remover linhas de slot ----------
  var slotsTable = document.getElementById("slots-body");
  if (slotsTable) {
    var template = document.getElementById("slot-template");
    var addBtn = document.getElementById("add-slot");
    var maxSlots = Number(slotsTable.getAttribute("data-max")) || 6;

    // Os nomes Slots[0].PokemonId... precisam ficar em sequencia pro model binding do ASP.NET.
    function renumber() {
      slotsTable.querySelectorAll("tr").forEach(function (row, index) {
        row.querySelector(".slot-index").textContent = String(index + 1);
        row.querySelectorAll("[data-field]").forEach(function (field) {
          field.name = "Slots[" + index + "]." + field.getAttribute("data-field");
        });
      });
      addBtn.disabled = slotsTable.querySelectorAll("tr").length >= maxSlots;
    }

    addBtn.addEventListener("click", function () {
      if (slotsTable.querySelectorAll("tr").length >= maxSlots) return;
      slotsTable.appendChild(template.content.firstElementChild.cloneNode(true));
      renumber();
    });
    slotsTable.addEventListener("click", function (event) {
      if (!event.target.closest(".remove-slot")) return;
      event.target.closest("tr").remove();
      renumber();
    });
    renumber();
  }

  // ---------- copiar link publico ----------
  document.querySelectorAll("[data-copy]").forEach(function (button) {
    button.addEventListener("click", function () {
      var text = button.getAttribute("data-copy");
      var done = function () { button.textContent = "Link copiado!"; };
      if (navigator.clipboard) navigator.clipboard.writeText(text).then(done, function () { window.prompt("Copie o link:", text); });
      else window.prompt("Copie o link:", text);
    });
  });
})();
