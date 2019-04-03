using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace codes_netCore.Models
{
    public class Country
    {
        public int Id { get; set; }
        [Required]
        [StringLength(2, MinimumLength = 2)]
        public string ISO3166_1_a2 { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 2)]
        public string Name { get; set; }
        [Required]
        public int Code { get; set; }

        public virtual ICollection<Network> Networks { get; set; }
        public virtual ICollection<Code> Codes { get; set; }
    }
}
