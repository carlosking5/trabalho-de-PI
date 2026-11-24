using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CineReserva.Infraestrutura;

// Problema: o site está em português (pt-PT), onde o separador decimal é a vírgula,
// mas o browser envia os campos numéricos com ponto ("7.50").
// Este "binder" aceita os dois formatos: "7.50" e "7,50".
public class DecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext contexto)
    {
        var valor = contexto.ValueProvider.GetValue(contexto.ModelName).FirstValue;

        if (string.IsNullOrWhiteSpace(valor))
            return Task.CompletedTask; // o [Required] trata do campo vazio

        var normalizado = valor.Trim().Replace(" ", "").Replace(',', '.');

        if (decimal.TryParse(normalizado, NumberStyles.Number, CultureInfo.InvariantCulture, out var numero))
            contexto.Result = ModelBindingResult.Success(numero);
        else
            contexto.ModelState.TryAddModelError(contexto.ModelName, "Introduza um valor numérico válido (ex: 7,50).");

        return Task.CompletedTask;
    }
}

public class DecimalModelBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext contexto)
    {
        var tipo = contexto.Metadata.UnderlyingOrModelType;
        return tipo == typeof(decimal) ? new DecimalModelBinder() : null;
    }
}
