using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DotNetLab10.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        [MinLength(2)]
        [MaxLength(40)]
        [DisplayName("Category")]
        public string Name { get; set; }

        public ICollection<Article> Articles { get; set; }
    }
}
