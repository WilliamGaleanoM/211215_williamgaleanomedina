using System.Web;
using System.Web.Optimization;

namespace _211215_williamgaleanomedina
{
    public class BundleConfig
    {
        // Para obtener más información sobre Bundles, visite http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Utilice la versión de desarrollo de Modernizr para desarrollar y obtener información. De este modo, estará
            // preparado para la producción y podrá utilizar la herramienta de compilación disponible en http://modernizr.com para seleccionar solo las pruebas que necesite.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/estilos").Include(
                    "~/Content/DataTables/css/responsive.dataTables.css",
                    "~/Content/DataTables/css/buttons.dataTables.min.css",
                    "~/Content/DataTables/css/colReorder.dataTables.min.css",
                    "~/Content/DataTables/css/autoFill.bootstrap.css",
                    "~/Content/DataTables/css/jquery.dataTables.min.css"                    
                    ));

            bundles.Add(new ScriptBundle("~/bundles/datatable").Include(
                      "~/Scripts/DataTables/jquery.dataTables.js",
                      "~/Scripts/DataTables/dataTables.bootstrap.js",
                      "~/Scripts/DataTables/dataTables.responsive.js",
                      "~/Scripts/DataTables/dataTables.buttons.min.js",
                      "~/Scripts/DataTables/fnFilterOnReturn.js",
                      "~/Scripts/DataTables/buttons.html5.min.js",
                      "~/Scripts/DataTables/buttons.colVis.min.js",
                      "~/Scripts/DataTables/dataTables.colReorder.min.js",
                      "~/Scripts/DataTables/dataTables.autoFill.min.js",
                      "~/Scripts/DataTables/responsive.bootstrap.min.js"));

            bundles.Add(new StyleBundle("~/Content/fontawesome").Include(
                   "~/Content/fontawesome-all.css"
                   ));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/mybootstrap.css",
                      "~/Content/site.css"));
        }
    }
}
