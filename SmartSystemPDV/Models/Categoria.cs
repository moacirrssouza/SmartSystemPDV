using System.ComponentModel.DataAnnotations;

namespace SmartSystemPDV.Models;

/// <summary>
/// Entidade Categoria
/// </summary>
public class Categoria
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; }

    [MaxLength(300)]
    public string Descricao { get; set; }

    public bool Ativo { get; set; } = true;

    public virtual ICollection<Produto> Produtos { get; set; }
}