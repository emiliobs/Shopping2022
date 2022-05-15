#nullable disable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shopping2022.Data;
using Shopping2022.Data.Entities;
using Shopping2022.Models;
using Vereyon.Web;

namespace Shopping2022.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CountriesController : Controller
    {
        private readonly DataContext _context;
        private readonly IFlashMessage _flashMessage;

        public CountriesController(DataContext context, IFlashMessage flashMessage)
        {
            _context = context;
            this._flashMessage = flashMessage;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Countries.Include(c => c.States).ToListAsync());
        }

        public IActionResult Create()
        {
            Country country = new()
            {
                States = new List<State>(),
            };

            return View(country);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Country country)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _ = _context.Add(country);
                    _ = await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        _flashMessage.Danger("Ya existe un País con el mismo nombre.");
                    }
                    else
                    {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception ex)
                {
                    _flashMessage.Danger(ex.Message);
                }
            }
            return View(country);
        }

        [HttpGet]

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Country country = await _context.Countries
                .Include(c => c.States)
                .ThenInclude(s => s.Cities)
                .FirstOrDefaultAsync(m => m.Id == id);
            return country == null ? NotFound() : View(country);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Country country = await _context.Countries.Include(c => c.States).FirstOrDefaultAsync(c => c.Id == id);
            return country == null ? NotFound() : View(country);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Country country)
        {
            if (id != country.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _ = _context.Update(country);
                    _ = await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        _flashMessage.Danger("Ya existe un País con el mismo nombre.");
                    }
                    else
                    {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception ex)
                {
                    _flashMessage.Danger(ex.Message);
                }
            }
            return View(country);
        }


        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Country country = await _context.Countries.Include(c => c.States)
                .FirstOrDefaultAsync(c => c.Id == id);
            return country == null ? NotFound() : View(country);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                Country country = await _context.Countries.FindAsync(id);
                _ = _context.Countries.Remove(country);
                _ = await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {

                throw;
            }
        }


        [HttpGet]

        public async Task<IActionResult> DetailsState(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            State state = await _context.States
                .Include(s => s.Country)
                .Include(s => s.Cities)
                .FirstOrDefaultAsync(m => m.Id == id);
            return state == null ? NotFound() : View(state);
        }


        [HttpGet]
        public async Task<IActionResult> AddState(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Country country = await _context.Countries.FindAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            StateViewModel stateVIewModel = new()
            {
                CountryId = country.Id,
            };

            return View(stateVIewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddState(StateViewModel stateViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    State state = new()
                    {
                        Cities = new List<City>(),
                        Country = await _context.Countries.FindAsync(stateViewModel.CountryId),
                        Name = stateViewModel.Name,
                    };

                    _ = _context.Add(state);
                    _ = await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Details), new { id = stateViewModel.Id });
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        _flashMessage.Danger("Ya existe un Departamento/Estado con el mismo nombre en este País.");
                    }
                    else
                    {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception ex)
                {
                    _flashMessage.Danger(ex.Message);
                }
            }
            return View(stateViewModel);
        }

        [HttpGet]

        public async Task<IActionResult> EditState(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            State state = await _context.States.Include(s => s.Country).FirstOrDefaultAsync(s => s.Id == id);
            if (state == null)
            {
                return NotFound();
            }

            StateViewModel stateViewModel = new()
            {
                CountryId = state.Country.Id,
                Id = state.Id,
                Name = state.Name,
            };

            return View(stateViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditState(int id, StateViewModel stateViewModely)
        {
            if (id != stateViewModely.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    State state = new()
                    {
                        Id = stateViewModely.Id,
                        Name = stateViewModely.Name,
                    };

                    _ = _context.Update(state);
                    _ = await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { id = stateViewModely.CountryId });
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        _flashMessage.Danger("Ya existe un Departamento/Estado con el mismo nombre en este País..");
                    }
                    else
                    {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception ex)
                {
                    _flashMessage.Danger(ex.Message);
                }
            }
            return View(stateViewModely);
        }

        [HttpGet]
        public async Task<IActionResult> AddCity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            State state = await _context.States.FindAsync(id);
            if (state == null)
            {
                return NotFound();
            }

            CityViewModel CityVIewModel = new()
            {
                StateId = state.Id,
            };

            return View(CityVIewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCity(CityViewModel cityViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    City City = new()
                    {
                        State = await _context.States.FindAsync(cityViewModel.StateId),
                        Name = cityViewModel.Name,
                    };

                    _ = _context.Add(City);
                    _ = await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(DetailsState), new { id = cityViewModel.Id });
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        _flashMessage.Danger("Ya existe una Ciudad con el mismo nombre en este Departamento/Estado.");
                    }
                    else
                    {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception ex)
                {
                    _flashMessage.Danger(ex.Message);
                }
            }
            return View(cityViewModel);
        }

        [HttpGet]

        public async Task<IActionResult> EditCity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            City city = await _context.Cities
                .Include(c => c.State)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (city == null)
            {
                return NotFound();
            }

            CityViewModel cityViewModel = new()
            {
                StateId = city.State.Id,
                Id = city.Id,
                Name = city.Name,
            };

            return View(cityViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCity(int id, CityViewModel cityViewModel)
        {
            if (id != cityViewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    City city = new()
                    {
                        Id = cityViewModel.Id,
                        Name = cityViewModel.Name,
                    };

                    _ = _context.Update(city);
                    _ = await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(DetailsState), new { id = cityViewModel.StateId });
                }
                catch (DbUpdateException dbUpdateException)
                {
                    if (dbUpdateException.InnerException.Message.Contains("duplicate"))
                    {
                        _flashMessage.Danger("Ya existe una Ciudad con el mismo nombre en este Departamento/Estado..");
                    }
                    else
                    {
                        _flashMessage.Danger(dbUpdateException.InnerException.Message);
                    }
                }
                catch (Exception ex)
                {
                    _flashMessage.Danger(ex.Message);
                }
            }
            return View(cityViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsCity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            City city = await _context.Cities
                .Include(c => c.State)
                .FirstOrDefaultAsync(c => c.Id == id);
            
                _flashMessage.Confirmation("Registro Borrado");


            return city == null ? NotFound() : View(city);
        }


        [HttpGet]
        public async Task<IActionResult> DeleteState(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            State state = await _context.States
                .Include(s => s.Country)
                .FirstOrDefaultAsync(s => s.Id == id);

            _flashMessage.Confirmation("Registro Borrado");

            return state == null ? NotFound() : View(state);
        }


        [HttpPost, ActionName("DeleteState")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedState(int id)
        {
            try
            {
                State state = await _context.States
                    .Include(s => s.Country)
                    .FirstOrDefaultAsync(s => s.Id == id);

                _ = _context.States.Remove(state);

                _ = await _context.SaveChangesAsync();

                _flashMessage.Confirmation("Registro Borrado");

                return RedirectToAction(nameof(Details), new { id = state.Country.Id });
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteCity(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            City city = await _context.Cities
                .Include(c => c.State)
                .FirstOrDefaultAsync(s => s.Id == id);
                _flashMessage.Confirmation("Registro Borrado");

            return city == null ? NotFound() : View(city);
        }


        [HttpPost, ActionName("DeleteCity")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmedCity(int id)
        {
            try
            {
                City city = await _context.Cities
                    .Include(c => c.State)
                    .FirstOrDefaultAsync(c => c.Id == id);

                _ = _context.Cities.Remove(city);

                _ = await _context.SaveChangesAsync();

                _flashMessage.Confirmation("Registro Borrado");

                return RedirectToAction(nameof(DetailsState), new { id = city.State.Id });
            }
            catch (Exception)
            {

                throw;
            }
        }




        //private bool CountryExists(int id)
        //{
        //    return _context.Countries.Any(e => e.Id == id);
        //}
    }
}
