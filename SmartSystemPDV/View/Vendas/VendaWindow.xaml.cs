using SmartSystemPDV.Data.Context;
using SmartSystemPDV.Data.UnitOfWork;
using SmartSystemPDV.ViewModels;
using System.Windows;

namespace SmartSystemPDV.View.Vendas;

public partial class VendaWindow : Window
{
    public VendaWindow()
    {
        InitializeComponent();
        
        // Configurar DataContext
        var context = new AppDbContext();
        var unitOfWork = new UnitOfWork(context);
        var viewModel = new VendaViewModel(unitOfWork);
        DataContext = viewModel;

        // Inicializar dados
        Loaded += async (s, e) => await ((VendaViewModel)DataContext).InitializeAsync();

        // Configurar eventos de foco para teclado numérico
        ConfigurarFoco(viewModel);
    }

    private void ConfigurarFoco(VendaViewModel viewModel)
    {
        txtValorPago.GotFocus += (s, e) => viewModel.DefinirFocoCommand.Execute("ValorPago");
        txtQuantidadeProduto.GotFocus += (s, e) => viewModel.DefinirFocoCommand.Execute("Quantidade");
        txtCodigoProduto.GotFocus += (s, e) => viewModel.DefinirFocoCommand.Execute("CodigoProduto");
        
        // Foco inicial
        txtCodigoProduto.Focus();
    }
}
