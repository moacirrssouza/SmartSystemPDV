using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSystemPDV.Models;

/// <summary>
/// Entidade Produto
/// </summary>
public class Produto
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int Codigo { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; }

    [MaxLength(500)]
    public string Descricao { get; set; }

    [Required]
    [MaxLength(50)]
    public string Categoria { get; set; }

    [Required]
    [MaxLength(20)]
    public string Unidade { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecoCusto { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecoVenda { get; set; }

    [Required]
    public int Estoque { get; set; }

    public int EstoqueMinimo { get; set; }

    [Required]
    [MaxLength(20)]
    public string Status { get; set; }

    public DateTime DataCadastro { get; set; }

    // Propriedades calculadas (não mapeadas)
    [NotMapped]
    public decimal ValorTotal => PrecoVenda * Estoque;

    [NotMapped]
    public string StatusEstoque
    {
        get
        {
            if (Estoque == 0) return "Crítico";
            if (Estoque <= EstoqueMinimo) return "Baixo";
            return "Normal";
        }
    }
}