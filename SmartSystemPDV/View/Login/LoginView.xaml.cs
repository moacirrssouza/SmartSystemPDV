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

namespace SystemSmartPDV.View.Login
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        private bool isPasswordVisible = false;
        private string currentPassword = "";

        public LoginView()
        {
            InitializeComponent();

            // Permitir arrastar a janela
            this.MouseLeftButtonDown += (s, e) => DragMove();
        }

        private void BtnEntrar_Click(object sender, RoutedEventArgs e)
        {
            string usuario = txtUsuario.Text;
            string senha = txtSenha.Password;

            // Validação simples
            if (string.IsNullOrEmpty(usuario))
            {
                MessageBox.Show("Por favor, informe o usuário.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrEmpty(senha))
            {
                MessageBox.Show("Por favor, informe a senha.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Aqui você pode adicionar sua lógica de autenticação
            // Por exemplo, validar com banco de dados

            // Simulação de login bem-sucedido
            if (ValidarLogin(usuario, senha))
            {
                // Abre a MainWindow
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();

                // Fecha a tela de login
                this.Close();
            }
            else
            {
                MessageBox.Show("Usuário ou senha incorretos!", "Erro de Login",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarLogin(string usuario, string senha)
        {
            // Implemente aqui sua lógica de validação
            // Exemplo simples (NUNCA use isso em produção):
            return usuario == "sa" && senha == "123456";

            // Em produção, você deve:
            // 1. Validar contra um banco de dados
            // 2. Usar hash de senha (bcrypt, SHA256, etc)
            // 3. Implementar tentativas de login limitadas
            // 4. Registrar logs de tentativas
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            // Funcionalidade para mostrar/ocultar senha
            // Esta é uma implementação básica
            // Para implementação completa, você precisaria de um TextBox adicional
            isPasswordVisible = !isPasswordVisible;

            if (isPasswordVisible)
            {
                btnTogglePassword.Content = "👁‍🗨";
                MessageBox.Show("Senha: " + txtSenha.Password, "Visualizar Senha",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                btnTogglePassword.Content = "👁";
            }
        }
    }
}