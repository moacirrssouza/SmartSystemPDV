using SmartSystemPDV.View.CadastroProdutos;
using SmartSystemPDV.View.CadastroUsuarios;
using SmartSystemPDV.View.ControleEstoque;
using SmartSystemPDV.View.GerenciarPermissoes;
using SmartSystemPDV.View.Vendas;
using SmartSystemPDV.Commands;
using SystemSmartPDV.View.Categorias;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace SystemSmartPDV;

public partial class MainWindow : Window
{
    #region Propriedades Privadas

    private bool menuAberto = false;
    private DispatcherTimer timer;

    #endregion

    public ICommand OpenUsuariosCommand { get; }
    public ICommand OpenProdutosCommand { get; }
    public ICommand OpenCaixaCommand { get; }
    public ICommand OpenEstoqueCommand { get; }
    public ICommand OpenPermissoesCommand { get; }
    public ICommand OpenCategoriasCommand { get; }
    public ICommand SairCommand { get; }

    #region Construtor

    public MainWindow()
    {
            InitializeComponent();
            DataContext = this;
            OpenUsuariosCommand = new RelayCommand(_ => BtnMenuUsuarios_Click(this, new RoutedEventArgs()));
            OpenProdutosCommand = new RelayCommand(_ => BtnMenuProdutos_Click(this, new RoutedEventArgs()));
            OpenCaixaCommand = new RelayCommand(_ => BtnMenuCaixa_Click(this, new RoutedEventArgs()));
            OpenEstoqueCommand = new RelayCommand(_ => BtnMenuEstoque_Click(this, new RoutedEventArgs()));
            OpenPermissoesCommand = new RelayCommand(_ => BtnMenuPermissoes_Click(this, new RoutedEventArgs()));
            OpenCategoriasCommand = new RelayCommand(_ => BtnMenuCategorias_Click(this, new RoutedEventArgs()));
            SairCommand = new RelayCommand(_ => BtnSair_Click(this, new RoutedEventArgs()));
            InicializarTimer();
            AtualizarDataHora();
        }
    /// Abre o módulo de Categorias
    /// </summary>
    private void BtnMenuCategorias_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidarAberturaNovanela())
            return;

        try
        {
            var categoriasWindow = ((App)Application.Current).Services.GetRequiredService<CategoriasWindow>();
            AbrirJanela(categoriasWindow);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erro ao abrir módulo de Categorias: {ex.Message}",
                "Erro",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    #endregion

    #region Métodos de Inicialização

    /// <summary>
    /// Inicializa o timer para atualização de data e hora
    /// </summary>
    private void InicializarTimer()
    {
        timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(1);
        timer.Tick += Timer_Tick;
        timer.Start();
    }

    #endregion

    #region Métodos de Atualização

    /// <summary>
    /// Atualiza a data e hora exibida na interface
    /// </summary>
    private void AtualizarDataHora()
    {
        txtDataSistema.Text = DateTime.Now.ToString("dd/MM/yyyy");
        txtHorarioSistema.Text = DateTime.Now.ToString("HH:mm:ss");
    }

    #endregion

    #region Métodos de Animação

    /// <summary>
    /// Abre o menu lateral com animação
    /// </summary>
    private void AbrirMenu()
    {
        var storyboard = (Storyboard)this.FindResource("OpenMenu");
        storyboard.Begin();

        // Animar opacidade dos textos
        AnimarOpacidade(pnlLogo, 1.0);
        AnimarOpacidade(txtDashboard, 1.0);
        AnimarOpacidade(txtVendas, 1.0);
        AnimarOpacidade(txtCaixa, 1.0);
        AnimarOpacidade(txtProdutos, 1.0);
        AnimarOpacidade(txtCategorias, 1.0);
        AnimarOpacidade(txtEstoque, 1.0);
        AnimarOpacidade(txtClientes, 1.0);
        AnimarOpacidade(txtFornecedores, 1.0);
        AnimarOpacidade(txtRelatorios, 1.0);
        AnimarOpacidade(txtUsuarios, 1.0);
        AnimarOpacidade(txtPermissoes, 1.0);
        AnimarOpacidade(txtConfiguracoes, 1.0);
        AnimarOpacidade(pnlUsuario, 1.0);

        // Animar texto do botão sair
        AnimarTextoBotaoSair(1.0);

        menuAberto = true;
    }

    /// <summary>
    /// Fecha o menu lateral com animação
    /// </summary>
    private void FecharMenu()
    {
        var storyboard = (Storyboard)this.FindResource("CloseMenu");
        storyboard.Begin();

        // Animar opacidade dos textos
        AnimarOpacidade(pnlLogo, 0.0);
        AnimarOpacidade(txtDashboard, 0.0);
        AnimarOpacidade(txtVendas, 0.0);
        AnimarOpacidade(txtCaixa, 0.0);
        AnimarOpacidade(txtProdutos, 0.0);
        AnimarOpacidade(txtCategorias, 0.0);
        AnimarOpacidade(txtEstoque, 0.0);
        AnimarOpacidade(txtClientes, 0.0);
        AnimarOpacidade(txtFornecedores, 0.0);
        AnimarOpacidade(txtRelatorios, 0.0);
        AnimarOpacidade(txtUsuarios, 0.0);
        AnimarOpacidade(txtPermissoes, 0.0);
        AnimarOpacidade(txtConfiguracoes, 0.0);
        AnimarOpacidade(pnlUsuario, 0.0);

        // Animar texto do botão sair
        AnimarTextoBotaoSair(0.0);

        menuAberto = false;
    }

    /// <summary>
    /// Anima a opacidade de um elemento
    /// </summary>
    /// <param name="elemento">Elemento a ser animado</param>
    /// <param name="paraOpacidade">Valor final de opacidade (0.0 a 1.0)</param>
    private void AnimarOpacidade(FrameworkElement elemento, double paraOpacidade)
    {
        if (elemento == null) return;

        var animation = new DoubleAnimation
        {
            To = paraOpacidade,
            Duration = TimeSpan.FromMilliseconds(300),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };

        elemento.BeginAnimation(UIElement.OpacityProperty, animation);
    }

    /// <summary>
    /// Anima o texto do botão sair
    /// </summary>
    /// <param name="paraOpacidade">Valor final de opacidade (0.0 a 1.0)</param>
    private void AnimarTextoBotaoSair(double paraOpacidade)
    {
        try
        {
            // Encontrar o TextBlock "txtSair" dentro do template do botão
            var txtSairElement = FindVisualChild<TextBlock>(btnSair, "txtSair");
            if (txtSairElement != null)
            {
                AnimarOpacidade(txtSairElement, paraOpacidade);
            }
        }
        catch (Exception ex)
        {
            // Log do erro (opcional)
            System.Diagnostics.Debug.WriteLine($"Erro ao animar botão sair: {ex.Message}");
        }
    }

    #endregion

    #region Métodos Auxiliares

    /// <summary>
    /// Encontra um elemento filho na árvore visual
    /// </summary>
    /// <typeparam name="T">Tipo do elemento a ser encontrado</typeparam>
    /// <param name="parent">Elemento pai</param>
    /// <param name="childName">Nome do elemento filho</param>
    /// <returns>Elemento encontrado ou null</returns>
    private T FindVisualChild<T>(DependencyObject parent, string childName) where T : DependencyObject
    {
        if (parent == null) return null;

        int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childrenCount; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);

            if (child is T typedChild && (child as FrameworkElement)?.Name == childName)
            {
                return typedChild;
            }

            T childOfChild = FindVisualChild<T>(child, childName);
            if (childOfChild != null)
            {
                return childOfChild;
            }
        }
        return null;
    }

    /// <summary>
    /// Abre uma janela de forma centralizada
    /// </summary>
    /// <param name="window">Janela a ser aberta</param>
    private void AbrirJanela(Window window)
    {
        if (window != null)
        {
            window.Owner = this;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            window.Show();
        }
    }

    #endregion

    #region Validações

    /// <summary>
    /// Valida se o menu pode ser alternado
    /// </summary>
    /// <returns>True se pode alternar, False caso contrário</returns>
    private bool ValidarAlternanciaMenu()
    {
        // Adicione validações personalizadas se necessário
        return true;
    }

    /// <summary>
    /// Valida se uma nova janela pode ser aberta
    /// </summary>
    /// <returns>True se pode abrir, False caso contrário</returns>
    private bool ValidarAberturaNovanela()
    {
        // Validar se há permissões, conexão com banco, etc.
        return true;
    }

    #endregion

    #region Eventos do Timer

    /// <summary>
    /// Evento disparado a cada tick do timer
    /// </summary>
    private void Timer_Tick(object sender, EventArgs e)
    {
        AtualizarDataHora();
    }

    #endregion

    #region Eventos do Menu - Toggle

    /// <summary>
    /// Evento de clique no botão de alternar menu
    /// </summary>
    private void BtnToggleMenu_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidarAlternanciaMenu())
            return;

        if (menuAberto)
        {
            FecharMenu();
        }
        else
        {
            AbrirMenu();
        }
    }

    #endregion

    #region Eventos do Menu - Navegação

    /// <summary>
    /// Abre o módulo de Vendas
    /// </summary>
    private void BtnMenuVendas_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidarAberturaNovanela())
            return;

        try
        {
            var vendaWindow = ((App)Application.Current).Services.GetRequiredService<VendaWindow>();
            AbrirJanela(vendaWindow);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erro ao abrir módulo de Vendas: {ex.Message}",
                "Erro",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Abre o módulo de Caixa (PDV)
    /// </summary>
    private void BtnMenuCaixa_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidarAberturaNovanela())
            return;

        try
        {
            var caixaWindow = ((App)Application.Current).Services.GetRequiredService<SmartSystemPDV.View.Caixa.CaixaWindow>();
            AbrirJanela(caixaWindow);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erro ao abrir módulo de Caixa: {ex.Message}",
                "Erro",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Abre o módulo de Produtos
    /// </summary>
    private void BtnMenuProdutos_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidarAberturaNovanela())
            return;

        try
        {
            var cadastroProdutosWindow = ((App)Application.Current).Services.GetRequiredService<CadastroProdutosWindow>();
            AbrirJanela(cadastroProdutosWindow);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erro ao abrir módulo de Produtos: {ex.Message}",
                "Erro",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Abre o módulo de Estoque
    /// </summary>
    private void BtnMenuEstoque_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidarAberturaNovanela())
            return;

        try
        {
            var controleEstoqueWindow = ((App)Application.Current).Services.GetRequiredService<ControleEstoqueWindow>();
            AbrirJanela(controleEstoqueWindow);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erro ao abrir módulo de Estoque: {ex.Message}",
                "Erro",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Abre o módulo de Usuários
    /// </summary>
    private void BtnMenuUsuarios_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidarAberturaNovanela())
            return;

        try
        {
            var cadastroUsuariosWindow = ((App)Application.Current).Services.GetRequiredService<CadastroUsuariosWindow>();
            AbrirJanela(cadastroUsuariosWindow);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erro ao abrir módulo de Usuários: {ex.Message}",
                "Erro",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// Abre o módulo de Permissões
    /// </summary>
    private void BtnMenuPermissoes_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidarAberturaNovanela())
            return;

        try
        {
            var gerenciarPermissoesWindow = ((App)Application.Current).Services.GetRequiredService<GerenciarPermissoesWindow>();
            AbrirJanela(gerenciarPermissoesWindow);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Erro ao abrir módulo de Permissões: {ex.Message}",
                "Erro",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    #endregion

    #region Eventos dos Cards de Atalho

    /// <summary>
    /// Atalho para abrir o módulo de Vendas
    /// </summary>
    private void Border_MouseLeftButtonDown_Vendas(object sender, MouseButtonEventArgs e)
    {
        BtnMenuVendas_Click(sender, e);
    }

    /// <summary>
    /// Atalho para abrir o módulo de Produtos
    /// </summary>
    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        BtnMenuProdutos_Click(sender, e);
    }

    /// <summary>
    /// Atalho para abrir o módulo de Estoque
    /// </summary>
    private void Border_MouseLeftButtonDown_Estoque(object sender, MouseButtonEventArgs e)
    {
        BtnMenuEstoque_Click(sender, e);
    }

    /// <summary>
    /// Atalho para abrir o módulo de Usuários
    /// </summary>
    private void Border_MouseLeftButtonDown_Usuarios(object sender, MouseButtonEventArgs e)
    {
        BtnMenuUsuarios_Click(sender, e);
    }

    /// <summary>
    /// Atalho para abrir o módulo de Permissões
    /// </summary>
    private void Border_MouseLeftButtonDown_Permissoes(object sender, MouseButtonEventArgs e)
    {
        BtnMenuPermissoes_Click(sender, e);
    }

    #endregion

    #region Eventos de Sistema

    /// <summary>
    /// Evento de clique no botão Sair
    /// </summary>
    private void BtnSair_Click(object sender, RoutedEventArgs e)
    {
        var resultado = MessageBox.Show(
            "Deseja realmente sair do sistema?",
            "Confirmar Saída",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (resultado == MessageBoxResult.Yes)
        {
            EncerrarSistema();
        }
    }

    /// <summary>
    /// Evento disparado quando a janela é fechada
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        EncerrarSistema();
        base.OnClosed(e);
    }

    #endregion

    #region Métodos de Encerramento

    /// <summary>
    /// Encerra o sistema de forma segura
    /// </summary>
    private void EncerrarSistema()
    {
        try
        {
            // Parar o timer
            timer?.Stop();

            // Liberar recursos (adicione conforme necessário)
            // Exemplo: fechar conexões, salvar configurações, etc.

            // Encerrar aplicação
            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao encerrar sistema: {ex.Message}");
        }
    }

    #endregion
}
