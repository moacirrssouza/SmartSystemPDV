using Microsoft.EntityFrameworkCore;
using SmartSystemPDV.Models;
using System;

namespace SmartSystemPDV.Data.Context
{
    /// <summary>
    /// Contexto do banco de dados - Configurado para SQL Server
    /// </summary>
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets - Tabelas do banco de dados
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Venda> Vendas { get; set; }
        public DbSet<ItemVenda> ItensVenda { get; set; }
        public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<FormaPagamento> FormasPagamento { get; set; }
        public DbSet<Categoria> Categorias { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // ===== CONFIGURAÇÃO SQL SERVER =====
                // OPÇÃO 1: Autenticação Windows (Recomendado para rede local)
                //optionsBuilder.UseSqlServer(
                //    "Server=localhost;Database=SmartSystemPDV;Integrated Security=True;TrustServerCertificate=True;");

                // OPÇÃO 2: Autenticação SQL Server (com usuário e senha)
                // optionsBuilder.UseSqlServer(
                //     "Server=localhost;Database=SmartSystemPDV;User Id=sa;Password=SuaSenha123;TrustServerCertificate=True;");

                // OPÇÃO 3: SQL Server Express com nome da instância
                optionsBuilder.UseSqlServer(
                    "Server=.\\SQLEXPRESS;Database=SmartSystemPDV;Integrated Security=True;TrustServerCertificate=True;");

                // OPÇÃO 4: Servidor remoto
                // optionsBuilder.UseSqlServer(
                //     "Server=192.168.1.100,1433;Database=SmartSystemPDV;User Id=usuario;Password=senha;TrustServerCertificate=True;");

                // OPÇÃO 5: Azure SQL Database
                // optionsBuilder.UseSqlServer(
                //     "Server=tcp:seuservidor.database.windows.net,1433;Database=SmartSystemPDV;User Id=usuario;Password=senha;Encrypt=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações de Produto
            modelBuilder.Entity<Produto>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Codigo).IsRequired();
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Descricao).HasMaxLength(500);
                entity.Property(e => e.PrecoCusto).HasColumnType("decimal(18,2)");
                entity.Property(e => e.PrecoVenda).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DataCadastro).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.Codigo).IsUnique();
            });

            // Configurações de Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CpfCnpj).HasMaxLength(18);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.Telefone).HasMaxLength(20);
                entity.Property(e => e.Endereco).HasMaxLength(200);
                entity.Property(e => e.DataCadastro).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.CpfCnpj).IsUnique();
            });

            // Configurações de Venda
            modelBuilder.Entity<Venda>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.NumeroVenda).IsRequired();
                entity.Property(e => e.ValorTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ValorDesconto).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ValorPago).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Troco).HasColumnType("decimal(18,2)");
                entity.Property(e => e.DataVenda).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Cliente)
                    .WithMany()
                    .HasForeignKey(e => e.ClienteId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.NumeroVenda).IsUnique();
            });

            // Configurações de ItemVenda
            modelBuilder.Entity<ItemVenda>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PrecoUnitario).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Desconto).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Subtotal).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.Venda)
                    .WithMany(v => v.Itens)
                    .HasForeignKey(e => e.VendaId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Produto)
                    .WithMany()
                    .HasForeignKey(e => e.ProdutoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configurações de MovimentacaoEstoque
            modelBuilder.Entity<MovimentacaoEstoque>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Motivo).HasMaxLength(500);
                entity.Property(e => e.DataHora).HasDefaultValueSql("GETDATE()");

                entity.HasOne(e => e.Produto)
                    .WithMany()
                    .HasForeignKey(e => e.ProdutoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.UsuarioId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configurações de Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Login).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Senha).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Email).HasMaxLength(100);
                entity.Property(e => e.DataCadastro).HasDefaultValueSql("GETDATE()");

                entity.HasIndex(e => e.Login).IsUnique();
            });

            // Configurações de FormaPagamento
            modelBuilder.Entity<FormaPagamento>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PermiteParcelas).HasDefaultValue(false);
            });

            // Configurações de Categoria
            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Descricao).HasMaxLength(200);
            });

            // Dados iniciais (Seed Data) - Apenas se usar Migrations
            // Caso use o script SQL, comente esta seção
            // SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Categorias padrão
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nome = "Eletrônicos", Descricao = "Produtos eletrônicos", Ativo = true },
                new Categoria { Id = 2, Nome = "Alimentos", Descricao = "Produtos alimentícios", Ativo = true },
                new Categoria { Id = 3, Nome = "Bebidas", Descricao = "Bebidas diversas", Ativo = true },
                new Categoria { Id = 4, Nome = "Limpeza", Descricao = "Produtos de limpeza", Ativo = true },
                new Categoria { Id = 5, Nome = "Higiene", Descricao = "Produtos de higiene pessoal", Ativo = true },
                new Categoria { Id = 6, Nome = "Vestuário", Descricao = "Roupas e acessórios", Ativo = true },
                new Categoria { Id = 7, Nome = "Outros", Descricao = "Outros produtos", Ativo = true }
            );

            // Formas de pagamento padrão
            modelBuilder.Entity<FormaPagamento>().HasData(
                new FormaPagamento { Id = 1, Nome = "Dinheiro", PermiteParcelas = false, Ativo = true },
                new FormaPagamento { Id = 2, Nome = "Cartão de Débito", PermiteParcelas = false, Ativo = true },
                new FormaPagamento { Id = 3, Nome = "Cartão de Crédito", PermiteParcelas = true, Ativo = true },
                new FormaPagamento { Id = 4, Nome = "PIX", PermiteParcelas = false, Ativo = true },
                new FormaPagamento { Id = 5, Nome = "Boleto", PermiteParcelas = false, Ativo = true },
                new FormaPagamento { Id = 6, Nome = "Crediário", PermiteParcelas = true, Ativo = true }
            );

            // Usuário administrador padrão
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    Nome = "Administrador",
                    Login = "admin",
                    Senha = "admin123", // Em produção, use hash de senha
                    Email = "admin@smartpdv.com",
                    Perfil = "Administrador",
                    Ativo = true,
                    DataCadastro = DateTime.Now
                }
            );

            // Cliente padrão
            modelBuilder.Entity<Cliente>().HasData(
                new Cliente
                {
                    Id = 1,
                    Nome = "Cliente Padrão",
                    CpfCnpj = "00000000000",
                    Email = "",
                    Telefone = "",
                    Endereco = "",
                    Ativo = true,
                    DataCadastro = DateTime.Now
                }
            );
        }
    }
}