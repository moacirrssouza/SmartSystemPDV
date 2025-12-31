namespace SmartSystemPDV.ViewModels;

public class ItemVendaViewModel
{
    public int ProdutoId { get; set; }
    public string ProdutoCodigo { get; set; }
    public string ProdutoNome { get; set; }
    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Desconto { get; set; }
    public decimal Subtotal { get; set; }

    public void CalcularSubtotal()
    {
        Subtotal = (PrecoUnitario * Quantidade) - Desconto;
    }
}