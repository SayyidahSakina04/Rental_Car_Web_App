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

namespace RentalCarManagementSystem.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<MyUser> _userManager;
        private readonly IBookingRepository _bookingRepository;
        private readonly ICarRepository _carRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public CustomerController(ApplicationDbContext context, UserManager<MyUser> userManager,
                            ICarRepository carRepository, IBookingRepository bookingRepository,
                            IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _carRepository = carRepository;
            _bookingRepository = bookingRepository;
            _hubContext = hubContext;
        }

        public IActionResult Index()
        {
            return View();
        }
        
        
        [Authorize(Policy = "CustomerOnly")]
        public IActionResult Dashboard()
        {
            // getting the user's ID
            var userId = _userManager.GetUserId(User);

            // getting all bookings for my user//
            var bookings = _context.Bookings
                .Where(b => b.UserId == userId)
                .ToList();//

            //var bookings = _bookingRepository.GetBookingsOfUser(_context, userId);

            // getting all the related car IDs from the bookings
            var carIds = bookings.Select(b => b.CarId).Distinct().ToList();

            // getting the car details for the those ids
            var cars = _context.Cars
                .Where(c => carIds.Contains(c.Id))
                .ToDictionary(c => c.Id);

            // rental applications k under anay wala = Pending (currently ongoing)
            var currentBookings = bookings
                .Where(b => b.Status == "Pending" )
                .Select(b => new
                {
                    b.BookingId,
                    CarModel = cars.ContainsKey(b.CarId) ? cars[b.CarId].Model : "Unknown",
                    b.StartDate,
                    b.EndDate,
                    b.Status
                }).ToList();

            // history k under show krne walay = Rejected + Returned + Approved => (to show active history with "return" button)
            var bookingHistory = bookings
                .Where(b => b.Status == "Rejected" || b.Status == "Returned" || b.Status == "Approved" || b.Status == "Cancelled")
                .Select(b => new
                {
                    b.BookingId,
                    CarModel = cars.ContainsKey(b.CarId) ? cars[b.CarId].Model : "Unknown",
                    b.StartDate,
                    b.EndDate,
                    Duration = (b.EndDate - b.StartDate).Days,
                    b.Status
                }).ToList();


            ViewBag.CurrentBookings = currentBookings;
            ViewBag.BookingHistory = bookingHistory;

            return View();
        }


        [Authorize(Policy = "CustomerOnly")]
        public IActionResult Products()
        {
            var availableCars = _carRepository.GetAllCarsForCustomer(_context);
            return View(availableCars);
        }
        
        
        [Authorize(Policy = "CustomerOnly")]
        public IActionResult BookCar(int id)
        {
            var car = _carRepository.GetCarById(_context, id);
            if (car == null)
                return NotFound();

            return View(car);
        }

        
        [HttpPost]
        [Authorize(Policy = "CustomerOnly")]
        public async Task<IActionResult> BookCar(int carId, DateTime startDate, DateTime endDate)
        {
            var userId = _userManager.GetUserId(User);

            var booking = new Booking
            {
                CarId = carId,
                UserId = userId,
                StartDate = startDate,
                EndDate = endDate,
                Status = "Pending"
            };

            _context.Bookings.Add(booking);
            _context.SaveChanges();

            //_bookingRepository.MakeABooking(_context, userId, carId, startDate, endDate);
            await _hubContext.Clients.All.SendAsync("ManageApprovalsUpdate");

            return RedirectToAction("Dashboard", "Customer");
        }


        [HttpPost]
        [Authorize(Policy = "CustomerOnly")]
        public IActionResult CancelBooking(int bookingId)
        {
            //var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == bookingId && b.Status == "Pending");
            //if (booking == null)
            //    return NotFound();

            //booking.Status = "Cancelled";
            //_context.SaveChanges();

            _bookingRepository.CancelBooking( _context, bookingId);
            TempData["Success"] = "Booking cancelled successfully.";
            return RedirectToAction("Dashboard");
        }


        [HttpPost]
        [Authorize(Policy = "CustomerOnly")]
        public async Task<IActionResult> ReturnCar(int bookingId)
        {
            //var booking = _context.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
            //if (booking == null || booking.Status != "Approved")
            //    return NotFound();

            //booking.Status = "Returned";

            //var car = _context.Cars.FirstOrDefault(c => c.Id == booking.CarId);
            //if (car != null)
            //    car.IsAvailable = true;

            //_context.SaveChanges();

            _bookingRepository.ReturnCar( _context, bookingId);
            await _hubContext.Clients.All.SendAsync("ActiveRentalsUpdated");

            TempData["Success"] = "Car returned successfully.";
            return RedirectToAction("Dashboard");
        }



        [Authorize(Policy = "CustomerOnly")]
        public IActionResult ContactUs()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Policy = "CustomerOnly")]
        public IActionResult ContactUs(ContactUs contact)
        {
            if (ModelState.IsValid)
            {
                contact.SubmittedAt = DateTime.Now;
                _context.ContactUs.Add(contact);
                _context.SaveChanges();
                TempData["Success"] = "Message sent successfully!";
                return RedirectToAction("Dashboard", "Customer");
            }

            return View("Dashboard", "Customer");
        }
    }
}
