using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Data;
using Shopping2022.Helpers;
using Vereyon.Web;

namespace Shopping2022.Controllers
{
    public class OrdersController : Controller
    {
        private readonly DataContext _context;
        private readonly IFlashMessage _flashMessage;
        private readonly IOrdersHelper _ordersHelper;

        public OrdersController(DataContext context, IFlashMessage flashMessage, IOrdersHelper ordersHelper)
        {
            _context = context;
            _flashMessage = flashMessage;
            _ordersHelper = ordersHelper;
        }

        [Authorize(Roles = "Admin")]

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

        [Authorize(Roles = "Admin")]

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

        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Dispatch(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            Data.Entities.Sale sale = await _context.Sales.FindAsync(id);
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
                _ = _context.Sales.Update(sale);
                _ = await _context.SaveChangesAsync();
                _flashMessage.Confirmation("El estado del pedido ha sido cambiado a 'Despachado'.");
            }

            return RedirectToAction(nameof(Details), new { sale.Id });
        }

        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Send(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            Data.Entities.Sale sale = await _context.Sales.FindAsync(id);
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
                _ = _context.Sales.Update(sale);
                _ = await _context.SaveChangesAsync();
                _flashMessage.Confirmation("En el estado del pedido ha sido cambiado a 'Enviado'.");
            }

            return RedirectToAction(nameof(Details), new { id = sale.Id });
        }

        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Confirm(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            Data.Entities.Sale sale = await _context.Sales.FindAsync(id);
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
                _ = _context.Sales.Update(sale);
                _ = await _context.SaveChangesAsync();
                _flashMessage.Confirmation("El estado del pedido ha sido cambiado a 'Confirmado' .");
            }

            return RedirectToAction(nameof(Details), new { sale.Id });
        }

        [Authorize(Roles = "Admin")]

        public async Task<IActionResult> Cancel(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            Data.Entities.Sale sale = await _context.Sales.FindAsync(id);
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
                _ = await _ordersHelper.CancelOrderAsync(sale.Id);
                _flashMessage.Confirmation("El estado del pedido ha sido cambiado a 'Cancelado'.");
            }

            return RedirectToAction(nameof(Details), new { sale.Id });
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyOrders()
        {
            return View(await _context.Sales
                .Include(s => s.User)
                .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
                .Where(s => s.User.UserName == User.Identity.Name)
                .ToListAsync());
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> MyDetails(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            Data.Entities.Sale sale = await _context.Sales
                .Include(s => s.User)
                .Include(s => s.SaleDetails)
                .ThenInclude(sd => sd.Product)
                .ThenInclude(p => p.ProductImages)
                .FirstOrDefaultAsync(s => s.Id == id);

            return sale is null ? NotFound() : View(sale);
        }
    }
}
