﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Project_MVC.Models
{
    public class CartItem
    {
        public string FlowerCode { get; set; }
        public string FlowerName { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
    }
}