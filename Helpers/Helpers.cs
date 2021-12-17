using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Collections;


namespace System.Web.Mvc
{
    public static class Helpers
    {
        public static MvcHtmlString DisplayWithIdFor<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression, string wrapperTag = "div")
        {
            var id = helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(ExpressionHelper.GetExpressionText(expression));
            return MvcHtmlString.Create(string.Format("<{0} id=\"{1}\">{2}</{0}>", wrapperTag, id, helper.DisplayFor(expression)));
        }
        
        public static bool Contains(this string input, string find, StringComparison comparisonType)
        {
            return String.IsNullOrWhiteSpace(input) ? false : input.IndexOf(find, comparisonType) > -1;
        }

        public static IQueryable<TSource> WhereIf<TSource>(this IQueryable<TSource> source, bool condition, Expression<Func<TSource, bool>> expression)
        {
            if (condition)
                return source.Where(expression);
            else
                return source;
        }

        public static IQueryable<T> WhereLike<T>(this IQueryable<T> source, String Name, String value, bool UpperCompare = false)
        {
            Type model = typeof(T);
            ParameterExpression param = Expression.Parameter(typeof(T), "m");
            PropertyInfo key = model.GetProperty(Name);
            MemberExpression lhs = Expression.MakeMemberAccess(param, key);

            if(lhs.Type.Name=="Int32")
            {
               return source.AsEnumerable().Where(Name + " = @0", Int32.Parse(value.Replace("%",""))).AsQueryable();
            }
            if (lhs.Type.Name == "Boolean")
            {
                bool var = false;
                value = value.Replace("%", "");
                try
                {
                    var = value.ToBoolean();
                }catch(Exception)
                {                   
                    Boolean.TryParse(value, out var);
                }
               
                return source.AsEnumerable().Where(Name + " = @0", var).AsQueryable();
            }
            if(lhs.Type.Name == "DateTime")
            {
                DateTime var = DateTime.Now;
                value = value.Replace("%", "");
                try
                {
                    var = Convert.ToDateTime(value);
                }
                catch (Exception)
                {
                    DateTime.TryParse(value, out var);
                }
                return source.AsEnumerable().Where( Name + ".Date = @0", var.Date).AsQueryable();
            }
            Expression<Func<T, String>> lambda = Expression.Lambda<Func<T, String>>(lhs, param);
            return source.Where(BuildLikeExpression(lambda, value, UpperCompare));
        }

        public static List<List<string>> ToArrayString(this System.Linq.IQueryable collection, Type tipo, bool BotonesAdelante = false)
        {
            //Type t = collection.Take(1).GetType();

            var propers = tipo.GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

            Dictionary<string, string> columnas = new Dictionary<string, string>();

            foreach (var it in propers)
            {
                //PropertyInfo prop = ((System.Reflection.TypeInfo)collection.ElementType).DeclaredProperties.AccessProperty(it.Name);

                int can = ((System.Reflection.TypeInfo)collection.ElementType).DeclaredProperties.Where(p => p.Name == it.Name).Count();

                if (can != 0)
                {
                    IEnumerable<KeyValuePair<string, System.Reflection.CustomAttributeData>> key_pro = (from p in it.CustomAttributes
                                                                                                        select new KeyValuePair<string, System.Reflection.CustomAttributeData>(p.AttributeType.Name, p));
                    string Name = null;
                    bool Show = true;
                    //string Valor = prop.GetValue(prop, null).ToString();
                    System.Reflection.CustomAttributeData DisplayAttribute = key_pro.SingleOrDefault(m => m.Key == "DisplayAttribute").Value;
                    if (DisplayAttribute != null)
                    {
                        var llamar = (from a in DisplayAttribute.NamedArguments
                                      where a.MemberName == "AutoGenerateField"
                                      select a.TypedValue.Value).FirstOrDefault();
                        Show = (bool)(llamar ?? true);

                    }
                    Name = it.Name;
                    if (Show)
                    {
                        columnas.Add(Name, Name);
                    }
                }

            }


            var resultado = collection.Select("new (" + (BotonesAdelante? "\" \" AS BOTONES,":"") + string.Join(",", columnas.Keys.ToList()) + ") ");


            List<List<string>> lista_main = new List<List<string>>();


            IEnumerable<PropertyInfo> pia = ((System.Reflection.TypeInfo)resultado.ElementType).DeclaredProperties; // t.GetProperties();

            //Populate the table
            foreach (var item in resultado)
            {
                List<string> lista = new List<string>();
                foreach (PropertyInfo pi in pia)
                {
                    lista.Add((pi.GetValue(item, null) ?? DBNull.Value).ToString());
                }
                lista_main.Add(lista);
            }
            return lista_main;
        }

