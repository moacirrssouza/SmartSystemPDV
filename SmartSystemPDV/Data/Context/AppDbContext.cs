using SmartSystemPDV.Models;
using Microsoft.EntityFrameworkCore;

namespace SmartSystemPDV.Data.Context;

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

            entity.Property(e => e.Codigo)
                .IsRequired();

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Descricao)
                .HasMaxLength(500);

            entity.Property(e => e.Categoria)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Unidade)
                .IsRequired()
                .HasMaxLength(10);

            entity.Property(e => e.CodigoBarras)
                .HasMaxLength(50);

            entity.Property(e => e.Lote)
                .HasMaxLength(30);

            entity.Property(e => e.PrecoCusto)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.PrecoVenda)
                .IsRequired()
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.Estoque)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(e => e.EstoqueAtual)
                .HasDefaultValue(0);

            entity.Property(e => e.EstoqueMinimo)
                .HasDefaultValue(0);

            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.DataCadastro)
                .HasDefaultValueSql("GETDATE()");

            entity.Property(e => e.Ativo)
                .HasDefaultValue(true);

            entity.HasIndex(e => e.Codigo)
                .IsUnique();

            entity.HasOne(p => p.CategoriaNavigation)
                .WithMany(c => c.Produtos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configurações de Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.CpfCnpj)
                .HasMaxLength(18);

            entity.Property(e => e.Email)
                .HasMaxLength(100);

            entity.Property(e => e.Telefone)
                .HasMaxLength(20);

            entity.Property(e => e.Endereco)
                .HasMaxLength(200);

            entity.Property(e => e.Ativo)
                .HasDefaultValue(true);

            entity.Property(e => e.DataCadastro)
                .HasDefaultValueSql("GETDATE()");

            entity.HasIndex(e => e.CpfCnpj)
                .IsUnique()
                .HasFilter("[CpfCnpj] IS NOT NULL");
        });

        // Configurações de Venda
        modelBuilder.Entity<Venda>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.NumeroVenda)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.DataVenda)
                .HasDefaultValueSql("GETDATE()");

            entity.Property(e => e.UsuarioId)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.ValorTotal)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.ValorDesconto)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.ValorFinal)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.ValorPago)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.Troco)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.FormaPagamento)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.NumeroParcelas)
                .HasDefaultValue(0);

            entity.Property(e => e.Status)
                .HasMaxLength(20);

            entity.Property(e => e.Observacoes)
                .HasMaxLength(500);

            entity.Property(e => e.MotivoCancelamento)
                .HasMaxLength(500);

            entity.HasOne(e => e.Cliente)
                .WithMany()
                .HasForeignKey(e => e.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Itens)
                .WithOne(i => i.Venda)
                .HasForeignKey(i => i.VendaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.NumeroVenda)
                .IsUnique();
        });


        // Configurações de ItemVenda
        modelBuilder.Entity<ItemVenda>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ProdutoNome)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Quantidade)
                .HasColumnType("decimal(18,3)")
                .HasDefaultValue(0);

            entity.Property(e => e.PrecoUnitario)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.Desconto)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.Subtotal)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.Sequencia)
                .HasDefaultValue(1);

            entity.HasOne(e => e.Venda)
                .WithMany(v => v.Itens)
                .HasForeignKey(e => e.VendaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Produto)
                .WithMany()
                .HasForeignKey(e => e.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.VendaId, e.Sequencia })
                .IsUnique();
        });


        // Configurações de MovimentacaoEstoque
        modelBuilder.Entity<MovimentacaoEstoque>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.DataHora)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            entity.Property(e => e.Tipo)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Quantidade)
                .IsRequired();

            entity.Property(e => e.QuantidadeAnterior)
                .IsRequired();

            entity.Property(e => e.QuantidadeFinal)
                .IsRequired();

            entity.Property(e => e.Motivo)
                .HasMaxLength(500);

            entity.HasOne(e => e.Produto)
                .WithMany()
                .HasForeignKey(e => e.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Usuario)
                .WithMany()
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(e => e.NomeProduto);

            entity.HasIndex(e => e.DataHora);
            entity.HasIndex(e => e.ProdutoId);
        });


        // Configurações de Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Login)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Senha)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Email)
                .HasMaxLength(100);

            entity.Property(e => e.Perfil)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Ativo)
                .HasDefaultValue(true);

            entity.Property(e => e.DataCadastro)
                .HasDefaultValueSql("GETDATE()");

            entity.HasIndex(e => e.Login)
                .IsUnique();
        });


        // Configurações de FormaPagamento
        modelBuilder.Entity<FormaPagamento>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Tipo)
                .HasMaxLength(20);

            entity.Property(e => e.PermiteParcelas)
                .HasDefaultValue(false);

            entity.Property(e => e.MaxParcelas)
                .HasDefaultValue(0);

            entity.Property(e => e.TaxaJuros)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);

            entity.Property(e => e.Ativo)
                .HasDefaultValue(true);

            entity.Property(e => e.Icone)
                .HasMaxLength(50);
        });


        // Configurações de Categoria
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Descricao)
                .HasMaxLength(300);

            entity.Property(e => e.Ativo)
                .HasDefaultValue(true);
        });
    }
}