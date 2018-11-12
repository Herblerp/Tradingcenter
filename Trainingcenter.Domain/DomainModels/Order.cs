﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Trainingcenter.Domain.DomainModels
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public int PortfolioId { get; set; }

        [Required]
        public string Exchange { get; set; }
        [Required]
        public string ExchangeOrderId { get; set; }
        [Required]
        public string Symbol { get; set; }
        [Required]
        public string Side { get; set; }
        [Required]
        public double OrderQty { get; set; }
        [Required]
        public string Currency { get; set; }
        [Required]
        public double Price { get; set; }
        public DateTime Timestamp { get; set; }

        public User User { get; set; }
        public Portfolio Portfolio { get; set; }

        public ICollection<Comment> Comments { get; set; }
    }
}
