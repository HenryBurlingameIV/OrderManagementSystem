using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderManagementSystem.Shared.Authorization
{
    public static class Permissions
    {
        public static class Catalog
        {
            public const string Create = "Catalog.Create";
            public const string Update = "Catalog.Update";
            public const string Delete = "Catalog.Delete";
        }

        public static class Orders
        {
            public const string Create = "Orders.Create";
            public const string ReadAll = "Orders.ReadAll";
            public const string ReadOwn = "Orders.ReadOwn";
            public const string CancelOwn = "Orders.CancelOwn";
            public const string UpdateStatusAll = "Orders.UpdateStatusAll";
        }

        public static class OrderProcessing
        {

            public const string Read = "OrderProcessing.Read";
            public const string BeginAssembly = "OrderProcessing.BeginAssembly";
            public const string BeginDelivery = "OrderProcessing.BeginDelivery";
        }

        public static class Users
        {
            public const string Create = "Users.Create";
            public const string ReadAll = "Users.ReadAll";
            public const string ReadOwn = "Users.ReadOwn";
            public const string UpdateAll = "Users.UpdateAll";
            public const string UpdateOwn = "Users.UpdateOwn";
            public const string Block = "Users.Block";
            public const string Unblock = "Users.Unblock";
            public const string AssignRoles = "Users.AssignRoles";
            public const string UnassignRoles = "Users.UnassignRoles";
        }
    }
}
