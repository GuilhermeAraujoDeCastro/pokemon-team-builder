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

  // ---------- abrir arquivo de time ----------
  var teamFile = document.getElementById("team-json-file");
  async function readTeamFile() {
    var status = document.getElementById("team-file-status");
    var file = teamFile.files[0];
    if (!file) return;
    if (file.size > 65536) { status.textContent = "O arquivo deve ter até 64 KiB."; return; }
    try {
      document.getElementById("team-json").value = await file.text();
      status.textContent = "Arquivo lido. Use Validar e visualizar antes de confirmar.";
    } catch { status.textContent = "Não foi possível ler este arquivo."; }
  }
  if (teamFile) {
    teamFile.addEventListener("change", readTeamFile);
    // O usuário pode escolher o arquivo antes de o script terminar de carregar.
    if (teamFile.files.length) readTeamFile();
  }

  document.querySelectorAll("[data-catalog-import]").forEach(function (form) {
    var busy = false;
    form.addEventListener("submit", async function (event) {
      event.preventDefault();
      if (busy) return;
      busy = true;
      var button = form.querySelector("button[type=submit]");
      var status = form.querySelector("[data-import-status]");
      button.disabled = true;
      status.textContent = "Consultando catálogo...";
      var abort = new AbortController();
      var timer = setTimeout(function () { abort.abort(); }, 15000);
      try {
        var response = await fetch(form.action, { method: "POST", body: new FormData(form), credentials: "same-origin", signal: abort.signal, headers: { Accept: "application/json" } });
        if (response.redirected) throw new Error("Sua sessão terminou. Entre novamente para importar.");
        if (!(response.headers.get("content-type") || "").includes("application/json")) throw new Error("O catálogo está indisponível agora.");
        var result = await response.json();
        if (!response.ok) throw new Error(result.error || "Não foi possível importar agora.");
        var pokemon = result.pokemon;
        var selects = Array.from(document.querySelectorAll('select[data-field="PokemonId"]'));
        var slotTemplate = document.getElementById("slot-template");
        if (slotTemplate) selects.push(...slotTemplate.content.querySelectorAll('select[data-field="PokemonId"]'));
        selects.forEach(function (select) {
          if (Array.from(select.options).some(function (option) { return Number(option.value) === pokemon.id; })) return;
          var option = document.createElement("option");
          option.value = String(pokemon.id);
          option.textContent = pokemon.name + " (" + pokemon.types.join("/") + ")";
          select.appendChild(option);
        });
        var picker = document.getElementById("team-picker");
        if (picker && !Array.from(picker.querySelectorAll("[data-id]")).some(function (item) { return Number(item.dataset.id) === pokemon.id; })) {
          var item = document.createElement("button");
          item.type = "button"; item.className = "roster-item";
          item.dataset.id = String(pokemon.id); item.dataset.rosterName = pokemon.name; item.dataset.rosterTypes = pokemon.types.join(" ");
          item.setAttribute("aria-pressed", "false");
          var number = document.createElement("span"); number.className = "slot-number";
          var label = document.createElement("span"); label.className = "d-block fw-semibold small"; label.textContent = pokemon.name;
          var types = document.createElement("span"); types.textContent = pokemon.types.join("/");
          item.append(number,label,types); picker.appendChild(item);
        }
        status.textContent = result.message;
      } catch (error) {
        status.textContent = (error.name === "AbortError" ? "A consulta demorou demais." : error.message) + " Os campos do time continuam preenchidos.";
      } finally {
        clearTimeout(timer); busy = false; button.disabled = false;
      }
    });
  });

  document.querySelectorAll("[data-copy]").forEach(function (button) {
    button.addEventListener("click", function () {
      var text = button.getAttribute("data-copy");
      var done = function () { button.textContent = "Link copiado!"; };
      if (navigator.clipboard) navigator.clipboard.writeText(text).then(done, function () { window.prompt("Copie o link:", text); });
      else window.prompt("Copie o link:", text);
    });
  });
})();
