using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Identity.Models
{
    public partial class ItemRec
    {
        [Key] 
        public byte product_ID { get; set; }
        public byte recommendation_1 { get; set; }
        public byte recommendation_2 { get; set; }
        public byte recommendation_3 { get; set; }
        public byte recommendation_4 { get; set; }
        public byte recommendation_5 { get; set; }
    }
}