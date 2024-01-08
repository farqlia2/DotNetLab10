using Microsoft.AspNetCore.Http;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace DotNetLab10.Models
{

    public class Article
    {
        
        public int ArticleId { get; set; }
     
        [MinLength(2, ErrorMessage = "Too short name")]
  
        [MaxLength(25, ErrorMessage = " Too long name, do not exceed {0}")]
        public string Name { get; set; }


        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [Range(0.0, Double.MaxValue, ErrorMessage = "The field {0} must be greater than {1}.")]
        public double Price { get; set; }

        [DisplayName("Picture")]
        public string? PictureName { get; set; }

        public int CategoryId { get; set; }
        [DisplayName("Category")]
        public Category Category { get; set; }

        public ICollection<BasketItem> BasketItems { get; set; }
    }
}
