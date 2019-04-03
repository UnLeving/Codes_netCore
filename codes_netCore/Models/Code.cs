using System.ComponentModel.DataAnnotations;

namespace codes_netCore.Models
{
    public class Code
    {
        public int Id { get; set; }
        public string Value { get; set; }
        [Required]
        public string R { get; set; }
        public int CountryId { get; set; }
        public int NetworkId { get; set; }
        public virtual Country Country { get; set; }
        public virtual Network Network { get; set; }
    }
}
