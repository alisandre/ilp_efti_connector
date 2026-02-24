namespace ilp_efti_connector.FormInputService.Models;

/// <summary>Risultato della validazione di un form di trasporto.</summary>
public sealed record FormValidationResult(
    bool                          IsValid,
    IReadOnlyList<ValidationError> Errors
);

/// <summary>Singolo errore di validazione.</summary>
public sealed record ValidationError(string Field, string Message);
