using SmartSystemPDV.Models;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SmartSystemPDV.View.CadastroProdutos;

/// <summary>
/// Interaction logic for ProductsView.xaml
/// </summary>
public partial class CadastroProdutosWindow : Window
{
    private ObservableCollection<Produto> listaProdutos;
    private int proximoCodigo = 1;
    private Produto produtoSelecionado;

    public CadastroProdutosWindow()
    {
        InitializeComponent();
        InicializarDados();
        CarregarProdutos();
    }

    #region Eventos
    private void BtnNovo_Click(object sender, RoutedEventArgs e)
    {
        LimparCampos();
        txtCodigo.Text = proximoCodigo.ToString();
        txtNome.Focus();
        produtoSelecionado = null;
    }

    private void BtnSalvar_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidarCampos())
            return;

        try
        {
            if (produtoSelecionado == null)
            {
                // Novo produto
                Produto novoProduto = new Produto
                {
                    Codigo = proximoCodigo++,
                    Nome = txtNome.Text.Trim(),
                    Descricao = txtDescricao.Text.Trim(),
                    Categoria = ((ComboBoxItem)cbCategoria.SelectedItem).Content.ToString(),
                    Unidade = ((ComboBoxItem)cbUnidade.SelectedItem).Content.ToString(),
                    PrecoCusto = decimal.Parse(txtPrecoCusto.Text, CultureInfo.CurrentCulture),
                    PrecoVenda = decimal.Parse(txtPrecoVenda.Text, CultureInfo.CurrentCulture),
                    Estoque = int.Parse(txtEstoque.Text),
                    EstoqueMinimo = string.IsNullOrEmpty(txtEstoqueMinimo.Text) ? 0 : int.Parse(txtEstoqueMinimo.Text),
                    Status = ((ComboBoxItem)cbStatus.SelectedItem).Content.ToString()
                };

                listaProdutos.Add(novoProduto);
                MessageBox.Show("Produto cadastrado com sucesso!", "Sucesso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Editar produto existente
                produtoSelecionado.Nome = txtNome.Text.Trim();
                produtoSelecionado.Descricao = txtDescricao.Text.Trim();
                produtoSelecionado.Categoria = ((ComboBoxItem)cbCategoria.SelectedItem).Content.ToString();
                produtoSelecionado.Unidade = ((ComboBoxItem)cbUnidade.SelectedItem).Content.ToString();
                produtoSelecionado.PrecoCusto = decimal.Parse(txtPrecoCusto.Text, CultureInfo.CurrentCulture);
                produtoSelecionado.PrecoVenda = decimal.Parse(txtPrecoVenda.Text, CultureInfo.CurrentCulture);
                produtoSelecionado.Estoque = int.Parse(txtEstoque.Text);
                produtoSelecionado.EstoqueMinimo = string.IsNullOrEmpty(txtEstoqueMinimo.Text) ? 0 : int.Parse(txtEstoqueMinimo.Text);
                produtoSelecionado.Status = ((ComboBoxItem)cbStatus.SelectedItem).Content.ToString();

                dgProdutos.Items.Refresh();
                MessageBox.Show("Produto atualizado com sucesso!", "Sucesso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            LimparCampos();
            txtCodigo.Text = proximoCodigo.ToString();
            produtoSelecionado = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao salvar produto: {ex.Message}", "Erro",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BtnCancelar_Click(object sender, RoutedEventArgs e)
    {
        LimparCampos();
        txtCodigo.Text = proximoCodigo.ToString();
        produtoSelecionado = null;
        dgProdutos.SelectedItem = null;
        btnEditar.IsEnabled = false;
        btnExcluir.IsEnabled = false;
    }

    private void BtnEditar_Click(object sender, RoutedEventArgs e)
    {
        if (dgProdutos.SelectedItem is Produto produto)
        {
            produtoSelecionado = produto;
            PreencherCampos(produto);
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
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                listaProdutos.Remove(produto);
                MessageBox.Show("Produto excluído com sucesso!", "Sucesso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LimparCampos();
                btnEditar.IsEnabled = false;
                btnExcluir.IsEnabled = false;
            }
        }
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

    // Validação para permitir apenas números e vírgula/ponto decimal
    private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex(@"^[0-9.,]+$");
        e.Handled = !regex.IsMatch(e.Text);
    }

    // Validação para permitir apenas números inteiros
    private void TextBox_PreviewTextInputNumeric(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex(@"^[0-9]+$");
        e.Handled = !regex.IsMatch(e.Text);
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
            this.Close();
        }
    }
    #endregion

    #region Métodos

    private void InicializarDados()
    {
        listaProdutos = new ObservableCollection<Produto>();

    }

    private void CarregarProdutos()
    {
        dgProdutos.ItemsSource = listaProdutos;
        txtCodigo.Text = proximoCodigo.ToString();
    }

    private void FiltrarProdutos()
    {
        string filtro = txtPesquisa.Text.ToLower();

        if (string.IsNullOrWhiteSpace(filtro))
        {
            dgProdutos.ItemsSource = listaProdutos;
        }
        else
        {
            var produtosFiltrados = listaProdutos.Where(p =>
                p.Codigo.ToString().Contains(filtro) ||
                p.Nome.ToLower().Contains(filtro) ||
                p.Categoria.ToLower().Contains(filtro) ||
                p.Status.ToLower().Contains(filtro)
            ).ToList();

            dgProdutos.ItemsSource = produtosFiltrados;
        }
    }

    private void PreencherCampos(Produto produto)
    {
        txtCodigo.Text = produto.Codigo.ToString();
        txtNome.Text = produto.Nome;
        txtDescricao.Text = produto.Descricao;

        // Selecionar categoria
        foreach (ComboBoxItem item in cbCategoria.Items)
        {
            if (item.Content.ToString() == produto.Categoria)
            {
                cbCategoria.SelectedItem = item;
                break;
            }
        }

        // Selecionar unidade
        foreach (ComboBoxItem item in cbUnidade.Items)
        {
            if (item.Content.ToString() == produto.Unidade)
            {
                cbUnidade.SelectedItem = item;
                break;
            }
        }

        txtPrecoCusto.Text = produto.PrecoCusto.ToString("F2");
        txtPrecoVenda.Text = produto.PrecoVenda.ToString("F2");
        txtEstoque.Text = produto.Estoque.ToString();
        txtEstoqueMinimo.Text = produto.EstoqueMinimo.ToString();

        // Selecionar status
        foreach (ComboBoxItem item in cbStatus.Items)
        {
            if (item.Content.ToString() == produto.Status)
            {
                cbStatus.SelectedItem = item;
                break;
            }
        }

        txtNome.Focus();
    }

    private void LimparCampos()
    {
        txtNome.Clear();
        txtDescricao.Clear();
        cbCategoria.SelectedIndex = 0;
        cbUnidade.SelectedIndex = 0;
        txtPrecoCusto.Clear();
        txtPrecoVenda.Clear();
        txtEstoque.Clear();
        txtEstoqueMinimo.Clear();
        cbStatus.SelectedIndex = 0;
    }

    private bool ValidarCampos()
    {
        if (string.IsNullOrWhiteSpace(txtNome.Text))
        {
            MessageBox.Show("Por favor, informe o nome do produto.", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNome.Focus();
            return false;
        }

        if (cbCategoria.SelectedIndex == 0)
        {
            MessageBox.Show("Por favor, selecione uma categoria.", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            cbCategoria.Focus();
            return false;
        }

        if (cbUnidade.SelectedIndex == 0)
        {
            MessageBox.Show("Por favor, selecione uma unidade de medida.", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            cbUnidade.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtPrecoCusto.Text))
        {
            MessageBox.Show("Por favor, informe o preço de custo.", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtPrecoCusto.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtPrecoVenda.Text))
        {
            MessageBox.Show("Por favor, informe o preço de venda.", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtPrecoVenda.Focus();
            return false;
        }

        if (string.IsNullOrWhiteSpace(txtEstoque.Text))
        {
            MessageBox.Show("Por favor, informe o estoque atual.", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtEstoque.Focus();
            return false;
        }

        // Validar valores numéricos
        if (!decimal.TryParse(txtPrecoCusto.Text, out decimal precoCusto) || precoCusto < 0)
        {
            MessageBox.Show("Preço de custo inválido.", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtPrecoCusto.Focus();
            return false;
        }

        if (!decimal.TryParse(txtPrecoVenda.Text, out decimal precoVenda) || precoVenda < 0)
        {
            MessageBox.Show("Preço de venda inválido.", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtPrecoVenda.Focus();
            return false;
        }

        if (!int.TryParse(txtEstoque.Text, out int estoque) || estoque < 0)
        {
            MessageBox.Show("Estoque inválido.", "Atenção",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtEstoque.Focus();
            return false;
        }

        // Validar se preço de venda é maior que preço de custo
        if (precoVenda < precoCusto)
        {
            var result = MessageBox.Show(
                "O preço de venda é menor que o preço de custo. Deseja continuar?",
                "Atenção",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result == MessageBoxResult.No)
            {
                txtPrecoVenda.Focus();
                return false;
            }
        }

        return true;
    }
    #endregion
}