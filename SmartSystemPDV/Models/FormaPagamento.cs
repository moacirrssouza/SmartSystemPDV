using System.ComponentModel.DataAnnotations;

namespace SmartSystemPDV.Models;

/// <summary>
/// Entidade FormaPagamento
/// </summary>
public class FormaPagamento
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Nome { get; set; }

    public bool PermiteParcelas { get; set; }

    public bool Ativo { get; set; }
}
