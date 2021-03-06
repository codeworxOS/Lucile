﻿using System;
using Lucile.Test.Model;

namespace Tests
{
    public class ArticleStatistics : EntityBase
    {
        public Guid ArticleId { get; set; }

        public string ArticleNumber { get; internal set; }

        public decimal SoldCurrentMonth { get; set; }

        public decimal SoldCurrentYear { get; set; }

        public decimal SoldLastMonth { get; set; }
    }
}