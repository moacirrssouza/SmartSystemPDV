using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SmartSystemPDV.Data.Context;
using SmartSystemPDV.Data.UnitOfWork;
using SmartSystemPDV.ViewModels;
using SmartSystemPDV.View.CadastroProdutos;
using SmartSystemPDV.View.ControleEstoque;
using SmartSystemPDV.View.CadastroUsuarios;
using SmartSystemPDV.View.GerenciarPermissoes;
using SmartSystemPDV.View.Vendas;
using System.Windows;

namespace SystemSmartPDV
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public IServiceProvider Services { get; private set; } = default!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);

            // Database Context
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // ViewModels
            services.AddTransient<CadastroProdutosViewModel>();
            services.AddTransient<VendaViewModel>();
            services.AddTransient<ControleEstoqueViewModel>();
            services.AddTransient<LoginViewModel>();
            services.AddTransient<CategoriasViewModel>();
            services.AddTransient<CadastroUsuariosViewModel>();
            // Add other ViewModels here as we create them

            // Windows
            services.AddTransient<CadastroProdutosWindow>();
            services.AddTransient<ControleEstoqueWindow>();
            services.AddTransient<CadastroUsuariosWindow>();
            services.AddTransient<GerenciarPermissoesWindow>();
            services.AddTransient<VendaWindow>();
            services.AddTransient<MainWindow>();
            services.AddTransient<SystemSmartPDV.View.Categorias.CategoriasWindow>();
            services.AddTransient<SmartSystemPDV.View.Caixa.CaixaWindow>();

            Services = services.BuildServiceProvider();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (Services is IDisposable disposable)
            {
                disposable.Dispose();
            }

            base.OnExit(e);
        }
    }

}
