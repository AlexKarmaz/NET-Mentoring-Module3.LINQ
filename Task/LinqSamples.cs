// Copyright © Microsoft Corporation.  All Rights Reserved.
// This code released under the terms of the 
// Microsoft Public License (MS-PL, http://opensource.org/licenses/ms-pl.html.)
//
//Copyright (C) Microsoft Corporation.  All rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using SampleSupport;
using Task.Data;

// Version Mad01

namespace SampleQueries
{
	[Title("LINQ Module")]
	[Prefix("Linq")]
	public class LinqSamples : SampleHarness
	{

		private DataSource dataSource = new DataSource();

        [Category("Task")]
        [Title("Task 001")]
        [Description("Displays all customers with sum of orders total greater than X")]
        public void Linq001()
        {
            decimal x = 10000;
            var customersList = dataSource.Customers
                .Where(c => c.Orders.Sum(o => o.Total) > x)
                .Select(c => new
                {
                    CustomerName = c.CompanyName,
                    TotalSum = c.Orders.Sum(o => o.Total)
                });

            Console.WriteLine($"Customers with sum greater than {x}:");
            Console.WriteLine();
            foreach (var c in customersList)
            {
                ObjectDumper.Write(c);
            }

            x = 20000;

            Console.WriteLine();
            Console.WriteLine($"Customers with sum greater than {x}:");
            Console.WriteLine();
            foreach (var c in customersList)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Task")]
        [Title("Task 002")]
        [Description("For each customer, make a list of suppliers located in the same country and the same city")]
        public void Linq002()
        {
            var customersWithSuppliers = dataSource.Customers
                .Select(c => new
                {
                    Customer = c,
                    Suppliers = dataSource.Suppliers.Where(s => s.City == c.City && s.Country == c.Country)
                });

            Console.WriteLine($"Without grouping:");
            foreach (var c in customersWithSuppliers)
            {
                Console.Write($"Customer name: {c.Customer.CompanyName} ");
                Console.WriteLine($"city: {c.Customer.City}, country: {c.Customer.Country} ");
                Console.WriteLine("Suppliers: ");
                foreach (var s in c.Suppliers)
                {
                    Console.WriteLine($"Supplier name: {s.SupplierName}, city:{s.City}, country:{s.Country};");
                }
                Console.WriteLine();
            }

            var groupedCustomersWithSuppliers = dataSource.Customers.GroupBy(c => new { c.Country, c.City },
                g => new
                {
                    Customer = g, 
                    Suppliers = dataSource.Suppliers.Where(s => s.City == g.City && s.Country == g.Country)
                });

            Console.WriteLine($"With grouping by country and city:");
            foreach (var c in groupedCustomersWithSuppliers)
            {
                Console.Write($"Country: {c.Key.Country}, City: {c.Key.City}: ");
               
                Console.WriteLine();
                foreach (var g in c)
                {
                    Console.WriteLine($"Customer name: {g.Customer.CompanyName} ");

                    Console.WriteLine("Suppliers: ");
                    foreach (var s in g.Suppliers)
                    {
                        Console.WriteLine($"Supplier name: {s.SupplierName}, city:{s.City}, country:{s.Country};");
                    }
                }
                Console.WriteLine();
            }
        }

        [Category("Task")]
        [Title("Task 003")]
        [Description("Displays all customers who has order with total greater than X")]
        public void Linq003()
        {
            decimal x = 5000;
            var customers = dataSource.Customers.Where(c => c.Orders.Any(s => s.Total > x));

            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Task")]
        [Title("Task 004")]
        [Description("Displays all customers with their first orders month and year")]
        public void Linq004()
        {
            var customers = dataSource.Customers.Where(c => c.Orders.Any())
                .Select(c => new
                {
                    ClientName = c.CompanyName,
                    StartDate = c.Orders.Min(o => o.OrderDate)
                });

            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Task")]
        [Title("Task 005")]
        [Description("Displays all customers with their first orders month and year ordered by:year, month, sum of orders total, clientName")]
        public void Linq005()
        {
            var customers = dataSource.Customers.Where(c => c.Orders.Any())
                .Select(c => new
                {
                    ClientName = c.CompanyName,
                    StartDate = c.Orders.Min(o => o.OrderDate),
                    Total = c.Orders.Sum(o => o.Total)
                }).OrderByDescending(c => c.StartDate.Year)
                .ThenByDescending(c => c.StartDate.Month)
                .ThenByDescending(c => c.Total)
                .ThenBy(c => c.ClientName); ;

            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
            }
        }

        [Category("Task")]
        [Title("Task 006")]
        [Description("Displays all customers with not number postal code or without region or whithout operator's code")]
        public void Linq006()
        {
            var customers = dataSource.Customers.Where(
                c => c.PostalCode != null && c.PostalCode.Any(sym => sym < '0' || sym > '9')
                    || string.IsNullOrWhiteSpace(c.Region)
                    || c.Phone.FirstOrDefault() != '(');

            foreach (var c in customers)
            {
                ObjectDumper.Write(c);
            }
        }


        [Category("Task")]
        [Title("Task 007")]
        [Description("Groups products by categories then by units in stock > 0 then order by unitPrice")]
        public void Linq007()
        {
            var groupedProducts = dataSource.Products
                .GroupBy(p => p.Category)
                .Select(g => new
                {
                    Category = g.Key,
                    ProductsByStock = g.GroupBy(p => p.UnitsInStock > 0)
                    .Select( s => new {
                        HasInStock = s.Key,
                        Products = s.OrderBy(p => p.UnitPrice)
                    })
            });

            foreach (var p in groupedProducts)
            {
                ObjectDumper.Write(p,2);
            }
        }

        [Category("Task")]
        [Title("Task 008")]
        [Description("Groups products by price: Cheap, Average price, Expensive")]
        public void Linq008()
        {
            var groupedProducts = dataSource.Products
                .GroupBy(p => p.UnitPrice < 10.0000M ? "Cheap" 
                : p.UnitPrice < 50.0000M ? "Average" : "Expensive");

            foreach (var g in groupedProducts)
            {
                Console.WriteLine(g.Key);
                foreach (var t in g)
                {
                    ObjectDumper.Write(t);
                }
                Console.WriteLine();
            }
        }

        [Category("Task")]
        [Title("Task 009")]
        [Description("Counts average order sum and average client's intensity for every city")]
        public void Linq009()
        {
            var cityStats = dataSource.Customers.GroupBy(c => c.City)
                .Select(g => new
                {
                    City = g.Key,
                    AverageOrderSum = g.Average(s => s.Orders.Sum(o => o.Total)),
                    ClientIntensity = g.Average(s => s.Orders.Length)
                });

            foreach (var g in cityStats)
            {
                Console.WriteLine( $"City: {g.City}, Average order sum: {g.AverageOrderSum}, Client intensity: {g.ClientIntensity}.");
            }
        }

        [Category("Task")]
        [Title("Task 010")]
        [Description("Displays clients activity statistic by month (without year), by year and  by year and month")]
        public void Linq010()
        {
            var clientStats = dataSource.Customers
                .Select(c => new
                {
                   ClientId = c.CustomerID,
                   MonthsStatistic = c.Orders.GroupBy(o => o.OrderDate.Month)
                                    .Select(g => new
                                    {
                                        Month = g.Key,
                                        OrderCount = g.Count()
                                    }),
                   YearsStatistic = c.Orders.GroupBy(o => o.OrderDate.Year)
                                    .Select(g => new
                                    {
                                        Year = g.Key,
                                        OrderCount =g.Count()
                                    }),
                   YearsAndMonthStatistic = c.Orders.GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                                           .Select(g => new
                                           {
                                               Year = g.Key.Year,
                                               Month = g.Key.Month,
                                               OrderCount = g.Count()
                                           })

                });

            foreach (var g in clientStats)
            {
                Console.WriteLine($"ClientId: {g.ClientId}");
                Console.WriteLine("Months statistic:");
                foreach (var ms in g.MonthsStatistic)
                {
                    Console.WriteLine($"Month: {ms.Month} Orders count: {ms.OrderCount}");
                }
                Console.WriteLine("");
                Console.WriteLine("Years statistic:");
                foreach (var year in g.YearsStatistic)
                {
                    Console.WriteLine($"Year: {year.Year} Orders count: {year.OrderCount}");
                }
                Console.WriteLine("");
                Console.WriteLine("Year and month statistic:");
                foreach (var ym in g.YearsAndMonthStatistic)
                {
                    Console.WriteLine($"Year: {ym.Year} Month: {ym.Month} Orders count: {ym.OrderCount}");
                }
                Console.WriteLine("");
            }
        }
    }
}
