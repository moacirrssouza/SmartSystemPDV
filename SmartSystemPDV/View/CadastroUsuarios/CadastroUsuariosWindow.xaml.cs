using SmartSystemPDV.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace SmartSystemPDV.View.CadastroUsuarios
{
    public partial class CadastroUsuariosWindow : Window
    {
        public CadastroUsuariosWindow(CadastroUsuariosViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private void TxtSenha_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is CadastroUsuariosViewModel viewModel)
            {
                viewModel.Senha = ((PasswordBox)sender).Password;
            }
        }

        private void TxtConfirmarSenha_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is CadastroUsuariosViewModel viewModel)
            {
                viewModel.ConfirmacaoSenha = ((PasswordBox)sender).Password;
            }
        }

        private void BtnMostrarSenha_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSenha.Password))
            {
                MessageBox.Show($"Senha: {txtSenha.Password}", "Visualizar Senha",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
