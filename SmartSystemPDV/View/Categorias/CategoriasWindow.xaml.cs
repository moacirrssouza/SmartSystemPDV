using Microsoft.Extensions.DependencyInjection;
using SmartSystemPDV.ViewModels;
using System.Windows;

namespace SystemSmartPDV.View.Categorias
{
    public partial class CategoriasWindow : Window
    {
        public CategoriasWindow()
        {
            InitializeComponent();
            var app = (SystemSmartPDV.App)Application.Current;
            DataContext = app.Services.GetRequiredService<CategoriasViewModel>();
        }
    }
}
