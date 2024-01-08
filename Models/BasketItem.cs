using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System;

namespace DotNetLab10.Models
{
    public class BasketItem
    {
        
        public int Id { get; set; }
        public int ArticleId { get; set; }

        public int Count { get; set; }

        public string Name { get; set; }


        [DisplayFormat(DataFormatString = "{0:C}", ApplyFormatInEditMode = false)]
        [Range(0.0, Double.MaxValue, ErrorMessage = "The field {0} must be greater than {1}.")]
        public double Price { get; set; }

        [DisplayName("Picture")]
        public string? PictureName { get; set; }

        [DisplayName("Category")]
        public Category Category { get; set; }

        public Article Article { get; set; }

        public BasketItem(int articleId, int count)
        {
            ArticleId = articleId;
            Count = count;
        }
    }
}
