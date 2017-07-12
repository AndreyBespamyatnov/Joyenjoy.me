using System;

namespace PhotoBooth.Models
{
    public class Email
    {
        public string To { get; set; }
        public Guid PhotoId { get; set; }
    }
}
