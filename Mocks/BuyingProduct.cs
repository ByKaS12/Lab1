using EndedTask.Models;
using System;
using System.Linq;

namespace EndedTask.Mocks
{
    static public class BuyingProduct
    {

        static  public void AddToCan(ApplicationDbContext context,Client client,Product product )
        {
            var h =  context.Orders.FirstOrDefault(u => u.ClientId == client.Id);
            if ( h == null) {


                Order order = new Order
                {
                    ClientId = client.Id,
                    OrderDate = DateTime.Today.Date,
                    OrderNumber = 0,
                    ShipmentDate = DateTime.Today.AddDays(14).Date,
                    Status = "New"
                };
                 context.Orders.Add(order);
                OrderItem orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = product.Id,
                    ItemsCount = 1,
                    ItemPrice = product.Price

                };
                context.OrderItems.Add(orderItem);
                context.SaveChanges();
            }
            else if(h!=null && h.OrderNumber==0)
            {
                
                var g = context.OrderItems.FirstOrDefault(u => u.ProductId == product.Id && u.OrderId == h.Id);
                if (g == null) {
                    OrderItem orderItem = new OrderItem
                    {
                        OrderId = h.Id,
                        ProductId = product.Id,
                        ItemsCount = 1,
                        ItemPrice = product.Price

                    };
                    context.OrderItems.Add(orderItem);
                    context.SaveChanges();
                    
                } else {
                    g.ItemsCount++;
                    context.Update(g);
                    context.SaveChanges();
                }
            }
            else { }
            }
        }
    }

