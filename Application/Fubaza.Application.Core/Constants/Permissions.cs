using System.ComponentModel;


namespace Fubaza.Application.Core.Constants
{
    public static class Permissions
    {
        [DisplayName("Dashboard")]
        [Description("Dashboard Permissions")]
        public static class Dashboard
        {
            public const string View = "Permissions.Dashboard.View";
        }

        [DisplayName("User Temepletes")]
        [Description("User Temepletes Permissions")]
        public static class UserTemepletes
        {
            public const string View = "Permissions.UserTemepletes.View";
        }

        [DisplayName("Overview")]
        [Description("Overview Permissions")]
        public static class Overview
        {
            public const string Clubs_View = "Permissions.Overview.Clubs.View";
            public const string Players_View = "Permissions.Overview.Players.View";
        }

        [DisplayName("Templates")]
        [Description("Templates Permissions")]
        public static class Templates
        {
            public const string View = "Permissions.Templates.View";
            public const string Create = "Permissions.Templates.Create";
            public const string Edit = "Permissions.Templates.Edit";
            public const string Delete = "Permissions.Templates.Delete";
            public const string Bulk_Approve = "Permissions.Templates.BulkApprove";
            public const string View_GraphicDesigner_Filter = "Permissions.Templates.View_GraphicDesigner_Filter";
        }

        [DisplayName("Users")]
        [Description("Users Permissions")]
        public static class Users
        {
            public const string View = "Permissions.UserManagement.Users.View";
            public const string Create = "Permissions.UserManagement.Users.Create";
            public const string Edit = "Permissions.UserManagement.Users.Edit";
            public const string Delete = "Permissions.UserManagement.Users.Delete";
            public const string ChangePassword = "Permissions.UserManagement.Users.ChangePassword";
        }

        [DisplayName("Roles")]
        [Description("Roles Permissions")]
        public static class Roles
        {
            public const string View = "Permissions.UserManagement.Roles.View";
            public const string Create = "Permissions.UserManagement.Roles.Create";
            public const string Edit = "Permissions.UserManagement.Roles.Edit";
            public const string Delete = "Permissions.UserManagement.Roles.Delete";
            public const string AssignPermission = "Permissions.UserManagement.Roles.AssignPermission";
        }
    }
}
