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

    // ---- 3. Pedir confirmação antes de apagar/cancelar ----
    document.querySelectorAll("form[data-confirmar]").forEach(function (form) {
        form.addEventListener("submit", function (e) {
            if (!confirm(form.dataset.confirmar)) e.preventDefault();
        });
    });
});
