using System;
using System.ComponentModel.DataAnnotations;

namespace CloudPOEpart2.Models
{
    public class Booking
    {
        public int BookingID { get; set; }

        [Required]
        public int EventID { get; set; }

        [Required]
        public int VenueID { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; }

        // Navigation properties — no [Required] here!
        public Event? Event { get; set; }
        public Venue? Venue { get; set; }
    }
}
