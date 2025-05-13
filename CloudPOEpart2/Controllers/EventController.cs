using CloudPOEpart2.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudPOEpart2.Controllers
{
    public class EventController : Controller
    {
        private readonly DataBaseContext _context;

        public EventController(DataBaseContext context)
        {
            _context = context;
        }

        // Index: Displays all events
        public async Task<IActionResult> Index()
        {
            var events = await _context.Event.Include(e => e.Venue).ToListAsync();
            return View(events);
        }

        public IActionResult Create()
        {
            ViewData["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event eventModel)
        {
            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("Model state is invalid.");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    System.Diagnostics.Debug.WriteLine(error.ErrorMessage);
                }
            }

            // Provide a default value for ImageUrl if not supplied
            if (string.IsNullOrEmpty(eventModel.ImageUrl))
            {
                eventModel.ImageUrl = "default-image-url.jpg";  // Default image URL (or empty string if preferred)
            }

            if (ModelState.IsValid)
            {
                _context.Add(eventModel);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName", eventModel.VenueID);
            return View(eventModel);
        }

        // Edit (GET): Display the form to edit an event
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventModel = await _context.Event.FindAsync(id);
            if (eventModel == null)
            {
                return NotFound();
            }

            ViewData["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName", eventModel.VenueID);
            return View(eventModel);
        }

        // Edit (POST): Handle the form submission to update an event
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Event eventModel)
        {
            if (id != eventModel.EventID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(eventModel);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(eventModel.EventID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName", eventModel.VenueID);
            return View(eventModel);
        }

        // Delete (GET): Display the form to confirm deletion of an event
        public async Task<IActionResult> Delete(int? id)
        {
            var eventModel = await _context.Event
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventID == id);
            if (eventModel == null)
            {
                return NotFound();
            }

            return View(eventModel);
        }

        // Delete (POST): Handle the deletion of an event
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventModel = await _context.Event.FindAsync(id);

            // Check for active bookings
            bool isLinked = await _context.Booking.AnyAsync(b => b.EventID == id);
            if (isLinked)
            {
                TempData["ErrorMessage"] = "Cannot delete this event because it is associated with existing bookings.";
                return RedirectToAction(nameof(Index));
            }

            if (eventModel != null)
            {
                _context.Event.Remove(eventModel);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.EventID == id);
        }
    }
}
