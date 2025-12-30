using SmartSystemPDV.Data.Context;
using SmartSystemPDV.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartSystemPDV.View.CadastroProdutos
{
    public partial class CadastroProdutosWindow : Window
    {
        private readonly AppDbContext _context;
        private Produto? _produtoSelecionado;

        public CadastroProdutosWindow()
        {
            InitializeComponent();
            _context = new AppDbContext();
            CarregarProdutos();
        }

        #region Eventos

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
                _produtoSelecionado.EstoqueMinimo = int.Parse(txtEstoqueMinimo.Text);
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
                    _context.Produtos.Remove(produto);
                    _context.SaveChanges();

                    CarregarProdutos();
                    LimparCampos();
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

        private void BtnPesquisar_Click(object sender, RoutedEventArgs e)
        {
            FiltrarProdutos();
        }

        private void TxtPesquisa_TextChanged(object sender, TextChangedEventArgs e)
        {
            FiltrarProdutos();
        }

        private void DgProdutos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            btnEditar.IsEnabled = dgProdutos.SelectedItem != null;
            btnExcluir.IsEnabled = dgProdutos.SelectedItem != null;
        }

        private void BtnVoltar_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Deseja voltar?", "Confirmar",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Close();
            }
        }

        #endregion

        #region Métodos

        private void CarregarProdutos()
        {
            dgProdutos.ItemsSource = _context.Produtos
                .OrderBy(p => p.Nome)
                .ToList();

            txtCodigo.Text = GerarProximoCodigo().ToString();
        }

        private int GerarProximoCodigo()
        {
            return _context.Produtos.Any()
                ? _context.Produtos.Max(p => p.Codigo) + 1
                : 1;
        }

        private void FiltrarProdutos()
        {
            string filtro = txtPesquisa.Text.ToLower();

            var query = _context.Produtos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(filtro))
            {
                query = query.Where(p =>
                    p.Codigo.ToString().Contains(filtro) ||
                    p.Nome.ToLower().Contains(filtro) ||
                    p.Categoria.ToLower().Contains(filtro) ||
                    p.Status.ToLower().Contains(filtro));
            }

            dgProdutos.ItemsSource = query.OrderBy(p => p.Nome).ToList();
        }

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

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(txtNome.Text))
            {
                MessageBox.Show("Informe o nome do produto.");
                return false;
            }

            if (!decimal.TryParse(txtPrecoCusto.Text, out var custo) || custo < 0)
            {
                MessageBox.Show("Preço de custo inválido.");
                return false;
            }

            if (!decimal.TryParse(txtPrecoVenda.Text, out var venda) || venda < 0)
            {
                MessageBox.Show("Preço de venda inválido.");
                return false;
            }

            if (!int.TryParse(txtEstoque.Text, out var estoque) || estoque < 0)
            {
                MessageBox.Show("Estoque inválido.");
                return false;
            }

            if (venda < custo)
            {
                return MessageBox.Show(
                    "Preço de venda menor que custo. Continuar?",
                    "Atenção",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning) == MessageBoxResult.Yes;
            }

            return true;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9.,]+$");
        }

        private void TextBox_PreviewTextInputNumeric(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
        }

        #endregion
    }
}
