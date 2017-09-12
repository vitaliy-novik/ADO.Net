using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using NorthwindDAL.Models;

namespace NorthwindDAL.Repositories
{
	public class OrderRepository : IOrderRepository
	{
		private readonly DbProviderFactory ProviderFactory;
		private readonly string ConnectionString;

		public OrderRepository(string connectionString, string provider)
		{
			ProviderFactory = DbProviderFactories.GetFactory(provider);
			ConnectionString = connectionString;
		}

		public IEnumerable<Order> GetOrders()
		{
			List<Order> resultOrders = new List<Order>();

			using (var connection = ProviderFactory.CreateConnection())
			{
				connection.ConnectionString = ConnectionString;
				connection.Open();

				using (var command = connection.CreateCommand())
				{
					command.CommandText = "SELECT * FROM Northwind.Orders";
					command.CommandType = CommandType.Text;

					using (var reader = command.ExecuteReader())
					{
						while (reader.Read())
						{
							resultOrders.Add(this.ParseOrder(reader));
						}
					}
				}
			}

			return resultOrders;
		}

		public Order GetOrderById(int id)
		{
			using (var connection = ProviderFactory.CreateConnection())
			{
				connection.ConnectionString = ConnectionString;
				connection.Open();

				using (var command = connection.CreateCommand())
				{
					command.CommandText =
						"select * from Northwind.Orders where OrderID = @id; " +
						"select * from Northwind.[Order Details] where OrderID = @id";
					command.CommandType = CommandType.Text;

					DbParameter paramId = command.CreateParameter();
					paramId.ParameterName = "@id";
					paramId.Value = id;
					command.Parameters.Add(paramId);

					using (var reader = command.ExecuteReader())
					{
						if (!reader.HasRows)
						{
							return null;
						}

						reader.Read();
						Order order = this.ParseOrder(reader);
						reader.NextResult();
						order.Details = new List<OrderDetail>();
						while (reader.Read())
						{
							OrderDetail detail = new OrderDetail();
							detail.UnitPrice = (decimal)reader["unitPrice"];
							detail.Quantity = (short)reader["Quantity"];

							order.Details.Add(detail);
						}

						return order;
					}
				}
			}
		}

		public void Insert(Order order)
		{
			using (var connection = ProviderFactory.CreateConnection())
			{
				connection.ConnectionString = ConnectionString;
				connection.Open();

				using (var command = connection.CreateCommand())
				{
					command.CommandText = "SET IDENTITY_INSERT Northwind.Orders ON INSERT INTO Northwind.Orders (OrderID,CustomerID,EmployeeID,OrderDate,RequiredDate,ShippedDate,ShipVia,Freight,ShipName,ShipAddress,ShipCity,ShipRegion,ShipPostalCode,ShipCountry)" +
						"VALUES (@OrderID,@CustomerID,@EmployeeID,@OrderDate,@RequiredDate,@ShippedDate,@ShipVia,@Freight,@ShipName,@ShipAddress,@ShipCity,@ShipRegion,@ShipPostalCode,@ShipCountry)";
					command.CommandType = CommandType.Text;
					this.AddParameter(command, "@OrderID", order.OrderID);
					this.AddParameter(command, "@CustomerID", order.CustomerID);
					this.AddParameter(command, "@EmployeeID", order.EmployeeID);
					this.AddParameter(command, "@OrderDate", order.OrderDate);
					this.AddParameter(command, "@RequiredDate", order.RequiredDate);
					this.AddParameter(command, "@ShippedDate", order.ShippedDate);
					this.AddParameter(command, "@ShipVia", order.ShipVia);
					this.AddParameter(command, "@Freight", order.Freight);
					this.AddParameter(command, "@ShipName", order.ShipName);
					this.AddParameter(command, "@ShipAddress", order.ShipAddress);
					this.AddParameter(command, "@ShipCity", order.ShipCity);
					this.AddParameter(command, "@ShipRegion", order.ShipRegion);
					this.AddParameter(command, "@ShipPostalCode", order.ShipPostalCode);
					this.AddParameter(command, "@ShipCountry", order.ShipCountry);
					command.ExecuteNonQuery();
				}
			}
		}

