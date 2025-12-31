using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using Microsoft.Extensions.DependencyInjection;
using SmartSystemPDV.ViewModels;
using SystemSmartPDV;

namespace SmartSystemPDV.View.Caixa
{
    /// <summary>
    /// Interaction logic for CaixaWindow.xaml
    /// </summary>
    public partial class CaixaWindow : Window
    {
        public CaixaWindow()
        {
            InitializeComponent();
            var app = (App)Application.Current;
            var vm = app.Services.GetRequiredService<VendaViewModel>();
            DataContext = vm;
            Loaded += async (s, e) => await vm.InitializeAsync();

            txtCodigoProduto.GotFocus += (s, e) => vm.DefinirFocoCommand.Execute("CodigoProduto");
            txtQuantidadeProduto.GotFocus += (s, e) => vm.DefinirFocoCommand.Execute("Quantidade");
            txtCodigoProduto.Loaded += (s, e) => txtCodigoProduto.Focus();
        }
    }
}
