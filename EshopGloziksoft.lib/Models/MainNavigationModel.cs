using System.Web.Security;

namespace eshopgloziksoft.lib.Models
{
    public class MainNavigationModel
    {
        public MembershipUser User { get; set; }
        public _EshopModel Eshop { get; set; }
    }
}
