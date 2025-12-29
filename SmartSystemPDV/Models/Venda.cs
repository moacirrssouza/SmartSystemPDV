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

    [Required]
    public DateTime DataVenda { get; set; }

    [Required]
    public int ClienteId { get; set; }

    [ForeignKey("ClienteId")]
    public Cliente Cliente { get; set; }

    [Required]
    public int UsuarioId { get; set; }

    [ForeignKey("UsuarioId")]
    public Usuario Usuario { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorDesconto { get; set; }

    [Required]
    [MaxLength(50)]
    public string FormaPagamento { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorPago { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Troco { get; set; }

    public int? NumeroParcelas { get; set; }

    [MaxLength(20)]
    public string Status { get; set; }

    [MaxLength(500)]
    public string Observacoes { get; set; }

    // Navegação
    public ICollection<ItemVenda> Itens { get; set; }
}