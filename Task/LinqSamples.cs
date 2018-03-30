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
        [Title("Task001")]
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
        [Title("Task002")]
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
    }
}
