using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Data;
using Shopping2022.Helpers;
using Vereyon.Web;

namespace Shopping2022.Controllers
{
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly DataContext _context;
        private readonly IFlashMessage _flashMessage;
        private readonly IOrdersHelper _ordersHelper;

        public OrdersController(DataContext context, IFlashMessage flashMessage, IOrdersHelper ordersHelper)
        {
            _context = context;
            this._flashMessage = flashMessage;
            this._ordersHelper = ordersHelper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(
                         await _context.Sales
                         .Include(s => s.User)
                         .Include(s => s.SaleDetails)
                         .ThenInclude(sd => sd.Product)
                         .ToListAsync()
                       );
        }

        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            Data.Entities.Sale sales = await _context.Sales
                        .Include(s => s.User)
                        .Include(s => s.SaleDetails)
                        .ThenInclude(sd => sd.Product)
                        .ThenInclude(p => p.ProductImages)
                        .FirstOrDefaultAsync(s => s.Id == id);

            return sales is null ? NotFound() : View(sales);
        }

        public async Task<IActionResult> Dispatch(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var sale = await _context.Sales.FindAsync(id);
            if (sale is null)
            {
                return NotFound();
            }

            if (sale.OrderStatus != Enums.OrderStatus.Nuevo)
            {
                _flashMessage.Danger("Solo se pueden despachar pedidos que estén en estado 'Nuevo'.");
            }
            else
            {
                sale.OrderStatus = Enums.OrderStatus.Despachado;
                _context.Sales.Update(sale);
                await _context.SaveChangesAsync();
                _flashMessage.Confirmation("El estado del pedido ha sido cambiado a 'Despachado'.");
            }

            return RedirectToAction(nameof(Details), new {Id = sale.Id});
        }

        public async Task<IActionResult>Send(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var sale = await _context.Sales.FindAsync(id);
            if (sale is null)
            {
                return NotFound();
            }

            if (sale.OrderStatus != Enums.OrderStatus.Despachado)
            {
                _flashMessage.Danger("Solo se pueden enviar pedidos que esten en estado 'Despachado'.");
            }
            else
            {
                sale.OrderStatus = Enums.OrderStatus.Enviado;
                _context.Sales.Update(sale);
                await _context.SaveChangesAsync();
                _flashMessage.Confirmation("En el estado del pedido ha sido cambiado a 'Enviado'.");
            }

            return RedirectToAction(nameof(Details), new { id = sale.Id });
        }

        public async Task<IActionResult> Confirm(int? id)
        {
            if (id  is null)
            {
                return NotFound();
            }

            var sale = await _context.Sales.FindAsync(id);
            if (sale is null)
            {
                return NotFound();
            }

            if (sale.OrderStatus != Enums.OrderStatus.Enviado)
            {
                _flashMessage.Danger("Solo se puede confirmar pedidos que estén en estado 'Enviado'.");
            }
            else
            {
                sale.OrderStatus = Enums.OrderStatus.Confirmado;
                _context.Sales.Update(sale);
                await _context.SaveChangesAsync();
                _flashMessage.Confirmation("El estado del pedido ha sido cambiado a 'Confirmado' .");
            }

            return RedirectToAction(nameof(Details), new {Id = sale.Id} );
        }

        public async Task<IActionResult> Cancel(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var sale = await _context.Sales.FindAsync(id);
            if (sale is null)
            {
                return NotFound();
            }

            if (sale.OrderStatus == Enums.OrderStatus.Cancelado)
            {
                _flashMessage.Danger("NO se puede cancelar un pedido que esté en estado de 'Cancelado'.");
            }
            else
            {
                await _ordersHelper.CancelOrderAsync(sale.Id);
                _flashMessage.Confirmation("El estado del pedido ha sido cambiado a 'Cancelado'.");
            }

            return RedirectToAction(nameof(Details), new {Id = sale.Id});
        }
    }
}
