using System.ComponentModel.DataAnnotations;

namespace SmartSystemPDV.Models;

// <summary>
/// Entidade Cliente
/// </summary>
public class Cliente
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; }

    [MaxLength(18)]
    public string CpfCnpj { get; set; }

    [MaxLength(100)]
    public string Email { get; set; }

    [MaxLength(20)]
    public string Telefone { get; set; }

    [MaxLength(200)]
    public string Endereco { get; set; }

    public bool Ativo { get; set; }

    public DateTime DataCadastro { get; set; }
}