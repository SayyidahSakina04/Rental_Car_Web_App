using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RentalCarManagementSystem.Data;
using RentalCarManagementSystem.Hubs;
using RentalCarManagementSystem.Models;
using RentalCarManagementSystem.Models.Interfaces;
using RentalCarManagementSystem.Models.Repositories;
using static RentalCarManagementSystem.Hubs.NotificationHub;

namespace RentalCarManagementSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBookingRepository _bookingRepository;
        private readonly ICarRepository _carRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AdminController(ApplicationDbContext context,
            ICarRepository carRepository, IBookingRepository bookingRepository,
            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _bookingRepository = bookingRepository;
            _carRepository = carRepository;
            _hubContext = hubContext;
        }
        
        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Products()
        {
            var cars = _carRepository.GetAllCarsForAdmin(_context);
            return View(cars);
        }
        [Authorize(Policy = "AdminOnly")]
        public IActionResult Dashboard()
        {
            return View();
        }


        [Authorize(Policy = "AdminOnly")]
        public IActionResult ActiveRentals()
        {
            //var activeRentals = (from booking in _context.Bookings
            //                     join car in _context.Cars on booking.CarId equals car.Id
            //                     join user in _context.Users on booking.UserId equals user.Id
            //                     where booking.Status == "Approved"
            //                     select new RentalCarManagementSystem.Models.ViewModels.ActiveRentalViewModel
            //                     {
            //                         BookingId = booking.BookingId,
            //                         CustomerEmail = user.Email,
            //                         CarModel = car.Model,
            //                         StartDate = booking.StartDate,
            //                         EndDate = booking.EndDate
            //                     }).ToList();

            var activeRentals = _carRepository.GetAllActiveRentals(_context);

            return View(activeRentals);
        }


        [Authorize(Policy = "AdminOnly")]
        public IActionResult BookingHistory()
        {
            var bookings = _bookingRepository.GetBookingHistoryForAdmin(_context);

            return View(bookings);
        }


        [Authorize(Policy = "AdminOnly")]
        public IActionResult PendingApprovals()
        {
            
            var pendingBookings = _bookingRepository.GetPendingApprovalForAdmin(_context);

            return View(pendingBookings);
        }

        [HttpPost]  // to approve
        public async Task<IActionResult> ApproveBooking(int id)
        {
            _bookingRepository.ApproveBooking(_context, id);
            await _hubContext.Clients.All.SendAsync("BookingStatusChanged");
            TempData["Success"] = $"Booking #{id} approved successfully.";
            return RedirectToAction("PendingApprovals");
        }

        [HttpPost]  // to reject
        public IActionResult RejectBooking(int id)
        {
            _bookingRepository.RejectBooking(_context, id);

            TempData["Success"] = $"Booking #{id} rejected.";
            return RedirectToAction("PendingApprovals");
        }

        // to get form view
        [Authorize(Policy = "AdminOnly")]
        public IActionResult AddProduct()
        {
            return View();
        }

        //to actually add product
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> AddProduct(Car car)
        {
            var done = _carRepository.AddCar(car, _context);

            if (done)
            {
                await _hubContext.Clients.All.SendAsync("CarListUpdated");
                TempData["Success"] = "Car added successfully!";
                return RedirectToAction("Products");
            }
            return View(car);
        }

        [Authorize(Policy = "AdminOnly")]
        public IActionResult EditProduct(int id)
        {
            var car = _context.Cars.FirstOrDefault(c => c.Id == id);
            if (car == null)
                return View();
            return View(car);
        }

        [HttpPost]
        public async Task<IActionResult> EditProduct(Car updatedCar)
        {
            if (ModelState.IsValid)
            {
                var existingCar = _context.Cars.FirstOrDefault(c => c.Id == updatedCar.Id);
                if (existingCar == null)
                    return NotFound();

                existingCar.Model = updatedCar.Model;
                existingCar.Description = updatedCar.Description;
                existingCar.PricePerDay = updatedCar.PricePerDay;
                existingCar.ImageURL = updatedCar.ImageURL;
                //existingCar.IsAvailable = updatedCar.IsAvailable;

                _context.SaveChanges();
                
                await _hubContext.Clients.All.SendAsync("CarListUpdated");
                
                TempData["Success"] = "Car updated successfully!";
                Console.WriteLine("Redirecting to Products page...");
                return RedirectToAction("Products", "Admin");
            }
            return View(updatedCar);
        }

        [Authorize(Policy = "AdminOnly")]
        public IActionResult DeleteProduct(int id)  // will show the confirmatuon message that you really wanna delete this 
        {
            var car = _context.Cars.FirstOrDefault(c => c.Id == id);
            if (car == null)
                return View();
            return View(car);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var car = _context.Cars.FirstOrDefault(c => c.Id == id);
            if (car == null)
                return View();

            _context.Cars.Remove(car);
            _context.SaveChanges();

            await _hubContext.Clients.All.SendAsync("CarListUpdated");
            TempData["Success"] = "Car deleted successfully!";
            return RedirectToAction("Products", "Admin");
        }


        [Authorize(Policy = "AdminOnly")]
        public IActionResult ContactUsMessages()
        {
            var messages = _context.ContactUs
                        .OrderByDescending(c => c.SubmittedAt)
                        .ToList();

            return View(messages);
        }

    }
}
