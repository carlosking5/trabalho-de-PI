// =========================================================
// CineReserva - pequenos comportamentos das páginas
// =========================================================

document.addEventListener("DOMContentLoaded", function () {

    // ---- 1. Seletor de quantidade (+/-) e total automático na página de reserva ----
    const caixa = document.querySelector("[data-quantidade]");
    if (caixa) {
        const campo = caixa.querySelector("input");
        const menos = caixa.querySelector("[data-menos]");
        const mais = caixa.querySelector("[data-mais]");
        const total = document.getElementById("total");
        const preco = parseFloat(caixa.dataset.preco);   // ex: 7.5
        const maximo = parseInt(caixa.dataset.maximo);   // lugares restantes

        function atualizar() {
            let q = parseInt(campo.value) || 1;
            q = Math.min(Math.max(q, 1), maximo);         // fica entre 1 e o máximo
            campo.value = q;
            menos.disabled = q <= 1;
            mais.disabled = q >= maximo;
            total.textContent = (q * preco).toLocaleString("pt-PT", { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " €";
        }

        menos.addEventListener("click", function () { campo.value = parseInt(campo.value) - 1; atualizar(); });
        mais.addEventListener("click", function () { campo.value = parseInt(campo.value) + 1; atualizar(); });
        campo.addEventListener("input", atualizar);
        atualizar();
    }

    // ---- 2. Pré-visualização do cartaz no formulário de filme ----
    const ficheiro = document.getElementById("FicheiroCartaz");
    const preVisualizacao = document.getElementById("preVisualizacao");
    const nomeFicheiro = document.getElementById("nomeFicheiro");
    if (ficheiro && preVisualizacao) {
        ficheiro.addEventListener("change", function () {
            const imagem = ficheiro.files[0];
            if (!imagem) return;
            preVisualizacao.src = URL.createObjectURL(imagem);   // mostra a imagem sem a enviar
            if (nomeFicheiro) nomeFicheiro.textContent = imagem.name;
        });
    }

    // ---- 3. Formulário de filme: campos de série (Temporada / Episódios) ----
    const tipo = document.getElementById("Tipo");
    const camposSerie = document.getElementById("camposSerie");
    if (tipo && camposSerie) {
        const temporada = document.getElementById("Temporada");
        const inicial = document.getElementById("EpisodioInicial");
        const final = document.getElementById("EpisodioFinal");
        const campos = [temporada, inicial, final];
        const maximo = parseInt(camposSerie.dataset.maxEpisodios); // 3

        const ehSerie = () => tipo.value === "Serie";
        const mensagemDe = (campo) => document.querySelector('[data-valmsg-for="' + campo.name + '"]');

        function limparErros() {
            campos.forEach(function (c) {
                c.classList.remove("is-invalid");
                const m = mensagemDe(c);
                if (m) m.textContent = "";
            });
        }

        function erro(campo, texto) {
            campo.classList.add("is-invalid");
            const m = mensagemDe(campo);
            if (m) m.textContent = texto;
        }

        // Mesmas regras que o servidor (FilmeFormViewModel.ErrosSerie)
        function validarEpisodios() {
            limparErros();
            if (!ehSerie()) return true;

            const t = parseInt(temporada.value), a = parseInt(inicial.value), b = parseInt(final.value);
            let valido = true;

            if (!(t >= 1)) { erro(temporada, "Indique a temporada (número maior que 0)."); valido = false; }
            if (!(a >= 1)) { erro(inicial, "Indique o episódio inicial (número maior que 0)."); valido = false; }
            if (!(b >= 1)) { erro(final, "Indique o episódio final (número maior que 0)."); valido = false; }

            if (a >= 1 && b >= 1) {
                if (b < a) { erro(final, "O episódio final tem de ser maior ou igual ao inicial."); valido = false; }
                else if (b - a + 1 > maximo) { erro(final, "Uma sessão pode ter no máximo 3 episódios."); valido = false; }
            }
            return valido;
        }

        // Mostra/esconde sem recarregar. Campos "disabled" não são enviados, por isso nos filmes chegam vazios.
        function alternarCamposSerie() {
            const serie = ehSerie();
            camposSerie.classList.toggle("d-none", !serie);
            campos.forEach(function (c) { c.disabled = !serie; });
            if (!serie) limparErros();
        }

        tipo.addEventListener("change", alternarCamposSerie);
        campos.forEach(function (c) { c.addEventListener("input", validarEpisodios); });
        tipo.form.addEventListener("submit", function (e) {
            if (!validarEpisodios()) {
                e.preventDefault();
                camposSerie.scrollIntoView({ behavior: "smooth", block: "center" });
            }
        });
        alternarCamposSerie(); // estado inicial (mantém as mensagens do servidor, se houver)
    }

    // ---- 5. Ecrã de pagamento: mostrar os campos do método escolhido (Cartão / MB WAY) ----
    const formPagamento = document.getElementById("formPagamento");
    if (formPagamento) {
        const grupos = {
            Cartao: document.getElementById("camposCartao"),
            MbWay: document.getElementById("camposMbWay")
        };

        // Campos dentro de um fieldset "disabled" não são enviados: só segue o método escolhido
        function alternarMetodo() {
            const escolhido = formPagamento.querySelector("[data-metodo]:checked");
            Object.keys(grupos).forEach(function (nome) {
                const ativo = escolhido && escolhido.value === nome;
                grupos[nome].classList.toggle("d-none", !ativo);
                grupos[nome].disabled = !ativo;
            });
        }
        formPagamento.querySelectorAll("[data-metodo]").forEach(function (r) {
            r.addEventListener("change", function () {
                alternarMetodo();
                const primeiro = grupos[r.value].querySelector("input");
                if (primeiro) primeiro.focus();
            });
        });
        alternarMetodo();

        // Número do cartão em grupos de 4: "4111 1111 1111 1111"
        const numero = formPagamento.querySelector('[data-formato="cartao"]');
        if (numero) numero.addEventListener("input", function () {
            const digitos = numero.value.replace(/\D/g, "").slice(0, 16);
            numero.value = digitos.replace(/(\d{4})(?=\d)/g, "$1 ");
        });

        // Validade: escreve "0828" e fica "08/28"
        const validade = formPagamento.querySelector('[data-formato="validade"]');
        if (validade) validade.addEventListener("input", function (e) {
            let d = validade.value.replace(/\D/g, "").slice(0, 4);
            if (d.length > 2 || (d.length === 2 && e.inputType !== "deleteContentBackward")) d = d.slice(0, 2) + "/" + d.slice(2);
            validade.value = d;
        });

        // CVV: só dígitos, no máximo 3
        const cvv = document.getElementById("Cvv");
        if (cvv) cvv.addEventListener("input", function () { cvv.value = cvv.value.replace(/\D/g, "").slice(0, 3); });
    }

    // ---- 4. Pedir confirmação antes de apagar/cancelar ----
    document.querySelectorAll("form[data-confirmar]").forEach(function (form) {
        form.addEventListener("submit", function (e) {
            if (!confirm(form.dataset.confirmar)) e.preventDefault();
        });
    });
});
