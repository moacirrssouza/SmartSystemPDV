using System.Windows;
using SmartSystemPDV.ViewModels;

namespace SmartSystemPDV.View.CadastroProdutos
{
    /// <summary>
    /// Janela de cadastro de produtos
    /// </summary>
    public partial class CadastroProdutosWindow : Window
    {
        public CadastroProdutosWindow(CadastroProdutosViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
