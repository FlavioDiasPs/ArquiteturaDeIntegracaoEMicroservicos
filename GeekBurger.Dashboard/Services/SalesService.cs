﻿using GeekBurger.Dashboard.Contract;
using GeekBurger.Dashboard.Interfaces.Repository;
using GeekBurger.Dashboard.Interfaces.Service;
using GeekBurger.Dashboard.Model;
using GeekBurger.Dashboard.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeekBurger.Dashboard.Service
{
    public class SalesService : ISalesService
    {
        private readonly ISalesRepository _salesRepository;
        public SalesService(ISalesRepository salesRepository)
        {
            _salesRepository = salesRepository;
        }

        public bool SaveSale()
        {
            return _salesRepository.Add(new Sales());
        }

        public IEnumerable<ConsolidatedSales> GetSales(Interval? per, int? value)
        {
            var sales = new List<Sales>();

            if (per.HasValue)
            {
                var dateRange = SetDateInterval(per.Value, value.Value);
                sales = _salesRepository.GetByInterval(dateRange.start, dateRange.end);
            }
            else
            {
                sales = _salesRepository.GetAll();
            }

            return ConsolidateSales(sales);
        }

        private static IEnumerable<ConsolidatedSales> ConsolidateSales(List<Sales> sales)
        {
            return sales
                .GroupBy(g => g.StoreId)
                .Select(x => new ConsolidatedSales
                {
                    StoredId = x.Key,
                    Total = x.Count(),
                    Value = x.Sum(s => s.Price)
                });
        }

        private static (DateTime start, DateTime end) SetDateInterval(Interval per, int value)
        {
            var date = DateTime.Now.Date;

            (DateTime start, DateTime end) dateRange;
            dateRange.start = new DateTime();
            dateRange.end = new DateTime();

            switch (per)
            {
                case Interval.Day:
                    dateRange.start = date.AddDays(value);
                    dateRange.end = dateRange.start.AddDays(1);
                    break;
                case Interval.Hour:
                    dateRange.start = date.AddHours(value);
                    dateRange.end = dateRange.start.AddHours(1);
                    break;
                case Interval.Minute:
                    dateRange.start = date.AddMinutes(value);
                    dateRange.end = dateRange.start.AddMinutes(1);
                    break;
                default:
                    break;
            }

            return dateRange;
        }
    }
}
