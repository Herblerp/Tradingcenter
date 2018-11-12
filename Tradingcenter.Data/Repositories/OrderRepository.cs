﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trainingcenter.Domain.DomainModels;
using Trainingcenter.Domain.Repositories;

namespace Tradingcenter.Data.Repositories
{
    class OrderRepository : IOrderRepository
    {
        #region DependecyInjection
        private readonly DataContext _context;

        public OrderRepository(DataContext context)
        {
            _context = context;
        }
        #endregion

        public async Task<List<Order>> GetOrdersFromUserIdAsync(int userId)
        {
            var orderList = await _context.Orders.Where(x => x.UserId == userId).ToListAsync();

            return orderList;
        }

        public async Task<List<Order>> GetOrdersFromPortfolioIdAsync(int portfolioId)
        {
            var orderList = await _context.Orders.Where(x => x.PortfolioId == portfolioId).ToListAsync();

            return orderList;
        }

        public async Task<List<Order>> GetOrdersFromUserIdAsync(int userId, DateTime dateFrom, DateTime dateTo)
        {
            var orderList = await _context.Orders.Where(x =>
                x.PortfolioId == userId &&
                x.Timestamp > dateFrom &&
                x.Timestamp < dateTo).ToListAsync();

            return orderList;
        }

        public async Task<List<Order>> GetOrdersFromPortfolioIdAsync(int portfolioId, DateTime dateFrom, DateTime dateTo)
        {
            var orderList = await _context.Orders.Where(x =>
                x.PortfolioId == portfolioId &&
                x.Timestamp > dateFrom &&
                x.Timestamp < dateTo).ToListAsync();

            return orderList;
        }
    }
}