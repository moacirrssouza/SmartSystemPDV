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
    [MaxLength(50)]
    public string Nome { get; set; }

    [MaxLength(200)]
    public string Descricao { get; set; }

    public bool Ativo { get; set; }
}