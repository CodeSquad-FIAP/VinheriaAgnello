namespace VinheriaAgnello.Domain.Enums;

/// <summary>
/// Tipo de movimentação de estoque.
/// <para><strong>Alinhamento Mobile:</strong> inteiro simples (0 = Entrada, 1 = Saída)
/// compatível com enumerações em Kotlin/Swift.</para>
/// </summary>
public enum TipoMovimentacao : int
{
    Entrada = 0,
    Saida = 1
}
