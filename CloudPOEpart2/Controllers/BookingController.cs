using CloudPOEpart2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class BookingController : Controller
{
    private readonly DataBaseContext _context;

    public BookingController(DataBaseContext context)
    {
        _context = context;
    }

    // Index method with Search functionality
    public async Task<IActionResult> Index(string searchTerm)
    {
        var bookingsQuery = _context.Booking
            .Include(b => b.Event)
            .Include(b => b.Venue)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            bookingsQuery = bookingsQuery.Where(b =>
                b.Event.EventName.Contains(searchTerm) ||
                b.Venue.VenueName.Contains(searchTerm) ||
                b.BookingDate.ToString().Contains(searchTerm));
        }

        var bookings = await bookingsQuery.ToListAsync();
        ViewData["CurrentFilter"] = searchTerm;

        return View(bookings);
    }

    // Method that handles booking creation
    public async Task<IActionResult> Create()
    {
        ViewBag.Events = new SelectList(await _context.Event.ToListAsync(), "EventID", "EventName");
        ViewBag.Venues = new SelectList(await _context.Venue.ToListAsync(), "VenueID", "VenueName");
        return View();
    }

    // Method that handles double booking logic
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Booking booking)
    {
        bool isDoubleBooked = _context.Booking.Any(b =>
            b.VenueID == booking.VenueID &&
            b.BookingDate.Date == booking.BookingDate.Date);

        if (isDoubleBooked)
        {
            ModelState.AddModelError("", "This venue is already booked for the selected date.");
        }

        if (ModelState.IsValid)
        {
            _context.Add(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Events = new SelectList(await _context.Event.ToListAsync(), "EventID", "EventName", booking.EventID);
        ViewBag.Venues = new SelectList(await _context.Venue.ToListAsync(), "VenueID", "VenueName", booking.VenueID);
        return View(booking);
    }

    
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var booking = await _context.Booking.FindAsync(id);
        if (booking == null)
            return NotFound();

        ViewData["Events"] = await _context.Event.ToListAsync();
        ViewData["Venues"] = await _context.Venue.ToListAsync();
        return View(booking);
    }

    // Method that handles editing functionality of bookings
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Booking booking)
    {
        if (id != booking.BookingID)
            return NotFound();

        bool isDoubleBooked = _context.Booking.Any(b =>
            b.VenueID == booking.VenueID &&
            b.BookingDate.Date == booking.BookingDate.Date &&
            b.BookingID != booking.BookingID); // exclude current booking

        if (isDoubleBooked)
        {
            ModelState.AddModelError("", "This venue is already booked for the selected date.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(booking);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookingExists(booking.BookingID))
                    return NotFound();
                else
                    throw;
            }
            return RedirectToAction(nameof(Index));
        }

        ViewData["Events"] = await _context.Event.ToListAsync();
        ViewData["Venues"] = await _context.Venue.ToListAsync();
        return View(booking);
    }

    // Method that handles deletion of bookings
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
            return NotFound();

        var booking = await _context.Booking
            .Include(b => b.Event)
            .Include(b => b.Venue)
            .FirstOrDefaultAsync(m => m.BookingID == id);

        if (booking == null)
            return NotFound();

        return View(booking);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var booking = await _context.Booking.FindAsync(id);
        if (booking != null)
        {
            _context.Booking.Remove(booking);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool BookingExists(int id)
    {
        return _context.Booking.Any(e => e.BookingID == id);
    }
}
