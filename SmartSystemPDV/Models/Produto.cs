using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartSystemPDV.Models
{
    /// <summary>
    /// Modelo de Produto
    /// </summary>
    [Table("Produtos")]
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

        public int? CategoriaId { get; set; }

        public Categoria CategoriaNavigation { get; set; } 

        [Required]
        [MaxLength(10)]
        public string Unidade { get; set; }

        [MaxLength(50)]
        public string CodigoBarras { get; set; }

        [MaxLength(30)]
        public string Lote { get; set; }

        public DateTime? DataFabricacao { get; set; }

        public DateTime? DataVencimento { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoCusto { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrecoVenda { get; set; }

        [Required]
        public int Estoque { get; set; }

        public int EstoqueAtual { get; set; }

        public int EstoqueMinimo { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.Now;

        public DateTime? DataAtualizacao { get; set; }
        public bool Ativo { get; set; } = true;

        public ICollection<Categoria> Categorias { get; set; }

        [NotMapped]
        public decimal ValorTotal => Estoque * PrecoVenda;

        [NotMapped]
        public string StatusEstoque
        {
            get
            {
                if (Estoque <= 0) return "Crítico";
                if (Estoque <= EstoqueMinimo) return "Baixo";
                return "Normal";
            }
        }

        [NotMapped]
        public string NomeCompleto => $"{Codigo} - {Nome}";
    }
}