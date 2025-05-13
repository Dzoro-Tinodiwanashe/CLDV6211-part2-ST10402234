namespace CloudPOEpart2.Models
{
    public class Venue
    {
        public int VenueID { get; set; }
        public string VenueName { get; set; }
        public string Location { get; set; }
        public int Capacity { get; set; }
        public string ImageUrl { get; set; }

        // Add constructor to initialize non-nullable properties
        public Venue()
        {
            // Example: Initialize properties that are required.
            this.VenueName = string.Empty;
            this.Location = string.Empty;
            this.ImageUrl = string.Empty;
        }
    }
}
