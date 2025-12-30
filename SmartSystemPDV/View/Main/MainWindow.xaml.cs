using SmartSystemPDV.View.CadastroUsuarios;
using SmartSystemPDV.View.GerenciarPermissoes;
using SmartSystemPDV.View.CadastroProdutos;
using SmartSystemPDV.View.ControleEstoque;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;
using SystemSmartPDV.View.Login;
using SmartSystemPDV.View.Vendas;

namespace SystemSmartPDV
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnSair_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Deseja realmente sair do sistema?",
                "Confirmar Saída",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                // Fecha a MainWindow e abre a tela de login novamente
                LoginView loginView = new LoginView();
                loginView.Show();
                this.Close();
            }
        }

        private void Border_MouseLeftButtonDown_Vendas(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Abre a tela de vendas
            VendaWindow stockControl = new VendaWindow();
            stockControl.ShowDialog();
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Abre a tela de cadastro de produtos
            CadastroProdutosWindow cadastroProdutos = new CadastroProdutosWindow();
            cadastroProdutos.ShowDialog();
        }

        private void Border_MouseLeftButtonDown_Estoque(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Abre a tela de controle de estoque
            ControleEstoqueWindow stockControl = new ControleEstoqueWindow();
            stockControl.ShowDialog();
        }

        private void Border_MouseLeftButtonDown_Usuarios(object sender, MouseButtonEventArgs e)
        {
            CadastroUsuariosWindow cadastroUsuarios = new CadastroUsuariosWindow();
            cadastroUsuarios.Show();
        }

        private void Border_MouseLeftButtonDown_Permissoes(object sender, MouseButtonEventArgs e)
        {
            GerenciarPermissoesWindow gerenciarPermissoes = new GerenciarPermissoesWindow();
            gerenciarPermissoes.Show();
        }
    }
}
