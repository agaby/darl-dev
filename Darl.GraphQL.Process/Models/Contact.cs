using System;
using System.Collections.Generic;

namespace Darl.GraphQL.Models.Models
{
    public class Contact
    {
        public string Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Country { get; set; }

        public string Company { get; set; }

        public string Title { get; set; }

        public string Source { get; set; }

        public DateTime Created { get; set; }

        public string Notes { get; set; }

        public string Sector { get; set; }

        public bool IntroSent { get; set; } = false;

        public string IPAddress { get; set; }

        public List<Purchase> purchases { get; set; } = new List<Purchase>();
        public List<DarlLicense> licenses { get; set; } = new List<DarlLicense>();

    }
}
