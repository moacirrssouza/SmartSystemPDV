using SmartSystemPDV.Models;
using SmartSystemPDV.View.GerenciarPermissoes;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace SmartSystemPDV.View.CadastroUsuarios
{
    public partial class CadastroUsuariosWindow : Window
    {
        private Dictionary<string, bool> permissoes;

        public CadastroUsuariosWindow()
        {
            InitializeComponent();
        }

        #region Eventos

        private void BtnNovo_Click(object sender, RoutedEventArgs e)
        {
            LimparCampos();
            txtId.Text = "";
            btnGerenciarPermissoes.IsEnabled = false;
            txtNome.Focus();
            FiltrarUsuarios();
        }

        private async void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos())
                return;
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            LimparCampos();
            txtId.Text = "";
            dgUsuarios.SelectedItem = null;
            btnEditar.IsEnabled = false;
            btnResetarSenha.IsEnabled = false;
            btnExcluir.IsEnabled = false;
            btnGerenciarPermissoes.IsEnabled = false;
            FiltrarUsuarios();
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void BtnExcluir_Click(object sender, RoutedEventArgs e)
        {
        }

        private async void BtnResetarSenha_Click(object sender, RoutedEventArgs e)
        {
        }

        private void BtnGerenciarPermissoes_Click(object sender, RoutedEventArgs e)
        {
        }

        private void DgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void TxtPesquisa_TextChanged(object sender, TextChangedEventArgs e)
        {
            FiltrarUsuarios();
        }

        private void CbFiltroPerfil_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FiltrarUsuarios();
        }

        private void CbFiltroStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FiltrarUsuarios();
        }

        private void FiltrarUsuarios()
        {
        }

        private void CbPerfil_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           
        }

        private void BtnMostrarSenha_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtSenha.Password))
            {
                MessageBox.Show($"Senha: {txtSenha.Password}", "Visualizar Senha",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnVoltar_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Deseja voltar para a tela principal?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                Close();
            }
        }

        #endregion

        #region Métodos

        private void LimparCampos()
        {
            txtNome.Clear();
            txtUsuario.Clear();
            txtEmail.Clear();
            txtSenha.Clear();
            txtConfirmarSenha.Clear();
            txtCargo.Clear();
            txtTelefone.Clear();
            cbDepartamento.SelectedIndex = 0;
            cbPerfil.SelectedIndex = 0;
            cbStatus.SelectedIndex = 0;
            txtDescricaoPerfil.Text = "";
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show("Por favor, informe o nome completo.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNome.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                MessageBox.Show("Por favor, informe o nome de usuário.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtUsuario.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Por favor, informe um email válido.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEmail.Focus();
                return false;
            }

           

            if (cbPerfil.SelectedIndex == 0)
            {
                MessageBox.Show("Por favor, selecione um perfil de acesso.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cbPerfil.Focus();
                return false;
            }

            return true;
        }

        private static bool IsValidEmail(string email)
        {
            const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        #endregion
    }
}