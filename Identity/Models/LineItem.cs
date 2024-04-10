using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Identity.Models
{
    public partial class LineItem
    {
        [Key]
        public int TransactionId { get; set; }

        public byte? ProductId { get; set; }

        public byte? Qty { get; set; }

        public byte? Rating { get; set; }
    }
}
