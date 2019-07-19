using AutoMapper;
using AutoMapper.QueryableExtensions;
using EFC.Issues.Context;
using EFC.Issues.Test.Mapping;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace EFC.Issues.Test
{
    [TestClass]
    public class SharedMultiColumnFKTest : EFCBaseTest
    {


        private static IMapper Mapper
        {
            get;
            set;
        }

        private static void InitMapper()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<Mapping.Profile>());

            Mapper = config.CreateMapper();
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            InitMapper();
            var inMemoryContext = GetEFCContext(DBProvider.InMemory);

            var sqlServerContext = GetEFCContext(DBProvider.SqlServer, SqlServerConnectionString);

            AddTestData(inMemoryContext);
            AddTestData(sqlServerContext);
        }

        private static void AddTestData(EFCContext context)
        {

            var clients = new[]
            {
                new CLIENT {CLIENT_ID ="CL99", CLIENT_NAME ="Client 99"},
                //new CLIENT {CLIENT_ID ="CL80", CLIENT_NAME ="Client 80"}
            };

            var clientContacts = new[]
            {
                new CLIENT_CONTACT { CLIENT_ID ="CL99", CONTACT_ID = 1, CONTACT_NAME = "Contact 1", CONTACT_EMAIL = "c1@client99.com" },
                new CLIENT_CONTACT { CLIENT_ID ="CL99", CONTACT_ID = 2, CONTACT_NAME = "Contact 2", CONTACT_EMAIL = "c2@client99.com" },
                new CLIENT_CONTACT { CLIENT_ID ="CL99", CONTACT_ID = 3, CONTACT_NAME = "Contact 3", CONTACT_EMAIL = "c3@client99.com" },
                new CLIENT_CONTACT { CLIENT_ID ="CL99", CONTACT_ID = 4, CONTACT_NAME = "Contact 4", CONTACT_EMAIL = "c4@client99.com" },
            };

            var orders = new[]
            {
                new ORDER{ ORDER_ID = 1, CLIENT_ID = "CL99", CREATION_DATE = DateTime.Now.Subtract(TimeSpan.FromDays(10))},
                new ORDER{ ORDER_ID = 2, CLIENT_ID = "CL99", CREATION_DATE = DateTime.Now.Subtract(TimeSpan.FromDays(5))},
                new ORDER{ ORDER_ID = 3, CLIENT_ID = "CL99", CREATION_DATE = DateTime.Now.Subtract(TimeSpan.FromDays(2))},
                new ORDER{ ORDER_ID = 4, CLIENT_ID = "CL99", CREATION_DATE = DateTime.Now},
            };

            var orderDetails = new[]
            {
                new ORDER_DETAIL { ORDER_ID = 1, CLIENT_ID = null , BILLING_TYPE = "B0" }, //no contact information
                new ORDER_DETAIL { ORDER_ID = 2, CLIENT_ID ="CL99", BILLING_CONTACT_ID = 1, SHIPPING_CONTACT_ID = 2, BILLING_TYPE = "B0"}, // Both contacts set
                new ORDER_DETAIL { ORDER_ID = 3, CLIENT_ID ="CL99", SHIPPING_CONTACT_ID = 3, BILLING_TYPE = "B1"}, //only Shipping contact
                new ORDER_DETAIL { ORDER_ID = 4, CLIENT_ID ="CL99", BILLING_CONTACT_ID = 4 ,BILLING_TYPE = "B3"}, //only billing contact

            };

            try
            {
                context.AddRange(clients);
                context.AddRange(clientContacts);
                context.AddRange(orders);
                context.AddRange(orderDetails);
                var result = context.SaveChanges();

                Console.WriteLine($"Test data added to {context.Database.ProviderName}, result => {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed {nameof(AddTestData)} {ex}");
            }
        }


        // Retrieve information from table ORDER_DETAIL and 
        // contacts table CLIENT_CONTACT without projecting.
        // 

        [DataTestMethod]
        [DataRow(DBProvider.InMemory)]
        [DataRow(DBProvider.SqlServer)]
        public void NoProjectionQueryTest(DBProvider dBProvider)
        {
            var context = GetEFCContext(dBProvider, SqlServerConnectionString);
            var orderIds = new int[] { 1, 2, 3, 4 };

            try
            {
                var orders = context.ORDER_DETAIL
                .Include(od => od.CLIENT_BILLING_CONTACT)
                .Include(od => od.CLIENT_SHIPPING_CONTACT)
                .Where(od => orderIds.Contains(od.ORDER_ID))
                .ToList();

                Assert.IsNotNull(orders, "no results");
                Assert.IsTrue(orders.Count == 4, $"Mismatched result count 4 != {orders.Count}");
                Assert.IsTrue(orders.FirstOrDefault(od => od.ORDER_ID == 1 && od.CLIENT_SHIPPING_CONTACT == null && od.CLIENT_BILLING_CONTACT == null) != null);
                Assert.IsTrue(orders.FirstOrDefault(od => od.ORDER_ID == 2 && od.CLIENT_SHIPPING_CONTACT != null && od.CLIENT_BILLING_CONTACT != null) != null);
                Assert.IsTrue(orders.FirstOrDefault(od => od.ORDER_ID == 3 && od.CLIENT_SHIPPING_CONTACT != null && od.CLIENT_BILLING_CONTACT == null) != null);
                Assert.IsTrue(orders.FirstOrDefault(od => od.ORDER_ID == 4 && od.CLIENT_SHIPPING_CONTACT == null && od.CLIENT_BILLING_CONTACT != null) != null);
            }
            catch (Exception ex) when (!(ex is AssertFailedException))
            {
                var msg = $"Unexpected exception {ex}";
                Console.WriteLine(msg);
                Assert.Fail(msg);
            }

        }



        /// <summary>
        /// This test should be behaving as <see cref="NoProjectionQueryTest(DBProvider)"/>
        /// </summary>
        /// <param name="dBProvider"></param>
        [DataTestMethod]
        [DataRow(DBProvider.InMemory)]
        //[DataRow(DBProvider.SqlServer)]
        public void ExplicitProjectionQueryTest(DBProvider dBProvider)
        {
            var context = GetEFCContext(dBProvider, SqlServerConnectionString);
            var orderIds = new int[] { 1, 2, 3, 4 };


            try
            {
                var orders = context.ORDER_DETAIL
                .Where(od => orderIds.Contains(od.ORDER_ID))
                .Select(od =>
               new
               {
                   OrderId = od.ORDER_ID,
                   ShippingContact = od.CLIENT_SHIPPING_CONTACT != null ?
                                     new
                                     {
                                         ClientId = od.CLIENT_SHIPPING_CONTACT.CLIENT_ID,
                                         Name = od.CLIENT_SHIPPING_CONTACT.CONTACT_NAME,
                                         ContactId = od.CLIENT_SHIPPING_CONTACT.CONTACT_ID
                                     }
                                     : null,
                   BillingContact = od.CLIENT_BILLING_CONTACT != null ?
                                     new
                                     {
                                         ClientId = od.CLIENT_BILLING_CONTACT.CLIENT_ID,
                                         Name = od.CLIENT_BILLING_CONTACT.CONTACT_NAME,
                                         ContactId = od.CLIENT_BILLING_CONTACT.CONTACT_ID
                                     }
                                     : null,
               }
                )
                .ToList();

                Assert.IsNotNull(orders, "no results");
                Assert.IsTrue(orders.Count == 4, $"Mismatched result count 4 != {orders.Count}");
                Assert.IsTrue(orders.FirstOrDefault(od => od.OrderId == 1 && od.ShippingContact == null && od.BillingContact == null) != null);
                Assert.IsTrue(orders.FirstOrDefault(od => od.OrderId == 2 && od.ShippingContact != null && od.BillingContact != null) != null);
                Assert.IsTrue(orders.FirstOrDefault(od => od.OrderId == 3 && od.ShippingContact != null && od.BillingContact == null) != null);
                Assert.IsTrue(orders.FirstOrDefault(od => od.OrderId == 4 && od.ShippingContact == null && od.BillingContact != null) != null);
            }
            catch (Exception ex) when (!(ex is AssertFailedException))
            {
                var msg = $"Unexpected exception {ex}";
                Console.WriteLine(msg);
                Assert.Fail(msg);
            }

        }


        [DataTestMethod]
        [DataRow(DBProvider.InMemory)]
        //[DataRow(DBProvider.SqlServer)]
        public void OneEmptyContactMappingQueryTest(DBProvider dBProvider)
        {

            var context = GetEFCContext(dBProvider, SqlServerConnectionString);
            var orderIds = new int[] { 3, 4 };


            try
            {
                var orders = context.ORDER_DETAIL
                    .Where(od => orderIds.Contains(od.ORDER_ID))
                    .ProjectTo<OrderDetail>(Mapper.ConfigurationProvider)
                    .ToList();

                Assert.IsNotNull(orders, "no results");
                Assert.IsTrue(orders.Count == 2, $"Mismatched result count 2 != {orders.Count}");
                Assert.IsTrue(orders.FirstOrDefault(od => od.OrderId == 3 && od.ShippingContact != null && od.BillingContact == null) != null);
                Assert.IsTrue(orders.FirstOrDefault(od => od.OrderId == 4 && od.ShippingContact == null && od.BillingContact != null) != null);
            }
            catch (Exception ex) when (!(ex is AssertFailedException))
            {
                var msg = $"Unexpected exception {ex}";
                Console.WriteLine(msg);
                Assert.Fail(msg);
            }
        }


        [DataTestMethod]
        [DataRow(DBProvider.InMemory)]
        //[DataRow(DBProvider.SqlServer)]
        public void OneEmptyContactExplicitProjectionQueryTest(DBProvider dBProvider)
        {
            var context = GetEFCContext(dBProvider, SqlServerConnectionString);
            var orderIds = new int[] { 3, 4 };


            try
            {
                var orders = context.ORDER_DETAIL
                .Where(od => orderIds.Contains(od.ORDER_ID))
                .Select(od =>
               new
               {
                   OrderId = od.ORDER_ID,
                   ShippingContact = od.CLIENT_SHIPPING_CONTACT != null ?
                                     new
                                     {
                                         ClientId = od.CLIENT_SHIPPING_CONTACT.CLIENT_ID,
                                         Name = od.CLIENT_SHIPPING_CONTACT.CONTACT_NAME
                                     ,
                                         ContactId = od.CLIENT_SHIPPING_CONTACT.CONTACT_ID
                                     }
                                     : null,
                   BillingContact = od.CLIENT_BILLING_CONTACT != null ?
                                     new
                                     {
                                         ClientId = od.CLIENT_BILLING_CONTACT.CLIENT_ID,
                                         Name = od.CLIENT_BILLING_CONTACT.CONTACT_NAME
                                     ,
                                         ContactId = od.CLIENT_BILLING_CONTACT.CONTACT_ID
                                     }
                                     : null,
               }
                )
                .ToList();

                Assert.IsNotNull(orders, "no results");
                Assert.IsTrue(orders.Count == 2, $"Mismatched result count 2 != {orders.Count}");
                Assert.IsTrue(orders.FirstOrDefault(od => od.OrderId == 3 && od.ShippingContact != null && od.BillingContact == null) != null);
                Assert.IsTrue(orders.FirstOrDefault(od => od.OrderId == 4 && od.ShippingContact == null && od.BillingContact != null) != null);
            }
            catch (Exception ex) when (!(ex is AssertFailedException))
            {
                var msg = $"Unexpected exception {ex}";
                Console.WriteLine(msg);
                Assert.Fail(msg);
            }

        }


        [DataTestMethod]
        [DataRow(DBProvider.InMemory)]
        public void NoContactOrBothMappingQueryTest(DBProvider dBProvider)
        {

            var context = GetEFCContext(dBProvider, SqlServerConnectionString);
            var orderIds = new int[] { 1, 2 };


            try
            {
                var orders = context.ORDER_DETAIL
                .Where(od => orderIds.Contains(od.ORDER_ID))
                .ProjectTo<OrderDetail>(Mapper.ConfigurationProvider)
                .ToList();

                Assert.IsNotNull(orders, "no results");
                Assert.IsTrue(orders.Count == 2, $"Mismatched result count 2 != {orders.Count}");
                Assert.IsTrue(orders.FirstOrDefault(od => od.OrderId == 1 && od.ShippingContact == null && od.BillingContact == null) != null);
                Assert.IsTrue(orders.FirstOrDefault(od => od.OrderId == 2 && od.ShippingContact != null && od.BillingContact != null) != null);
            }
            catch (Exception ex) when (!(ex is AssertFailedException))
            {
                var msg = $"Unexpected exception {ex}";
                Console.WriteLine(msg);
                Assert.Fail(msg);
            }
        }


        [DataTestMethod]
        [DataRow(DBProvider.InMemory)]
        //[DataRow(DBProvider.SqlServer)]
        public void NoContactOrBothExplicitProjectionQueryTest(DBProvider dBProvider)
        {
            var context = GetEFCContext(dBProvider, SqlServerConnectionString);
            var orderIds = new int[] { 1, 2 };


            try
            {
                var orders = context.ORDER_DETAIL
                .Where(od => orderIds.Contains(od.ORDER_ID))
                .Select(od =>
               new
               {
                   OrderId = od.ORDER_ID,
                   ShippingContact = od.CLIENT_SHIPPING_CONTACT != null ?
                                     new
                                     {
                                         ClientId = od.CLIENT_SHIPPING_CONTACT.CLIENT_ID,
                                         Name = od.CLIENT_SHIPPING_CONTACT.CONTACT_NAME
                                     ,
                                         ContactId = od.CLIENT_SHIPPING_CONTACT.CONTACT_ID
                                     }
                                     : null,
                   BillingContact = od.CLIENT_BILLING_CONTACT != null ?
                                     new
                                     {
                                         ClientId = od.CLIENT_BILLING_CONTACT.CLIENT_ID,
                                         Name = od.CLIENT_BILLING_CONTACT.CONTACT_NAME
                                     ,
                                         ContactId = od.CLIENT_BILLING_CONTACT.CONTACT_ID
                                     }
                                     : null,
               }
                )
                .ToList();

                Assert.IsNotNull(orders, "no results");
                Assert.IsTrue(orders.Count == 2, $"Mismatched result count 2 != {orders.Count}");
                Assert.IsTrue(orders.FirstOrDefault(od => od.OrderId == 1 && od.ShippingContact == null && od.BillingContact == null) != null);
                Assert.IsTrue(orders.FirstOrDefault(od => od.OrderId == 2 && od.ShippingContact != null && od.BillingContact != null) != null);
            }
            catch (Exception ex) when (!(ex is AssertFailedException))
            {
                var msg = $"Unexpected exception {ex}";
                Console.WriteLine(msg);
                Assert.Fail(msg);
            }

        }



        [ClassCleanup]
        public static void ClassCleanup()
        {

            try
            {
                //quick cleanup
                var sqlServerContext = GetEFCContext(DBProvider.SqlServer, SqlServerConnectionString);

                sqlServerContext.Database.ExecuteSqlCommand("DELETE FROM [ORDER_DETAIL]");
                sqlServerContext.Database.ExecuteSqlCommand("DELETE FROM [CLIENT_CONTACT]");
                sqlServerContext.Database.ExecuteSqlCommand("DELETE FROM [ORDER]");
                sqlServerContext.Database.ExecuteSqlCommand("DELETE FROM [CLIENT]");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while cleaning up {ex}");
            }
        }
    }
}
