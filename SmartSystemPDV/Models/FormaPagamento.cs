using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSystemPDV.Models;

/// <summary>
/// Entidade FormaPagamento
/// </summary>
public class FormaPagamento
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; }

    [MaxLength(20)]
    public string Tipo { get; set; } // DINHEIRO, CARTAO_DEBITO, CARTAO_CREDITO, PIX, BOLETO, CREDIARIO

    public bool PermiteParcelas { get; set; }
    public int MaxParcelas { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxaJuros { get; set; }

    public bool Ativo { get; set; } = true;

    [MaxLength(50)]
    public string Icone { get; set; }
}