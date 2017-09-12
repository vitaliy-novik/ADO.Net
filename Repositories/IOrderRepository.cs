using System;
using System.Collections.Generic;
using NorthwindDAL.Models;

namespace NorthwindDAL.Repositories
{
	public interface IOrderRepository
	{
		IEnumerable<Order> GetOrders();

		Order GetOrderById(int id);

		void Insert(Order newOrder);

		void Update(Order order);

		void Delete(Order order);

		void MarkReady(Order order);

		IEnumerable<CustOrderHist> RunCustOrderHist(string customerID);

	}
}
