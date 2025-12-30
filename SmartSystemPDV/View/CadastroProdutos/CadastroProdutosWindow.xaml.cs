using SmartSystemPDV.Data.Context;
using SmartSystemPDV.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartSystemPDV.View.CadastroProdutos
{
    /// <summary>
    /// Janela de cadastro de produtos
    /// </summary>
    public partial class CadastroProdutosWindow : Window
    {
        #region Propriedades Privadas

        private readonly AppDbContext _context;
        private Produto? _produtoSelecionado;

        // Propriedades de Paginação
        private int _paginaAtual = 1;
        private int _itensPorPagina = 20;
        private int _totalItens = 0;
        private int _totalPaginas = 0;
        private List<Produto> _todosOsProdutos = new List<Produto>();

        #endregion

        #region Construtor

        public CadastroProdutosWindow()
        {
            InitializeComponent();
            _context = new AppDbContext();
            CarregarProdutos();
        }

        #endregion

        #region Eventos de Produto

        private void BtnNovo_Click(object sender, RoutedEventArgs e)
        {
            LimparCampos();
            txtCodigo.Text = GerarProximoCodigo().ToString();
            _produtoSelecionado = null;
            btnSalvar.Content = "💾 Salvar";
            txtNome.Focus();
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos())
                return;

            try
            {
                if (_produtoSelecionado == null)
                {
                    var produto = new Produto
                    {
                        Codigo = int.Parse(txtCodigo.Text),
                        Nome = txtNome.Text.Trim(),
                        Descricao = txtDescricao.Text.Trim(),
                        Categoria = ((ComboBoxItem)cbCategoria.SelectedItem).Content.ToString(),
                        Unidade = ((ComboBoxItem)cbUnidade.SelectedItem).Content.ToString(),
                        PrecoCusto = decimal.Parse(txtPrecoCusto.Text, CultureInfo.CurrentCulture),
                        PrecoVenda = decimal.Parse(txtPrecoVenda.Text, CultureInfo.CurrentCulture),
                        Estoque = int.Parse(txtEstoque.Text),
                        EstoqueMinimo = string.IsNullOrWhiteSpace(txtEstoqueMinimo.Text)
                            ? 0
                            : int.Parse(txtEstoqueMinimo.Text),
                        Status = ((ComboBoxItem)cbStatus.SelectedItem).Content.ToString()
                    };

                    _context.Produtos.Add(produto);
                }
                else
                {
                    _produtoSelecionado.Nome = txtNome.Text.Trim();
                    _produtoSelecionado.Descricao = txtDescricao.Text.Trim();
                    _produtoSelecionado.Categoria = ((ComboBoxItem)cbCategoria.SelectedItem).Content.ToString();
                    _produtoSelecionado.Unidade = ((ComboBoxItem)cbUnidade.SelectedItem).Content.ToString();
                    _produtoSelecionado.PrecoCusto = decimal.Parse(txtPrecoCusto.Text, CultureInfo.CurrentCulture);
                    _produtoSelecionado.PrecoVenda = decimal.Parse(txtPrecoVenda.Text, CultureInfo.CurrentCulture);
                    _produtoSelecionado.Estoque = int.Parse(txtEstoque.Text);
                    _produtoSelecionado.EstoqueMinimo = string.IsNullOrWhiteSpace(txtEstoqueMinimo.Text)
                        ? 0
                        : int.Parse(txtEstoqueMinimo.Text);
                    _produtoSelecionado.Status = ((ComboBoxItem)cbStatus.SelectedItem).Content.ToString();
                }

                _context.SaveChanges();

                MessageBox.Show("Produto salvo com sucesso!", "Sucesso",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                LimparCampos();
                CarregarProdutos();
                _produtoSelecionado = null;
                btnSalvar.Content = "💾 Salvar";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar produto: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEditar_Click(object sender, RoutedEventArgs e)
        {
            if (dgProdutos.SelectedItem is Produto produto)
            {
                _produtoSelecionado = produto;
                PreencherCampos(produto);
                btnSalvar.Content = "💾 Atualizar";
            }
        }

        private void BtnExcluir_Click(object sender, RoutedEventArgs e)
        {
            if (dgProdutos.SelectedItem is Produto produto)
            {
                var result = MessageBox.Show(
                    $"Deseja realmente excluir o produto '{produto.Nome}'?",
                    "Confirmar Exclusão",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        _context.Produtos.Remove(produto);
                        _context.SaveChanges();

                        MessageBox.Show("Produto excluído com sucesso!", "Sucesso",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        CarregarProdutos();
                        LimparCampos();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Erro ao excluir produto: {ex.Message}", "Erro",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            LimparCampos();
            dgProdutos.SelectedItem = null;
            _produtoSelecionado = null;
            txtCodigo.Text = GerarProximoCodigo().ToString();
            btnSalvar.Content = "💾 Salvar";
        }

        private void DgProdutos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnEditar.IsEnabled = dgProdutos.SelectedItem != null;
            btnExcluir.IsEnabled = dgProdutos.SelectedItem != null;
        }

        private void BtnVoltar_Click(object sender, RoutedEventArgs e)
        {
           this.Close();
        }

        #endregion

        #region Eventos de Pesquisa

        private void BtnPesquisar_Click(object sender, RoutedEventArgs e)
        {
            _paginaAtual = 1; // Voltar para primeira página ao pesquisar
            FiltrarProdutos();
        }

        private void TxtPesquisa_TextChanged(object sender, TextChangedEventArgs e)
        {
            _paginaAtual = 1; // Voltar para primeira página ao digitar
            FiltrarProdutos();
        }

        #endregion

        #region Eventos de Paginação

        /// <summary>
        /// Evento ao mudar quantidade de itens por página
        /// </summary>
        private void CbItensPorPagina_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbItensPorPagina.SelectedItem is ComboBoxItem item)
            {
                _itensPorPagina = int.Parse(item.Content.ToString());
                _paginaAtual = 1; // Voltar para primeira página
                AtualizarPaginacao();
            }
        }

        /// <summary>
        /// Ir para primeira página
        /// </summary>
        private void BtnPrimeiraPagina_Click(object sender, RoutedEventArgs e)
        {
            if (_paginaAtual > 1)
            {
                _paginaAtual = 1;
                AtualizarPaginacao();
            }
        }

        /// <summary>
        /// Ir para página anterior
        /// </summary>
        private void BtnPaginaAnterior_Click(object sender, RoutedEventArgs e)
        {
            if (_paginaAtual > 1)
            {
                _paginaAtual--;
                AtualizarPaginacao();
            }
        }

        /// <summary>
        /// Ir para próxima página
        /// </summary>
        private void BtnProximaPagina_Click(object sender, RoutedEventArgs e)
        {
            if (_paginaAtual < _totalPaginas)
            {
                _paginaAtual++;
                AtualizarPaginacao();
            }
        }

        /// <summary>
        /// Ir para última página
        /// </summary>
        private void BtnUltimaPagina_Click(object sender, RoutedEventArgs e)
        {
            if (_paginaAtual < _totalPaginas)
            {
                _paginaAtual = _totalPaginas;
                AtualizarPaginacao();
            }
        }

        /// <summary>
        /// Evento ao clicar em número de página específico
        /// </summary>
        private void BtnNumeroPagina_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int numeroPagina)
            {
                _paginaAtual = numeroPagina;
                AtualizarPaginacao();
            }
        }

        #endregion

        #region Métodos de Carregamento

        /// <summary>
        /// Carrega todos os produtos do banco
        /// </summary>
        private void CarregarProdutos()
        {
            try
            {
                _todosOsProdutos = _context.Produtos
                    .OrderBy(p => p.Nome)
                    .ToList();

                txtCodigo.Text = GerarProximoCodigo().ToString();
                _paginaAtual = 1;
                AtualizarPaginacao();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao carregar produtos: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Filtra produtos com base na pesquisa
        /// </summary>
        private void FiltrarProdutos()
        {
            try
            {
                string filtro = txtPesquisa.Text.ToLower().Trim();

                var query = _context.Produtos.AsQueryable();

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    query = query.Where(p =>
                        p.Codigo.ToString().Contains(filtro) ||
                        p.Nome.ToLower().Contains(filtro) ||
                        p.Categoria.ToLower().Contains(filtro) ||
                        p.Status.ToLower().Contains(filtro));
                }

                _todosOsProdutos = query.OrderBy(p => p.Nome).ToList();
                AtualizarPaginacao();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao filtrar produtos: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Métodos de Paginação

        /// <summary>
        /// Atualiza a exibição com base na paginação atual
        /// </summary>
        private void AtualizarPaginacao()
        {
            if (dgProdutos == null ||
                txtInfoPaginacao == null ||
                txtTotalProdutos == null ||
                pnlNumerosPagina == null)
                return;
                        

            // Calcular total de páginas
            _totalItens = _todosOsProdutos.Count;
            _totalPaginas = _totalItens > 0
                ? (int)Math.Ceiling((double)_totalItens / _itensPorPagina)
                : 1;

            // Garantir que página atual está no range válido
            if (_paginaAtual > _totalPaginas)
                _paginaAtual = _totalPaginas;

            if (_paginaAtual < 1)
                _paginaAtual = 1;

            // Obter itens da página atual
            var itensPagina = _todosOsProdutos
                .Skip((_paginaAtual - 1) * _itensPorPagina)
                .Take(_itensPorPagina)
                .ToList();

            // Atualizar DataGrid
            dgProdutos.ItemsSource = itensPagina;

            // Atualizar informações de paginação
            AtualizarInformacoesPaginacao();

            // Atualizar botões de navegação
            AtualizarBotoesPaginacao();

            // Gerar números de página
            GerarNumerosPagina();

            // Atualizar total de produtos
            txtTotalProdutos.Text = $"Total: {_totalItens} produto(s)";
        }

        /// <summary>
        /// Atualiza o texto com informações da paginação
        /// </summary>
        private void AtualizarInformacoesPaginacao()
        {
            int inicio = _totalItens > 0 ? ((_paginaAtual - 1) * _itensPorPagina) + 1 : 0;
            int fim = Math.Min(_paginaAtual * _itensPorPagina, _totalItens);

            txtInfoPaginacao.Text = _totalItens > 0
                ? $"Exibindo {inicio}-{fim} de {_totalItens} produtos | Página {_paginaAtual} de {_totalPaginas}"
                : "Nenhum produto encontrado";
        }

        /// <summary>
        /// Atualiza o estado dos botões de navegação
        /// </summary>
        private void AtualizarBotoesPaginacao()
        {
            btnPrimeiraPagina.IsEnabled = _paginaAtual > 1;
            btnPaginaAnterior.IsEnabled = _paginaAtual > 1;
            btnProximaPagina.IsEnabled = _paginaAtual < _totalPaginas;
            btnUltimaPagina.IsEnabled = _paginaAtual < _totalPaginas;
        }

        /// <summary>
        /// Gera os botões de número de página dinamicamente
        /// </summary>
        private void GerarNumerosPagina()
        {
            pnlNumerosPagina.Children.Clear();

            if (_totalPaginas <= 1)
                return;

            // Determinar range de páginas a exibir
            int inicioPagina, fimPagina;

            if (_totalPaginas <= 7)
            {
                // Se tiver 7 ou menos páginas, mostra todas
                inicioPagina = 1;
                fimPagina = _totalPaginas;
            }
            else
            {
                // Mostra 7 páginas: atual +/- 3
                inicioPagina = Math.Max(1, _paginaAtual - 3);
                fimPagina = Math.Min(_totalPaginas, _paginaAtual + 3);

                // Ajustar se estiver próximo do início ou fim
                if (_paginaAtual <= 4)
                {
                    inicioPagina = 1;
                    fimPagina = Math.Min(7, _totalPaginas);
                }
                else if (_paginaAtual >= _totalPaginas - 3)
                {
                    inicioPagina = Math.Max(1, _totalPaginas - 6);
                    fimPagina = _totalPaginas;
                }
            }

            // Adicionar "..." se necessário no início
            if (inicioPagina > 1)
            {
                AdicionarBotaoPagina(1);

                if (inicioPagina > 2)
                {
                    var txtReticencias = new TextBlock
                    {
                        Text = "...",
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(4, 0, 4, 0),
                        Foreground = System.Windows.Media.Brushes.Gray
                    };
                    pnlNumerosPagina.Children.Add(txtReticencias);
                }
            }

            // Adicionar botões de página
            for (int i = inicioPagina; i <= fimPagina; i++)
            {
                AdicionarBotaoPagina(i);
            }

            // Adicionar "..." se necessário no final
            if (fimPagina < _totalPaginas)
            {
                if (fimPagina < _totalPaginas - 1)
                {
                    var txtReticencias = new TextBlock
                    {
                        Text = "...",
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(4, 0, 4, 0),
                        Foreground = System.Windows.Media.Brushes.Gray
                    };
                    pnlNumerosPagina.Children.Add(txtReticencias);
                }

                AdicionarBotaoPagina(_totalPaginas);
            }
        }

        /// <summary>
        /// Adiciona um botão de número de página
        /// </summary>
        private void AdicionarBotaoPagina(int numeroPagina)
        {
            var btn = new Button
            {
                Content = numeroPagina.ToString(),
                Tag = numeroPagina,
                Style = (Style)FindResource("PaginationButton")
            };

            // Marcar página atual como ativa
            if (numeroPagina == _paginaAtual)
            {
                btn.Tag = "Active";
            }

            btn.Click += BtnNumeroPagina_Click;
            pnlNumerosPagina.Children.Add(btn);
        }

        #endregion

        #region Métodos Auxiliares

        /// <summary>
        /// Gera o próximo código disponível
        /// </summary>
        private int GerarProximoCodigo()
        {
            return _context.Produtos.Any()
                ? _context.Produtos.Max(p => p.Codigo) + 1
                : 1;
        }

        /// <summary>
        /// Preenche os campos com dados do produto
        /// </summary>
        private void PreencherCampos(Produto produto)
        {
            txtCodigo.Text = produto.Codigo.ToString();
            txtNome.Text = produto.Nome;
            txtDescricao.Text = produto.Descricao;
            txtPrecoCusto.Text = produto.PrecoCusto.ToString("F2");
            txtPrecoVenda.Text = produto.PrecoVenda.ToString("F2");
            txtEstoque.Text = produto.Estoque.ToString();
            txtEstoqueMinimo.Text = produto.EstoqueMinimo.ToString();

            SelecionarCombo(cbCategoria, produto.Categoria);
            SelecionarCombo(cbUnidade, produto.Unidade);
            SelecionarCombo(cbStatus, produto.Status);
        }

        /// <summary>
        /// Seleciona item no ComboBox
        /// </summary>
        private void SelecionarCombo(ComboBox combo, string valor)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if (item.Content.ToString() == valor)
                {
                    combo.SelectedItem = item;
                    break;
                }
            }
        }

        /// <summary>
        /// Limpa todos os campos do formulário
        /// </summary>
        private void LimparCampos()
        {
            txtNome.Clear();
            txtDescricao.Clear();
            txtPrecoCusto.Clear();
            txtPrecoVenda.Clear();
            txtEstoque.Clear();
            txtEstoqueMinimo.Clear();
            cbCategoria.SelectedIndex = 0;
            cbUnidade.SelectedIndex = 0;
            cbStatus.SelectedIndex = 0;
        }

        #endregion

        #region Validações

        /// <summary>
        /// Valida todos os campos do formulário
        /// </summary>
        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show("Informe o nome do produto.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtNome.Focus();
                return false;
            }

            if (cbCategoria.SelectedIndex == 0)
            {
                MessageBox.Show("Selecione uma categoria.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cbCategoria.Focus();
                return false;
            }

            if (cbUnidade.SelectedIndex == 0)
            {
                MessageBox.Show("Selecione uma unidade.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cbUnidade.Focus();
                return false;
            }

            if (!decimal.TryParse(txtPrecoCusto.Text, out var custo) || custo < 0)
            {
                MessageBox.Show("Preço de custo inválido.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrecoCusto.Focus();
                return false;
            }

            if (!decimal.TryParse(txtPrecoVenda.Text, out var venda) || venda < 0)
            {
                MessageBox.Show("Preço de venda inválido.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtPrecoVenda.Focus();
                return false;
            }

            if (!int.TryParse(txtEstoque.Text, out var estoque) || estoque < 0)
            {
                MessageBox.Show("Estoque inválido.", "Atenção",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                txtEstoque.Focus();
                return false;
            }

            if (venda < custo)
            {
                var result = MessageBox.Show(
                    "Preço de venda menor que custo. Continuar?",
                    "Atenção",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                return result == MessageBoxResult.Yes;
            }

            return true;
        }

        /// <summary>
        /// Valida entrada de valores decimais
        /// </summary>
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9.,]+$");
        }

        /// <summary>
        /// Valida entrada apenas numérica
        /// </summary>
        private void TextBox_PreviewTextInputNumeric(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Limpa recursos ao fechar a janela
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            _context?.Dispose();
            base.OnClosed(e);
        }

        #endregion
    }
}