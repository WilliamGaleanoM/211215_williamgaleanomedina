using System.Web;
using System.Web.Mvc;

namespace _211215_williamgaleanomedina
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
