using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Encuestas_Enter.Models
{
    public class TableParams
    {

        
        public string llave  { get; set; }
        public string llaveCustom { get; set; }
        public string id { get; set; }
        public string entity_id { get; set; }

        public bool ShowHistory { get; set; }
        private string _ActionHistory="History";
        public string ActionHistory { get { return _ActionHistory; } set { _ActionHistory = value; } }
        public string ControllerHistory { get; set; }


        public bool ShowDetails { get; set; }
        private string _ActionDetails = "Details";
        public string ActionDetails { get { return _ActionDetails; } set { _ActionDetails = value; } }
        public string ControllerDetails { get; set; }

        public string ClassDetails { get; set; }
        private string _TitleDetails = "Ver";
        public string TitleDetails { get { return _TitleDetails; } set { _TitleDetails = value; } }
        public bool NewPageDetails { get; set; }



        public bool ShowEdit { get; set; }
        private string _ActionEdit= "Edit";
        public string ActionEdit { get { return _ActionEdit; } set { _ActionEdit = value; } }
        public string ControllerEdit { get; set; }

        public string ClassEdit { get; set; }
        private string _TitleEdit = "Modificar";
        public string TitleEdit { get { return _TitleEdit; } set { _TitleEdit = value; } }

        public bool ShowDelete { get; set; }
        private string _ActionDelete = "Delete";
        public string ActionDelete { get { return _ActionDelete; } set { _ActionDelete = value; } }
        public string ControllerDelete { get; set; }

        public bool ShowCustom { get; set; }
        private string _ActionCustom = "Edit";
        public string ActionCustom { get { return _ActionCustom; } set { _ActionCustom = value; } }
        public string ControllerCustom { get; set; }

        public string ClassCustom { get; set; }
        private string _TitleCustom = "";
        public string TitleCustom { get { return _TitleCustom; } set { _TitleCustom = value; } }
        public bool NewPageCustom { get; set; }        
        public bool FunctionCustom { get; set; }


        private string _ParentPage = "";
        public string ParentPage { get { return _ParentPage; } set { _ParentPage = value; } }




    }
}