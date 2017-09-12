using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NorthwindDAL.Models;
using NorthwindDAL.Repositories;

namespace NorthwindDAL.Test
{
	[TestClass]
	public class OrderRepositoryTests
	{
		string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=Northwind;Integrated Security=True;Connect Timeout=60;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
		string providerName = "System.Data.SqlClient";

		[TestMethod]
		public void GetAll_Test()
		{
			OrderRepository orderRepository = new OrderRepository(connectionString, providerName);
			var list = orderRepository.GetOrders();

			Assert.IsTrue(list.Any());
		}

		[TestMethod]
		public void GetById_Test()
		{
			int id = 10248;
			OrderRepository orderRepository = new OrderRepository(connectionString, providerName);
			Order order = orderRepository.GetOrderById(id);

			Assert.IsNotNull(order);
		}

		[TestMethod]
		public void Insert_Test()
		{
			OrderRepository orderRepository = new OrderRepository(connectionString, providerName);
			var list = orderRepository.GetOrders().ToList();
			Order order = list[3];
			order.OrderID = 3000;
			orderRepository.Insert(order);

			Order insertedOrder = orderRepository.GetOrderById(order.OrderID);
			Assert.IsNotNull(insertedOrder);
		}

		[TestMethod]
		public void Update_Test()
		{
			OrderRepository orderRepository = new OrderRepository(connectionString, providerName);
			var list = orderRepository.GetOrders();
			Order order = list.First();
			order.ShipCity = "Minsk";
			orderRepository.Update(order);

			Order updatedOrder = orderRepository.GetOrderById(order.OrderID);
			Assert.AreEqual(updatedOrder.ShipCity, "Minsk");
		}

		[TestMethod]
		public void Delete_Test()
		{
			OrderRepository orderRepository = new OrderRepository(connectionString, providerName);
			var list = orderRepository.GetOrders();
			Order order = list.First(o => o.OrderStatus == Status.New);

			orderRepository.Delete(order);

			Assert.IsNull(orderRepository.GetOrderById(order.OrderID));
		}

		[TestMethod]
		public void MarkReady_Test()
		{
			OrderRepository orderRepository = new OrderRepository(connectionString, providerName);
			var list = orderRepository.GetOrders();
			Order order = list.First(o => o.OrderStatus == Status.InProgress || o.OrderStatus == Status.New);

			orderRepository.MarkReady(order);

			Assert.IsTrue(orderRepository.GetOrderById(order.OrderID).OrderStatus == Status.Ready);
		}

		[TestMethod]
		public void RunCustOrderHist_Test()
		{
			OrderRepository orderRepository = new OrderRepository(connectionString, providerName);
			var result = orderRepository.RunCustOrderHist("PERIC");

			Assert.IsTrue(result.Any());
		}
	}
}