        public static bool ToBoolean(this string value)
        {
            switch (value.ToLower())
            {
                case "true":
                case "verdadero":
                case "verdad":
                    return true;
                case "s":
                case "y":
                case "si":
                    return true;
                case "t":
                    return true;
                case "1":
                    return true;
                case "0":
                    return false;
                case "false":
                    return false;
                case "f":
                    return false;
                default:
                    return false;
            }
        }

        public static IQueryable<T> WhereLike<T>(this IQueryable<T> source, Expression<Func<T, String>> valueSelector, String value, bool UpperCompare = false)
        {
            return source.Where(BuildLikeExpression(valueSelector, value, UpperCompare));
        }
        public static Expression<Func<T, Boolean>> BuildLikeExpression<T>(Expression<Func<T, String>> valueSelector, String value, bool UpperCompare = false)
        {
            if (valueSelector == null)
                throw new ArgumentNullException("valueSelector");
            value = value.Replace("*", "%");        // this allows us to use '%' or '*' for our wildcard
            value = UpperCompare ? value.ToUpper() : value;
            if (value.Trim('%').Contains("%"))
            {
                Expression myBody = null;
                ParsedLike myParse = Parse(value);
                Type stringType = typeof(String);
                if (myParse.startwith != null)
                {
                    myBody = Expression.Call(valueSelector.Body, stringType.GetMethod("StartsWith", new Type[] { stringType }), Expression.Constant(myParse.startwith));
                }
                foreach (String contains in myParse.contains)
                {
                    if (myBody == null)
                    {
                        myBody = Expression.Call(valueSelector.Body, stringType.GetMethod("Contains", new Type[] { stringType }), Expression.Constant(contains));
                    }
                    else
                    {
                        Expression myInner = Expression.Call(valueSelector.Body, stringType.GetMethod("Contains", new Type[] { stringType }), Expression.Constant(contains));
                        myBody = Expression.And(myBody, myInner);
                    }
                }
                if (myParse.endwith != null)
                {
                    if (myBody == null)
                    {
                        myBody = Expression.Call(valueSelector.Body, stringType.GetMethod("EndsWith", new Type[] { stringType }), Expression.Constant(myParse.endwith));
                    }
                    else
                    {
                        Expression myInner = Expression.Call(valueSelector.Body, stringType.GetMethod("EndsWith", new Type[] { stringType }), Expression.Constant(myParse.endwith));
                        myBody = Expression.And(myBody, myInner);
                    }
                }
                return Expression.Lambda<Func<T, Boolean>>(myBody, valueSelector.Parameters.Single());
            }
            else
            {
                Type stringType = typeof(String);
                Expression myBody;

                if (UpperCompare)
                {
                    Expression myBody1 = Expression.Call(valueSelector.Body, "ToUpper", null);
                    myBody = Expression.Call(myBody1, GetLikeMethod(value), Expression.Constant(value.Trim('%')));
                }
                else
                {
                    myBody = Expression.Call(valueSelector.Body, GetLikeMethod(value), Expression.Constant(value.Trim('%')));
                }


                return Expression.Lambda<Func<T, Boolean>>(myBody, valueSelector.Parameters.Single());
            }
        }
        private static MethodInfo GetLikeMethod(String value)
        {
            Type stringType = typeof(String);

            if (value.EndsWith("%") && value.StartsWith("%"))
            {
                return stringType.GetMethod("Contains", new Type[] { stringType });
            }
            else if (value.EndsWith("%"))
            {
                return stringType.GetMethod("StartsWith", new Type[] { stringType });
            }
            else
            {
                return stringType.GetMethod("EndsWith", new Type[] { stringType });
            }
        }
        private class ParsedLike
        {
            public String startwith { get; set; }
            public String endwith { get; set; }
            public String[] contains { get; set; }
        }
        private static ParsedLike Parse(String inValue)
        {
            ParsedLike myParse = new ParsedLike();
            String work = inValue;
            Int32 loc;
            if (!work.StartsWith("%"))
            {
                work = work.TrimStart('%');
                loc = work.IndexOf("%");
                myParse.startwith = work.Substring(0, loc);
                work = work.Substring(loc + 1);
            }
            if (!work.EndsWith("%"))
            {
                loc = work.LastIndexOf('%');
                myParse.endwith = work.Substring(loc + 1);
                if (loc == -1)
                    work = String.Empty;
                else
                    work = work.Substring(0, loc);
            }
            myParse.contains = work.Split(new[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
            return myParse;
        }
        public static Expression<Func<TSource, Tkey>> GetPropertyExpression<TSource, Tkey>(this IQueryable<TSource> source, string propertyName)
        {
            if (typeof(TSource).GetProperty(propertyName, BindingFlags.IgnoreCase |
                BindingFlags.Public | BindingFlags.Instance) == null)
            {
                return null;
            }
            var paramterExpression = Expression.Parameter(typeof(TSource));
            return (Expression<Func<TSource, Tkey>>)
                Expression.Lambda(Expression.PropertyOrField(
                    paramterExpression, propertyName), paramterExpression);
        }
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, int prop, string order)
        {
            var type = typeof(T);
            var properties =  type.GetProperties().Where(a=>(a.CustomAttributes.Where(b => b.AttributeType.Name == "DisplayAttribute").Where(c => c.NamedArguments.Where(d=>d.MemberName== "AutoGenerateField").Count()>0).Count()>0));
            var property = type.GetProperties().Except(properties).ToArray()[prop];                          
            if (!new string[] { "String", "int32", "double", "Boolean", "DateTime", "System.Reflection.RuntimePropertyInfo" }.Contains(property.GetMethod.ReturnType.Name))
            {
                property = type.GetProperties().Except(properties).ToArray()[0];
            }
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable),
                order.Equals("asc", StringComparison.InvariantCultureIgnoreCase) ? "OrderBy" : "OrderByDescending",
                new Type[] { type, property.PropertyType }, source.Expression, Expression.Quote(orderByExp));
            return source.Provider.CreateQuery<T>(resultExp);
        }

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string prop, string order)
        {
            var type = typeof(T);
            var property = type.GetProperty(prop, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (!new string[] { "String", "int32", "double", "Boolean", "DateTime", "System.Reflection.RuntimePropertyInfo" }.Contains(property.GetMethod.ReturnType.Name))
            {
                var properties = type.GetProperties().Where(a => (a.CustomAttributes.Where(b => b.AttributeType.Name == "DisplayAttribute").Where(c => c.NamedArguments.Where(d => d.MemberName == "AutoGenerateField").Count() > 0).Count() > 0));
                property = type.GetProperties().Except(properties).ToArray()[0];
            }
            var parameter = Expression.Parameter(type, "p");
            var propertyAccess = Expression.MakeMemberAccess(parameter, property);
            var orderByExp = Expression.Lambda(propertyAccess, parameter);
            MethodCallExpression resultExp = Expression.Call(typeof(Queryable),
                order.Equals("asc", StringComparison.InvariantCultureIgnoreCase) ? "OrderBy" : "OrderByDescending",
                new Type[] { type, property.PropertyType }, source.Expression, Expression.Quote(orderByExp));
            return source.Provider.CreateQuery<T>(resultExp);
        }

        public static String RenderViewToString(ControllerContext context, String viewPath, object model = null)
        {
            context.Controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindView(context, viewPath, null);
                var viewContext = new ViewContext(context, viewResult.View, context.Controller.ViewData, context.Controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(context, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        public static DataTable ToDataTable<T>(this IEnumerable<T> collection, string tableName)
        {
            DataTable tbl = ToDataTable(collection);
            tbl.TableName = tableName;
            return tbl;
        }


        public static DataTable ToDataTable<T>(this IEnumerable<T> collection)
        {
            DataTable dt = new DataTable();
            Type t = typeof(T);
            PropertyInfo[] pia = t.GetProperties();       
                     
            //Create the columns in the DataTable
            foreach (PropertyInfo pi in pia)
            {
                dt.Columns.Add(pi.Name, Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType);
            }

      

            //Populate the table
            foreach (T item in collection)
            {
                DataRow dr = dt.NewRow();
                dr.BeginEdit();
                foreach (PropertyInfo pi in pia)
                {
                    dr[pi.Name] = pi.GetValue(item, null) ?? DBNull.Value;
                }
                dr.EndEdit();
                dt.Rows.Add(dr);
            }
            return dt;
        }

        public static DataTable ToDataTable(this IQueryable collection)
        {
            DataTable dt = new DataTable();
            Type t = collection.AsQueryable().Select("it").ElementType;
            PropertyInfo[] pia = t.GetProperties();

            if (pia.Length == 0)
            {
                t = collection.AsQueryable().Select("it").ElementType;
                pia = t?.GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                                 ?.Select(n => n)
                                 ?.ToArray();
            }

            //Create the columns in the DataTable
            foreach (PropertyInfo pi in pia)
            {
                dt.Columns.Add(pi.Name, Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType);
            }



            //Populate the table
            foreach (var item in collection.Cast<object>().ToList())
            {
                DataRow dr = dt.NewRow();
                dr.BeginEdit();
                foreach (PropertyInfo pi in pia)
                {
                    dr[pi.Name] = pi.GetValue(item, null) ?? DBNull.Value;
                }
                dr.EndEdit();
                dt.Rows.Add(dr);
            }
            return dt;
        }
        
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }

        public static IEnumerable<Filtro> Filtros_Request(this System.Web.HttpRequestBase Request)
        {

            var index_filtros = from s in Request.Params.AllKeys
                                where s.Contains("[search][value]")
                                let re = Request.Params.GetValues("columns[" + int.Parse(s.Substring(8, 3).Replace("[", "").Replace("]", "")) + "][search][value]").FirstOrDefault()
                                let col = Request.Params.GetValues("columns[" + int.Parse(s.Substring(8, 3).Replace("[", "").Replace("]", "")) + "][name]").FirstOrDefault()
                                where (re ?? "") != ""
                                select new Filtro() {index = s, ColName = col, cFilter = re };

            return index_filtros;
        }

        public static IEnumerable<OrderFiltro> Orders_Request(this System.Web.HttpRequestBase Request)
        {

            var index_orders = (from s in Request.Params.AllKeys
                                where s.Contains("order[")
                                let so = Request.Params.GetValues("order[" + s.Substring(6, 3).Replace("[", "").Replace("]", "") + "][dir]").FirstOrDefault()
                                let ci = Request.Params.GetValues("order[" + s.Substring(6, 3).Replace("[", "").Replace("]", "") + "][column]").FirstOrDefault()
                                let col = Request.Params.GetValues("columns[" + ci + "][name]").FirstOrDefault()
                                where (so ?? "") != ""
                                select new OrderFiltro() {
                                    index = s.Substring(6, 3).Replace("[", "").Replace("]", ""), ColName = col, ShortOrder = so
                                }).DistinctBy(p=>p.ColName);

            return index_orders;
        }

        public static int DiffMonths(this DateTime @from, DateTime @to)
        {
            return (((((@to.Year - @from.Year) * 12) + (@to.Month - @from.Month)) * 100 + @to.Day - @from.Day) / 100);
        }

        public static void GuardarLog(this Exception Error, HttpServerUtilityBase server, string Message = "", string Prefijo = "Error", bool Info =false)
        {
            string CarpetaLogs = "";
            try
            {
                CarpetaLogs = HttpContext.Current.Request.PhysicalApplicationPath;
            }
            catch (Exception)
            {
                CarpetaLogs = @"D:\log_mvc";
            }

            try
            {
                //var originalDirectory = new DirectoryInfo(string.Format("{0}Content\\Error\\Logs", server.MapPath(@"\")));
                string FullPath = System.IO.Path.Combine(CarpetaLogs, "Content\\Error\\Logs");

                bool isExists = System.IO.Directory.Exists(FullPath);
                if (!isExists)
                    System.IO.Directory.CreateDirectory(FullPath);


                string nombreArchivo = Prefijo + "_" + string.Format("{0:yyyyMMdd}", DateTime.Now);//+ "_"+ DateTime.Now.ToShortTimeString().Replace(":", "_").Replace(".", "");

                StreamWriter sw = new StreamWriter(FullPath + "\\" + nombreArchivo + ".txt", true);
                StringBuilder Cadena = new StringBuilder();
                //Cadena
                if(Info)
                {
                    Cadena.AppendLine("FECHA DEL EVENTO: " + DateTime.Now);
                    Cadena.AppendLine("DESCRIPCION: " + Error.Message);
                }
                else
                {
                    Cadena.AppendLine("FECHA DEL ERROR: " + DateTime.Now);
                    Cadena.AppendLine("ORIGEN DEL ERROR: " + (Error.Source != null ? Error.Source : ""));
                    Cadena.AppendLine("GENERADO EN " + (Error.TargetSite != null ? Error.TargetSite.Name : ""));
                    Cadena.AppendLine("DESCRIPCION: " + Error.Message);
                    Cadena.AppendLine("             " + Error.InnerException);
                    Cadena.AppendLine("PARAMETROS: ");
                    foreach (string item in Error.Data.Keys)
                    {
                        Cadena.AppendLine(item + ": " + Error.Data[item]);
                    }
                }                
                
                
                Cadena.AppendLine("MENSAJE : " + Message);
                Cadena.AppendLine("========================================");
                Cadena.AppendLine("");
                Cadena.AppendLine("");
                sw.Write(Cadena);
                sw.Close();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }


        // make properties of object accessible 
        // eg. x.AccessProperties() or x.AccessProperties()["PropName"]
        public static IDictionary AccessProperties(this object o, string propertyName = null)
        {
            Type type = o?.GetType();
            var properties = type?.GetProperties()
            ?.Select(n => n.Name)
            ?.ToDictionary(k => k, k => type.GetProperty(k));
            return properties;
        }

        public static Dictionary<string, Type> AccessProperties_Dictionary(this IQueryable o, string propertyName = null)
        {
            Type type = o.Select("it").ElementType;
            var properties = type?.GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            ?.Select(n => n.Name)
            ?.ToDictionary(k => k, k => Nullable.GetUnderlyingType(type.GetProperty(k).PropertyType) ?? type.GetProperty(k).PropertyType);
            return properties;
        }

        // returns specific property, i.e. x.AccessProperty(propertyName)
        public static PropertyInfo AccessProperty(this object o, string propertyName)
        {
            return (PropertyInfo)o?.AccessProperties()?[propertyName];
        }

        // returns specific property, i.e. x.AccessProperty(propertyName)
        public static object AccessPropertyValue(this object o, string propertyName)
        {
            return o?.AccessProperty(propertyName).GetValue(o, null);
        }

        public static string decToBase(long valor, Byte Base)
        {
            string car = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string ret = "";
            long _valor = valor;

            while (_valor >= Base)
            {
                int _mod = (int)_valor % Base;
                ret = car.Substring(_mod, 1) + ret;
                _valor = (long) Math.Truncate((Double)(_valor / Base));

            };

            ret = car.Substring((int)_valor, 1) + ret;

            return ret;
        }

        public static IDictionary<string, object> AddProperty(this object obj, string name, object value)
        {
            var dictionary = obj.ToDictionary();
            dictionary.Add(name, value);
            return dictionary;
        }

        public static IDictionary<string, object> ToDictionary(this object obj)
        {
            IDictionary<string, object> result = new Dictionary<string, object>();
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(obj);
            foreach (PropertyDescriptor property in properties)
            {
                result.Add(property.Name, property.GetValue(obj));
            }
            return result;
        }

        public static IDictionary<string, object> isDisabled(this object obj, bool disabled)
        {
            return disabled ? obj.AddProperty("disabled", "disabled") : obj.ToDictionary();
        }

        public static string Base64Decode(this string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static string FixBase64ForImage(this string Image)
        {
            System.Text.StringBuilder sbText = new System.Text.StringBuilder(Image, Image.Length);
            sbText.Replace("\r\n", string.Empty); sbText.Replace(" ", string.Empty);
            return sbText.ToString();
        }
    }

    public class Filtro
    { 
        public string index;
        public string ColName;
        public string cFilter;
    }

    public class OrderFiltro
    {
        public string index;
        public string ColName;
        public string ShortOrder;
    }


    public class FakeController : ControllerBase
    {
        protected override void ExecuteCore() { }
        public static string RenderViewToString(string controllerName, string viewName, object viewData)
        {
            using (var writer = new StringWriter())
            {
                var routeData = new RouteData();
                routeData.Values.Add("controller", controllerName);
                var fakeControllerContext = new ControllerContext(new HttpContextWrapper(new HttpContext(new HttpRequest(null, "http://google.com", null), new HttpResponse(null))), routeData, new FakeController());
                var razorViewEngine = new RazorViewEngine();
                var razorViewResult = razorViewEngine.FindView(fakeControllerContext, viewName, "", false);

                var viewContext = new ViewContext(fakeControllerContext, razorViewResult.View, new ViewDataDictionary(viewData), new TempDataDictionary(), writer);
                razorViewResult.View.Render(viewContext, writer);
                return writer.ToString();

            }
        }      
    }

}
