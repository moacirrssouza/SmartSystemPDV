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

    public int VendaId { get; set; }
    public virtual Venda Venda { get; set; }

    public int ProdutoId { get; set; }
    public virtual Produto Produto { get; set; }

    [Required]
    [MaxLength(200)]
    public string ProdutoNome { get; set; }

    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantidade { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecoUnitario { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Desconto { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    public int Sequencia { get; set; }
}