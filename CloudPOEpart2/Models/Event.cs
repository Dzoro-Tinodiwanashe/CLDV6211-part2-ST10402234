// Event.cs
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace CloudPOEpart2.Models
{
    public class Event
    {
        public int EventID { get; set; }

        [Required]
        public string EventName { get; set; }

        public string? Description { get; set; }
        public DateTime? EventDate { get; set; }
        public string? ImageUrl { get; set; }

        [Required]
        public int VenueID { get; set; }

        [ValidateNever] 
        public Venue Venue { get; set; } 
    }
}
