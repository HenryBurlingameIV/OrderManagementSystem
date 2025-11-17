using AuthService.Domain.Entities;
using AuthService.Infrastructure.Services;
using OrderManagementSystem.Shared.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AuthService.Infrastructure.Data
{
    public static class SeedData
    {
        public const string AdminHashedPassword = "$2a$11$AP30ALTeWRMpPxFLVS3yEeLhVt13hIfvbrUXRMQI4Ije9OwEgmKbC"; //Admin123!
        public static readonly Dictionary<string, Permission> AllPermissions = new()
        {
            // Catalog
            [Permissions.Catalog.Create] = new Permission { Id = 1, Name = Permissions.Catalog.Create },
            [Permissions.Catalog.Update] = new Permission { Id = 2, Name = Permissions.Catalog.Update },
            [Permissions.Catalog.Delete] = new Permission { Id = 3, Name = Permissions.Catalog.Delete },

            // Orders
            [Permissions.Orders.Create] = new Permission { Id = 4, Name = Permissions.Orders.Create },
            [Permissions.Orders.ReadAll] = new Permission { Id = 5, Name = Permissions.Orders.ReadAll },
            [Permissions.Orders.ReadOwn] = new Permission { Id = 6, Name = Permissions.Orders.ReadOwn },
            [Permissions.Orders.CancelOwn] = new Permission { Id = 7, Name = Permissions.Orders.CancelOwn },
            [Permissions.Orders.UpdateStatusAll] = new Permission { Id = 8, Name = Permissions.Orders.UpdateStatusAll },

            // OrderProcessing
            [Permissions.OrderProcessing.Read] = new Permission { Id = 9, Name = Permissions.OrderProcessing.Read },
            [Permissions.OrderProcessing.BeginAssembly] = new Permission { Id = 10, Name = Permissions.OrderProcessing.BeginAssembly },
            [Permissions.OrderProcessing.BeginDelivery] = new Permission { Id = 11, Name = Permissions.OrderProcessing.BeginDelivery },

            // Users
            [Permissions.Users.Create] = new Permission { Id = 12, Name = Permissions.Users.Create },
            [Permissions.Users.ReadAll] = new Permission { Id = 13, Name = Permissions.Users.ReadAll },
            [Permissions.Users.ReadOwn] = new Permission { Id = 14, Name = Permissions.Users.ReadOwn },
            [Permissions.Users.UpdateAll] = new Permission { Id = 15, Name = Permissions.Users.UpdateAll },
            [Permissions.Users.UpdateOwn] = new Permission { Id = 16, Name = Permissions.Users.UpdateOwn },
            [Permissions.Users.Block] = new Permission { Id = 17, Name = Permissions.Users.Block },
            [Permissions.Users.Unblock] = new Permission { Id = 18, Name = Permissions.Users.Unblock },
            [Permissions.Users.AssignRoles] = new Permission { Id = 19, Name = Permissions.Users.AssignRoles },
            [Permissions.Users.UnassignRoles] = new Permission { Id = 20, Name = Permissions.Users.UnassignRoles }
        };

        public static Permission[] GetPermissionsForSeed()
        {
            return AllPermissions.Values.OrderBy(p => p.Id).ToArray();
        }

        public static Role[] GetRolesForSeed()
        {
            var roles = new Role[]
            {
                new Role()
                {
                    Id = 1,
                    Name = Roles.Admin
                },
                new Role()
                {
                    Id = 2,
                    Name = Roles.Manager
                },
                new Role()
                {
                    Id = 3,
                    Name = Roles.Client
                }
            };
            return roles;
        }
        public static object[] GetRolePermissionsForSeed()
        {
            var rolePermissions = new List<object>();

            // Admin permissions
            var adminPermissions = new[]
            {
                Permissions.Catalog.Create, Permissions.Catalog.Update, Permissions.Catalog.Delete,
                Permissions.Orders.ReadAll, Permissions.Orders.UpdateStatusAll,
                Permissions.OrderProcessing.Read, Permissions.OrderProcessing.BeginAssembly,
                Permissions.OrderProcessing.BeginDelivery,
                Permissions.Users.Create, Permissions.Users.ReadAll, Permissions.Users.UpdateAll,
                Permissions.Users.Block, Permissions.Users.Unblock, Permissions.Users.AssignRoles,
                Permissions.Users.UnassignRoles
            };

            foreach (var permissionName in adminPermissions)
            {
                rolePermissions.Add(new { RoleId = 1, PermissionsId = AllPermissions[permissionName].Id });
            }

            // Manager permissions
            var managerPermissions = new[]
            {
                Permissions.Catalog.Create, Permissions.Catalog.Update, Permissions.Catalog.Delete,
                Permissions.Orders.ReadAll, Permissions.Orders.UpdateStatusAll,
                Permissions.OrderProcessing.Read, Permissions.OrderProcessing.BeginAssembly,
                Permissions.OrderProcessing.BeginDelivery
            };

            foreach (var permissionName in managerPermissions)
            {
                rolePermissions.Add(new { RoleId = 2, PermissionsId = AllPermissions[permissionName].Id });
            }

            // Client permissions
            var clientPermissions = new[]
            {
                Permissions.Orders.Create, Permissions.Orders.ReadOwn, Permissions.Orders.CancelOwn,
                Permissions.Users.ReadOwn, Permissions.Users.UpdateOwn
            };

            foreach (var permissionName in clientPermissions)
            {
                rolePermissions.Add(new { RoleId = 3, PermissionsId = AllPermissions[permissionName].Id });
            }

            return rolePermissions.ToArray();
        }

        public static User GetAdminUser()
        {
            var adminUser = new User
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), 
                Name = "Administrator",
                Email = "admin@system.com",
                HashedPassword = AdminHashedPassword,
                IsActive = true,
                Roles = new List<Role>()
            };

            return adminUser;
        }

        public static object[] GetUserRolesForSeed()
        {
            return new object[]
            {
                new { UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"), RolesId = 1 } 
            };
        }
    }  
}
