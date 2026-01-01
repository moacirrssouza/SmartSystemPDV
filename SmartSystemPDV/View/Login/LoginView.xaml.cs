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

namespace SystemSmartPDV.View.Login
{
    public partial class LoginView : Window
    {
        private bool isPasswordVisible = false;
        private LoginViewModel _viewModel;

        public LoginView()
        {
            InitializeComponent();

            this.MouseLeftButtonDown += (s, e) => DragMove();
            var app = (SystemSmartPDV.App)Application.Current;
            _viewModel = app.Services.GetRequiredService<LoginViewModel>();
            this.DataContext = _viewModel;
            _viewModel.LoginSucesso += OnLoginSucesso;

            isPasswordVisible = true;
            btnTogglePassword.Content = "👁‍🗨";
            txtSenhaVisivel.Visibility = Visibility.Visible;
            txtSenha.Visibility = Visibility.Collapsed;
            txtSenhaVisivel.Text = _viewModel.Senha ?? string.Empty;
        }

        private void OnLoginSucesso()
        {
            var app = (SystemSmartPDV.App)Application.Current;
            var main = app.Services.GetRequiredService<SystemSmartPDV.MainWindow>();
            main.Show();
            this.Close();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BtnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;

            if (isPasswordVisible)
            {
                btnTogglePassword.Content = "👁‍🗨";
                txtSenhaVisivel.Text = txtSenha.Password;
                txtSenha.Visibility = Visibility.Collapsed;
                txtSenhaVisivel.Visibility = Visibility.Visible;
            }
            else
            {
                btnTogglePassword.Content = "👁";
                txtSenha.Password = txtSenhaVisivel.Text;
                txtSenhaVisivel.Visibility = Visibility.Collapsed;
                txtSenha.Visibility = Visibility.Visible;
            }
        }

        private void TxtSenha_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.Senha = txtSenha.Password;
            }
        }
    }
}
