using System.ComponentModel.DataAnnotations;

namespace SmartSystemPDV.Models;


/// <summary>
/// Entidade Usuario
/// </summary>
public class Usuario
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; }

    [Required]
    [MaxLength(50)]
    public string Login { get; set; }

    [Required]
    [MaxLength(255)]
    public string Senha { get; set; }

    [MaxLength(100)]
    public string Email { get; set; }

    [Required]
    [MaxLength(50)]
    public string Perfil { get; set; }

    public bool Ativo { get; set; }

    public DateTime DataCadastro { get; set; }
}