using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using SmartSystemPDV.Data.Context;
using SmartSystemPDV.Data.UnitOfWork;
using SmartSystemPDV.ViewModels;

namespace SmartSystemPDV.View.ControleEstoque;

public partial class ControleEstoqueWindow : Window
{
    private readonly ControleEstoqueViewModel _viewModel;
    private readonly AppDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public ControleEstoqueWindow()
    {
        InitializeComponent();
        
        // Setup de dependências
        _context = new AppDbContext();
        _unitOfWork = new UnitOfWork(_context);
        _viewModel = new ControleEstoqueViewModel(_unitOfWork);
        
        DataContext = _viewModel;
        
        Loaded += async (s, e) => await _viewModel.InitializeAsync();
        Unloaded += (s, e) => _context.Dispose();
    }

    private void BtnVoltar_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
    
    private void TextBox_PreviewTextInputNumeric(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex(@"^[0-9]+$");
        e.Handled = !regex.IsMatch(e.Text);
    }
}
