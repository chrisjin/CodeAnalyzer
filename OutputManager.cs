/*
 *   Shikai Jin 
 *   sjin02@syr.edu
 *   SUID 844973756
 */
/*
 * Manual Page:
 * 
 * OutputManager.DisplayTypes(scope)            //Display all the children of scope recursively
 * OutputManager.DisplayReplationships(scope)   //Display all replationships related to it and its children
 * OutputManager.WriteTypes2XML(scope,filename) //Write all children of scope to filename in format of XML
 * 
 * Maintenance History:
 * Ver 1.0  Oct. 5  2014 created by Shikai Jin 
 * 
 * Ver 2.0 Oct 9 2014 
 * new way to display relationship
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace CodeAnalyzer
{
    /*
     * Display module
     * responsible for displaying stuffs in the repository in a certain user-friendly manner
     * 
     */
    class OutputManager
    {
        /*
         * 
         *Only display the following types in the repository   
         */
        static SType[] _concernedtypes = {/*SType.ARGUMENT,SType.MEMBER ,*/ SType.DELEGATE,SType.CLASS, SType.ENUM, SType.STRUCT, SType.INTERFACE, SType.NAMESPACE, SType.FUNCTION };
        /*
         * 
         *Display indents at the begining of each line
         */
        static void DisplayIndent(int num)
        {
            string str = "";
            for (int i = 0; i < num; i++)
            {
                str += "  ";
            }
            System.Console.Write(str);
        }
        /*
         * 
         *add paddings to the right of a string
         *
         */
        static void DisplayStringWithPaddingRight(string str, int padding)
        {
            System.Console.Write(str.PadRight(padding));
            if (str.Length >= padding)
                System.Console.Write(" ");
        }
        /*
         * 
         *Get  function argument
         */
        static string GetFunctionArgumentList(Scope s)
        {
            List<string> l = new List<string>();
            for (int i = 0; i < s.Child.Count; i++)
            {
                if(s.Child[i].Type==SType.ARGUMENT)
                    l.Add(s.Child[i].Name);
            }
            return "(" + String.Join(",", l) + ")";
        }
        /*
         * 
         *Display a function
         */
        static void DisplayFunction(Scope s)
        {
            if (s.IsGeneralFunction())
            {
                System.Console.Write("func ");
                //display its name
                //DisplayStringWithPaddingRight(s.Name, 17);
                string arglist = GetFunctionArgumentList(s);
                if (arglist.Length>0)
                {
                    DisplayStringWithPaddingRight(s.Name+" "+GetFunctionArgumentList(s), 40);
                }
                //System.Console.WriteLine("");
                //display function complexities and sizes
                //DisplayIndent(_indent);
                if (s.Property.ContainsKey(PROPERTYKEY.COMPLEXITY))
                    DisplayStringWithPaddingRight("cc: " + s.Property[PROPERTYKEY.COMPLEXITY], 7);
                DisplayStringWithPaddingRight("Size: " + (s.BeginLine.ToString() +"-"+ s.EndLine.ToString()), 10);
                //display function argument to tell apart the same function with different argumrnt table

                System.Console.WriteLine();
            }
        }
        /*
         * Display a concerned type
         *   
         */
        static void DisplayScope(Scope s)
        {

            if (s.IsGeneralFunction())
                DisplayFunction(s);
            else
                System.Console.WriteLine(s.Type.ToString().ToLower() + " " + s.Name);
        }
        /*
         * display all types recursively in the repository
         *   
         */
        static public void DisplayTypes(Scope s)
        {
            bool ShouldStayHereToWrite = _concernedtypes.Contains(s.Type);

            if (ShouldStayHereToWrite)
            {
                DisplayIndent(_indent);
                DisplayScope(s);
                _indent++;
            }
            //recursive call
            foreach (Scope child in s.Child)
            {
                DisplayTypes(child);
            }
            if (ShouldStayHereToWrite)
            {
                _indent--;
            }

        }
        //find relationship by searching the scope tree
        public static void FindRelationshipForClass(Scope s,
            out List<string> retinher, out List<string> retaggr, 
            out List<string> retcom, out List<string> retusin)
        {
            List<Scope> member = Scope.FindChildWithType(s, c => c == SType.MEMBER || c == SType.PROPERTY);
            List<Scope> argu = Scope.FindChildWithType(s, c => c == SType.ARGUMENT);

            Dictionary<string, string> inhe = new Dictionary<string, string>();
            inhe[s.Property[PROPERTYKEY.PARENTCLASS]] = "";

            Dictionary<string, string> usingre = new Dictionary<string, string>();
            for (int i = 0; i < argu.Count; i++)
            {
                if (argu[i].Property[PROPERTYKEY.TYPEURL].Length > 0)
                    usingre[argu[i].Property[PROPERTYKEY.TYPE]] = "";

            }

            Dictionary<string, string> aggre = new Dictionary<string, string>();
            Dictionary<string, string> comre = new Dictionary<string, string>();
            for (int i = 0; i < member.Count; i++)
            {
                if (member[i].Property[PROPERTYKEY.ISALLOCATED] == true.ToString()
                    && member[i].Property[PROPERTYKEY.TYPEURL].Length > 0)
                {
                    aggre[member[i].Property[PROPERTYKEY.TYPE]] = "";
                }
                else if (member[i].Property[PROPERTYKEY.ISSTRUCT] == true.ToString())
                {
                    comre[member[i].Property[PROPERTYKEY.TYPE]] = "";
                }
            }
            retinher = inhe.Keys.ToList();
            if(retinher.Count>0)
            {
                if(retinher[0]=="Object")
                    retinher=new List<string>();
            }
            retaggr = aggre.Keys.ToList();
            retcom = comre.Keys.ToList();
            retusin = usingre.Keys.ToList();
        }
        //show one relationship
        static void ShowRelationship(string str, List<string> re)
        {
            if (re.Count > 0)
            {
                DisplayIndent(3); Console.Write(str.PadRight(13));
                Console.WriteLine(String.Join(",",re));
            }
        }
        //show all the relationship with one class
        static void ShowRelationForClass(Scope s,
            List<string> retinher, List<string> retaggr,
            List<string> retcom, List<string> retusin)
        {
            int toshow = retinher.Count + retaggr.Count + retcom.Count + retusin.Count;
            if (toshow > 0)
            {
                System.Console.WriteLine(s.Type.ToString().ToLower() + " " + s.Name + ":");
                ShowRelationship("Inheritance: ", retinher);
                ShowRelationship("Aggragation: ", retaggr);
                ShowRelationship("Composition: ", retcom);
                ShowRelationship("Using: ", retusin);
            }
        }
        //new way to display replationship from just nothing but the tree infomation
        public static void DisplayRelationshipForClass(Scope s)
        {
            if (s.IsGeneralClass())
            {
                List<string> inheritance, aggragation, composition, usation;
                FindRelationshipForClass(s, out inheritance, out aggragation, 
                    out composition, out usation);
                ShowRelationForClass(s,inheritance,aggragation,composition,usation);
            }
            foreach (Scope chi in s.Child)
            {
                DisplayRelationshipForClass(chi);
            }
        }
        /*Obsolete!
         * Display a relationship in the repository
         *   
         */
        static  void DisplayARelationship(Scope s)
        {
            if (s.IsGeneralRelationship())
            {
                if (s.Parent == null)
                    return;
                DisplayStringWithPaddingRight(s.Parent.Type.ToString(), 10);
                DisplayStringWithPaddingRight(s.Parent.Name, 20);
                switch (s.Type)
                {
                    case SType.STATEMENT_INHERITANCE:
                        DisplayStringWithPaddingRight("INHERITING", 12);
                        break;
                    case SType.STATEMENT_USING:
                        DisplayStringWithPaddingRight("USING", 12);
                        break;
                    case SType.STATEMENT_COMPOSTION:
                        DisplayStringWithPaddingRight("COMPOSED OF", 12);
                        break;
                    case SType.STATEMENT_AGGREGATION:
                        DisplayStringWithPaddingRight("AGGREGATING", 12);
                        break;
                }
                DisplayStringWithPaddingRight(s.Name, 20);
                System.Console.WriteLine("");
            }
        }
        /*Obsolete!
         * display all relationships in the repository
         *   
         */
        static public void DisplayRelationships(Scope s)
        {
            if (s.IsGeneralRelationship())
            {
                DisplayARelationship(s);
            }
            foreach (Scope child in s.Child)
            {
                DisplayRelationships(child);
            }
        }
        static FileStream _filestream = null;
        static StreamWriter _streamwriter = null;
        static int _indent = 0;
        static int _indentstep = 5;
        /*
         * Initialize the xml writter
         *   
         */
        static void _InitializeWriter(string filename)
        {
            _filestream = new FileStream(filename, FileMode.Create);
            _streamwriter = new StreamWriter(_filestream, Encoding.Default);
            _indent = 0;
        }
        /*
         * finalize the xml writer
         *   
         */
        static void _finalizeWriter()
        {
            _streamwriter.Close();
            _filestream.Close();
            
            _indent = 0;
        }
        /*
         * 
         *   write indents to the file
         */
        static void _WriteIndent(int indent)
        {
            for (int i = 0; i < indent; i++)
                _streamwriter.Write(" ");
        
        }
        /*
         * 
         * to replace ilegal characters to XML required strings  
         */
        static string XMLString(string str)
        {
            string strtowrite = str.Replace("&", "&amp;").Replace("<", "&lt;");
            strtowrite = strtowrite.Replace(">", "&gt;")
                .Replace("\"", "&quot;").Replace("\'", "&apos;");
            return strtowrite;
        }
        /*
         * write a line to the file
         *   
         */
        static void _XMLWriteLine(string str, int indent=0)
        {
            if (indent != 0)
                _WriteIndent(indent);
            _streamwriter.WriteLine(str);
        }
        /*
         * append to the file
         *   
         */
        static void _XMLWrite(string str, int indent=0)
        {
            if (indent != 0)
                _WriteIndent(indent);
            _streamwriter.Write(str);
        }
        static void _XMLWriteBegin(string str)
        { 
        
        }
        static void _XMLWriteEnd(string str)
        {

        }
        /*
         * write the property of a node in the repository tree
         *   
         */
        static void _WritePropertyWithIndent(Scope s,int indent)
        {
            foreach (KeyValuePair<string, string> entry in s.Property)
            {
                _WriteIndent(indent);
                _XMLWrite("<" + XMLString(entry.Key) + ">");
                _XMLWrite(XMLString(entry.Value));
                _XMLWrite("</" + XMLString(entry.Key) + ">");
                _XMLWriteLine("");
            }
        }
        //write relationship to XML
        static void _WriteRelationship(string str, List<string> entry)
        {
            _XMLWrite("<" + str + ">");
            _XMLWrite(XMLString(String.Join(", ", entry)));
            _XMLWrite("</" + str + ">");
        }
        //write relationships of the current class to XML
        static void _WriteRelationForClass(Scope s,
        List<string> retinher, List<string> retaggr,
        List<string> retcom, List<string> retusin)
        {
            int toshow = retinher.Count + retaggr.Count + retcom.Count + retusin.Count;
            _WriteIndent(_indent); _XMLWrite("<RELATIONSHIP>");
            if (retinher.Count > 0)
            {
                _WriteIndent(_indent + _indentstep);
                _WriteRelationship("Inheritance", retinher);
                _XMLWriteLine("");
            }
            if (retaggr.Count > 0)
            {
                _WriteIndent(_indent + _indentstep);
                _WriteRelationship("Aggragation", retaggr);
                _XMLWriteLine("");
            }
            if (retcom.Count > 0)
            {
                _WriteIndent(_indent + _indentstep);
                _WriteRelationship("Composition", retcom);
                _XMLWriteLine("");
            }
            if (retusin.Count > 0)
            {
                _WriteIndent(_indent + _indentstep);
                _WriteRelationship("Using", retusin);
                _XMLWriteLine("");
            }
            if (toshow>0)
            _WriteIndent(_indent); 
            _XMLWrite("</RELATIONSHIP>");
        }
        /*
         * write to XML that make con true, recursively.
         * con is a lambda returning boolean 
         *   
         */
        static void _Write2XML(Scope s,Predicate<SType> con,bool isrel=false)
        {
            bool ShouldStayHereToWrite = con(s.Type);
            string midtag = null, begintag = null, endtag = null;
            if (ShouldStayHereToWrite)
            {
                midtag = XMLString(s.Type.ToString());
                begintag = "<" + midtag + ">";
                endtag = "<" + "/" + midtag + ">";
                _WriteIndent(_indent);
                _streamwriter.WriteLine(begintag);
                _indent += _indentstep;

                _WriteIndent(_indent);
                _streamwriter.WriteLine("<NAME>" + XMLString(s.Name) + "</NAME>");
                
                _WritePropertyWithIndent(s,_indent);
            }
            if (isrel&&s.IsGeneralClass())
            {
                List<string> inheritance, aggragation, composition, usation;
                FindRelationshipForClass(s, out inheritance, out aggragation,
                    out composition, out usation);
                _WriteRelationForClass(s,inheritance,aggragation,composition,usation);
            }
            foreach (Scope child in s.Child)
            {
                _Write2XML(child, con, isrel);
            }
            if (ShouldStayHereToWrite)
            {
                _indent -= _indentstep;
                _WriteIndent(_indent);
                _streamwriter.WriteLine(endtag);
            }
        }
        /*
         * execute all processes to write XML
         *   
         */
        static public void WriteTypes2XML(Scope s, string filename,bool isre=false)
        {
            try
            {
                _InitializeWriter(filename);
                _Write2XML(s, c => (_concernedtypes.Contains(c) || c == SType.ROOT), isre);
                _finalizeWriter();
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.ToString());
            }
        }

        //static public void WriteRelationship2XML(Scope s)

    }
    //TEST STUB!
#if(TEST_OUTPUTMANAGER)
    class TestOutputMgr
    {
        static void Main(string[] args)
        {
            OnAddAndEnterScope addenter = new OnAddAndEnterScope();
            OnAddScope addscope = new OnAddScope();
            OnLeanvingScope leavingscope = new OnLeanvingScope();
            addenter.Do(Scope.Make(SType.NAMESPACE, "anana", 0, 0));
            addenter.Do(Scope.Make(SType.CLASS, "Aname", 0, 0));
            leavingscope.Do(null);
            addenter.Do(Scope.Make(SType.CLASS, "Bname", 0, 0));
            addscope.Do(Scope.Make(SType.FUNCTION, "Func1", 0, 0));
            addscope.Do(Scope.Make(SType.FUNCTION, "Func2", 0, 0));
            leavingscope.Do(null);
            OutputManager.DisplayTypes(TypeRepository.Instance.RootScope);
            OutputManager.WriteTypes2XML(TypeRepository.Instance.RootScope,"Test_Output_Mgr.xml");
            System.Console.ReadLine();
        }

    };
#endif
}
