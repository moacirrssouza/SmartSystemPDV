using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSystemPDV.Models;

/// <summary>
/// Entidade Venda
/// </summary>
public class Venda
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string NumeroVenda { get; set; }

    public DateTime DataVenda { get; set; } = DateTime.Now;

    public int? ClienteId { get; set; }
    public virtual Cliente Cliente { get; set; }

    [Required]
    [MaxLength(50)]
    public string UsuarioId { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorDesconto { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorFinal { get; set; }

    [Required]
    [MaxLength(50)]
    public string FormaPagamento { get; set; }

    public int? NumeroParcelas { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorPago { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Troco { get; set; }

    [MaxLength(20)]
    public string Status { get; set; }

    [MaxLength(500)]
    public string Observacoes { get; set; }

    public DateTime? DataCancelamento { get; set; }
    public string MotivoCancelamento { get; set; }

    public virtual ICollection<ItemVenda> Itens { get; set; }
}