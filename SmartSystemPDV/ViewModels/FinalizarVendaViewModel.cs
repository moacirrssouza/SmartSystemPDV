using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace SmartSystemPDV.ViewModel;

public class FinalizarVendaViewModel : INotifyPropertyChanged
{
    #region Propriedades

    private decimal _totalVenda;
    public decimal TotalVenda
    {
        get => _totalVenda;
        set
        {
            _totalVenda = value;
            OnPropertyChanged();
            AtualizarCalculos();
        }
    }

    private string _formaPagamentoSelecionada;
    public string FormaPagamentoSelecionada
    {
        get => _formaPagamentoSelecionada;
        set
        {
            _formaPagamentoSelecionada = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MostrarPainelPagamento));

            // Quando seleciona forma de pagamento, sugere o valor restante
            if (!string.IsNullOrEmpty(value))
            {
                ValorPagamentoAtual = ValorRestante.ToString("N2");
            }
        }
    }

    private string _valorPagamentoAtual;
    public string ValorPagamentoAtual
    {
        get => _valorPagamentoAtual;
        set
        {
            _valorPagamentoAtual = value;
            OnPropertyChanged();
        }
    }

    private ObservableCollection<PagamentoItem> _pagamentosAdicionados;
    public ObservableCollection<PagamentoItem> PagamentosAdicionados
    {
        get => _pagamentosAdicionados;
        set
        {
            _pagamentosAdicionados = value;
            OnPropertyChanged();
        }
    }

    private PagamentoItem _pagamentoSelecionado;
    public PagamentoItem PagamentoSelecionado
    {
        get => _pagamentoSelecionado;
        set
        {
            _pagamentoSelecionado = value;
            OnPropertyChanged();
        }
    }

    private string _observacoes;
    public string Observacoes
    {
        get => _observacoes;
        set
        {
            _observacoes = value;
            OnPropertyChanged();
        }
    }

    // Propriedades Calculadas
    public decimal TotalPago => PagamentosAdicionados?.Sum(p => p.Valor) ?? 0;

    public decimal ValorRestante => TotalVenda - TotalPago;

    public decimal ValorRestanteTroco => Math.Abs(ValorRestante);

    public string LabelRestanteTroco => ValorRestante > 0 ? "Restante:" : "Troco:";

    public bool TemTroco => ValorRestante < 0;

    public bool PodeConfirmarVenda => TotalPago >= TotalVenda;
    public bool VendaConfirmada { get; private set; }

    public Visibility MostrarPainelPagamento =>
        string.IsNullOrEmpty(FormaPagamentoSelecionada) ? Visibility.Collapsed : Visibility.Visible;

    #endregion

    #region Commands

    public ICommand SelecionarFormaPagamentoCommand { get; }
    public ICommand AdicionarPagamentoCommand { get; }
    public ICommand RemoverPagamentoCommand { get; }
    public ICommand ConfirmarVendaCommand { get; }
    public ICommand CancelarCommand { get; }

    #endregion

    #region Construtor

    public FinalizarVendaViewModel(decimal totalVenda)
    {
        TotalVenda = totalVenda;
        PagamentosAdicionados = new ObservableCollection<PagamentoItem>();

        // Monitora mudanças na coleção de pagamentos
        PagamentosAdicionados.CollectionChanged += (s, e) => AtualizarCalculos();

        // Inicializa os commands
        SelecionarFormaPagamentoCommand = new RelayCommand<string>(SelecionarFormaPagamento);
        AdicionarPagamentoCommand = new RelayCommand(AdicionarPagamento, PodeAdicionarPagamento);
        RemoverPagamentoCommand = new RelayCommand<PagamentoItem>(RemoverPagamento);
        ConfirmarVendaCommand = new RelayCommand(ConfirmarVenda, () => PodeConfirmarVenda);
        CancelarCommand = new RelayCommand(Cancelar);
    }

    #endregion

    #region Métodos

    private void SelecionarFormaPagamento(string formaPagamento)
    {
        FormaPagamentoSelecionada = formaPagamento;
    }

    private bool PodeAdicionarPagamento()
    {
        if (string.IsNullOrEmpty(FormaPagamentoSelecionada))
            return false;

        if (string.IsNullOrEmpty(ValorPagamentoAtual))
            return false;

        return decimal.TryParse(ValorPagamentoAtual, out decimal valor) && valor > 0;
    }

    private void AdicionarPagamento()
    {
        if (!decimal.TryParse(ValorPagamentoAtual, out decimal valor))
        {
            MessageBox.Show("Valor inválido!", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (valor <= 0)
        {
            MessageBox.Show("O valor deve ser maior que zero!", "Erro", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var pagamento = new PagamentoItem
        {
            FormaPagamento = FormaPagamentoSelecionada,
            Valor = valor
        };

        PagamentosAdicionados.Add(pagamento);

        // Limpa seleção e valor
        FormaPagamentoSelecionada = null;
        ValorPagamentoAtual = string.Empty;
    }

    private void RemoverPagamento(PagamentoItem pagamento)
    {
        if (pagamento != null)
        {
            PagamentosAdicionados.Remove(pagamento);
        }
    }

    private void ConfirmarVenda()
    {
        if (!PodeConfirmarVenda)
        {
            MessageBox.Show("O valor pago é insuficiente!", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var resultado = MessageBox.Show(
            $"Confirmar venda no valor de R$ {TotalVenda:N2}?\n\n" +
            $"Total Pago: R$ {TotalPago:N2}\n" +
            (TemTroco ? $"Troco: R$ {ValorRestanteTroco:N2}" : ""),
            "Confirmar Venda",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (resultado == MessageBoxResult.Yes)
        {
            VendaConfirmada = true;

            // Fechar janela
            Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)
                ?.Close();
        }
    }

    private void Cancelar()
    {
        var resultado = MessageBox.Show(
            "Deseja cancelar esta operação?",
            "Cancelar",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (resultado == MessageBoxResult.Yes)
        {
            Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)
                ?.Close();
        }
    }

    private void AtualizarCalculos()
    {
        OnPropertyChanged(nameof(TotalPago));
        OnPropertyChanged(nameof(ValorRestante));
        OnPropertyChanged(nameof(ValorRestanteTroco));
        OnPropertyChanged(nameof(LabelRestanteTroco));
        OnPropertyChanged(nameof(TemTroco));
        OnPropertyChanged(nameof(PodeConfirmarVenda));
    }

    #endregion

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}

#region Classes Auxiliares

public class PagamentoItem
{
    public string FormaPagamento { get; set; }
    public decimal Valor { get; set; }
}

// RelayCommand - Implementação básica
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    public RelayCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object parameter) => _execute();
}

public class RelayCommand<T> : ICommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool> _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    public bool CanExecute(object parameter)
    {
        if (parameter is T typedParameter)
            return _canExecute?.Invoke(typedParameter) ?? true;
        return false;
    }

    public void Execute(object parameter)
    {
        if (parameter is T typedParameter)
            _execute(typedParameter);
    }
}

#endregion
