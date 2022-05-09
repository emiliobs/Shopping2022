using Shopping2022.Common;
using Shopping2022.Data;
using Shopping2022.Data.Entities;
using Shopping2022.Enums;
using Shopping2022.Models;

namespace Shopping2022.Helpers
{
    public class OrdersHelper : IOrdersHelper
    {
        private readonly DataContext _context;

        public OrdersHelper(DataContext context)
        {
            _context = context;
        }

        public async Task<Response> ProcessOrderAsync(ShowCartVIewModel showCartVIewModel)
        {
            Response response = await CheckInventoryAsync(showCartVIewModel);
            if (!response.IsSuccess)
            {
                return response;
            }

            Sale sale = new()
            {
                Date = DateTime.UtcNow,
                User = showCartVIewModel.User,
                Remarks = showCartVIewModel.Remarks,
                SaleDetails = new List<SaleDetail>(),
                OrderStatus = OrderStatus.Nuevo,
            };

            foreach (TemporalSale temporaleSale in showCartVIewModel.TemporalSales)
            {
                sale.SaleDetails.Add(new SaleDetail
                {
                    Product = temporaleSale.Product,
                    Quantity = temporaleSale.Quantity,
                    Remarks = temporaleSale.Remarks,
                });

                Product product = await _context.Products.FindAsync(temporaleSale.Product.Id);
                if (product != null)
                {
                    product.Stock -= temporaleSale.Quantity;
                    _ = _context.Products.Update(product);
                }

                _ = _context.TemporalSales.Remove(temporaleSale);

            }

            _ = _context.Sales.Add(sale);
            _ = await _context.SaveChangesAsync();

            return response;

        }

        private async Task<Response> CheckInventoryAsync(ShowCartVIewModel showCartVIewModel)
        {
            Response response = new() { IsSuccess = true };
            foreach (TemporalSale temporaleSale in showCartVIewModel.TemporalSales)
            {
                Product product = await _context.Products.FindAsync(temporaleSale.Product.Id);
                if (product is null)
                {
                    response.IsSuccess = false;
                    response.Message = $"El product {temporaleSale.Product.Name}, ya no esta disponible.";

                    return response;

                }

                if (product.Stock < temporaleSale.Quantity)
                {
                    response.IsSuccess = false;
                    response.Message = $"Lo sentimos no temos existencia suficientes del producto {temporaleSale.Product.Name}, para tomar su pedido. Por favor disminuir la cantidad o sustituirlo por otro.";
                    return response;
                }
            }

            return response;
        }
    }
}
