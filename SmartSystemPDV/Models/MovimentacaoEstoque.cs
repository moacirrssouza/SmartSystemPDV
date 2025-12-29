using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSystemPDV.Models;

/// <summary>
/// Entidade MovimentacaoEstoque
/// </summary>
public class MovimentacaoEstoque
{
    [Key]
    public int Id { get; set; }

    [Required]
    public DateTime DataHora { get; set; }

    [Required]
    [MaxLength(50)]
    public string Tipo { get; set; }

    [Required]
    public int ProdutoId { get; set; }

    [ForeignKey("ProdutoId")]
    public Produto Produto { get; set; }

    [Required]
    public int Quantidade { get; set; }

    [Required]
    public int QuantidadeAnterior { get; set; }

    [Required]
    public int QuantidadeFinal { get; set; }

    [MaxLength(500)]
    public string Motivo { get; set; }

    public int? UsuarioId { get; set; }

    [ForeignKey("UsuarioId")]
    public Usuario Usuario { get; set; }

    // Propriedade calculada
    [NotMapped]
    public string NomeProduto { get; set; }
}