		public void Update(Order order)
		{
			using (var connection = ProviderFactory.CreateConnection())
			{
				connection.ConnectionString = ConnectionString;
				connection.Open();

				using (var command = connection.CreateCommand())
				{
					command.CommandText = "UPDATE Northwind.Orders " + 
									"SET ShipName=@ShipName,ShipAddress=@ShipAddress,ShipCity=@ShipCity,ShipRegion=@ShipRegion,ShipPostalCode=@ShipPostalCode,ShipCountry=@ShipCountry " + 
									"WHERE OrderID=@OrderID";
					command.CommandType = CommandType.Text;
					this.AddParameter(command, "@OrderID", order.OrderID);
					this.AddParameter(command, "@RequiredDate", order.RequiredDate);
					this.AddParameter(command, "@ShipName", order.ShipName);
					this.AddParameter(command, "@ShipAddress", order.ShipAddress);
					this.AddParameter(command, "@ShipCity", order.ShipCity);
					this.AddParameter(command, "@ShipRegion", order.ShipRegion);
					this.AddParameter(command, "@ShipPostalCode", order.ShipPostalCode);
					this.AddParameter(command, "@ShipCountry", order.ShipCountry);
					command.ExecuteNonQuery();
				}
			}
		}

		public void Delete(Order order)
		{
			using (var connection = ProviderFactory.CreateConnection())
			{
				connection.ConnectionString = ConnectionString;
				connection.Open();

				using (var command = connection.CreateCommand())
				{
					command.CommandText = "DELETE FROM Northwind.[Order Details] " +
										"WHERE OrderID=@OrderID; " +
										"DELETE FROM Northwind.Orders " +
										"WHERE OrderID=@OrderID";
					command.CommandType = CommandType.Text;
					this.AddParameter(command, "@OrderID", order.OrderID);
					command.ExecuteNonQuery();
				}
			}
		}

		public void MarkReady(Order order)
		{
			if (order.OrderStatus < Status.Ready)
			{
				using (var connection = ProviderFactory.CreateConnection())
				{
					connection.ConnectionString = ConnectionString;
					connection.Open();

					using (var command = connection.CreateCommand())
					{
						command.CommandText = "UPDATE Northwind.Orders SET " +
						                      $"ShippedDate='{DateTime.Now}'" +
						                      "WHERE OrderID=@OrderID";
						command.CommandType = CommandType.Text;
						this.AddParameter(command, "@OrderID", order.OrderID);
						command.ExecuteNonQuery();
					}
				}
			}
		}

		public IEnumerable<CustOrderHist> RunCustOrderHist(string customerID)
		{
			using (var connection = ProviderFactory.CreateConnection())
			{
				connection.ConnectionString = ConnectionString;
				connection.Open();

				using (var command = connection.CreateCommand())
				{
					command.CommandType = CommandType.StoredProcedure;
					command.CommandText = "Northwind.CustOrderHist";
					this.AddParameter(command, "@CustomerID", customerID);

					using (var reader = command.ExecuteReader())
					{
						List<CustOrderHist> list = new List<CustOrderHist>();

						while (reader.Read())
						{
							list.Add(new CustOrderHist
							{
								ProductName = reader.GetString(0),
								Total = reader.GetInt32(1)
							});
						}

						return list;
					}
				}

			}
		}

		private void AddParameter(DbCommand command, string name, object value)
		{
			DbParameter param = command.CreateParameter();
			param.ParameterName = name;
			param.Value = value ?? DBNull.Value;
			command.Parameters.Add(param);
		}

		public Order ParseOrder(DbDataReader dataReader)
		{
			Order order = new Order
			{
				OrderID = dataReader.GetInt32(0),
				CustomerID = dataReader[1] as string,
				EmployeeID = dataReader[2] as int?,
				OrderDate = dataReader[3] as DateTime?,
				RequiredDate = dataReader[4] as DateTime?,
				ShippedDate = dataReader[5] as DateTime?,
				ShipVia = dataReader[6] as int?,
				Freight = dataReader[7] as decimal?,
				ShipName = dataReader[8] as string,
				ShipAddress = dataReader[9] as string,
				ShipCity = dataReader[10] as string,
				ShipRegion = dataReader[11] as string,
				ShipPostalCode = dataReader[12] as string,
				ShipCountry = dataReader[13] as string,
			};

			return order;
		}
	}
}
