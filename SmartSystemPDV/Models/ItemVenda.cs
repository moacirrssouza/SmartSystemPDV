using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSystemPDV.Models;

/// <summary>
/// Entidade ItemVenda
/// </summary>
public class ItemVenda
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int VendaId { get; set; }

    [ForeignKey("VendaId")]
    public Venda Venda { get; set; }

    [Required]
    public int ProdutoId { get; set; }

    [ForeignKey("ProdutoId")]
    public Produto Produto { get; set; }

    [Required]
    public int Quantidade { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecoUnitario { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Desconto { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    // Propriedades adicionais para exibição
    [NotMapped]
    public string Codigo { get; set; }

    [NotMapped]
    public string Nome { get; set; }
}