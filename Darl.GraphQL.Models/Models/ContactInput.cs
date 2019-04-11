using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class ContactInput
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Country { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }
        public string Source { get; set; }
        public string Notes { get; set; }
        public string Sector { get; set; }
        public bool IntroSent { get; set; }
    }
}
