/*
 *   Shikai Jin 
 *   sjin02@syr.edu
 *   SUID 844973756
 */
/*
 * Manual Page:
 * 
 * TypeRepository tr=TypeRepository.Instance; //Get the instance
 * TypeRepository.Instance.Reset();           //Set to the orginal status
 * TypeRepository.Instance.AddScope(scope);   //add a scope as a child to  tr.CurrentScope  
 * TypeRepository.Instance.EnterScope(scope); //change CurrentScope to scope
 * TypeRepository.Instance.LeaveScope();      //change CurrentScope to its parent
 * TypeRepository.Instance.PruneInvalidComposing(); // remove all non-struct compositions
 * TypeRepository.Instance.PruneBoring();           // remove all relationships between built-in types
 * TypeRepository.Instance.PruneIrrelevant();       // remove all relationships between unfound types
 * TypeRepository.Instance.RootScope;               // Getter of root scope
 * TypeRepository.Instance.CurrentScope;            // Getter of current scope
 * 
 * 
 * Maintenance History:
 * Ver 1.0  Oct. 4  2014 created by Shikai Jin 
 * 
 * Ver 2.0 Oct 7   
 * URL related
 * GetUrlInCurrentScope(string)
 * GetPureName(string)
 * 
 * Ver 2.1 Oct.9 
 * Add the feature that types like A.B can be tracked 
 * by checking the namespace info in FileHeaderInfo class in Parser.cs
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
/*
 *this repository is for all concerned stuffs including both types and replationships. 
 * it's organized in a tree
 */

    public class TypeRepository
    {
        //List<Scope> Namespaces = new List<Scope>();
        private static TypeRepository _instance = null;
        static public TypeRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TypeRepository();
                    _instance.Reset();
                }
                return _instance;
            }
        }

        //Set to the orginal status
        public void Reset()
        {
            _instance._rootscope = Scope.MakeRoot();
            _instance.CurrentScope = _instance._rootscope;
        }
        //add a scope as a child to  tr.CurrentScope  
        public Scope AddScope(Scope s)
        {
            Scope foundwithsamesig = null;
            foreach (Scope child in CurrentScope.Child)
            {
                if (child.HasSameSignature(s))
                {
                    foundwithsamesig = child;
                }
            }
            if (foundwithsamesig == null)
            {
                CurrentScope.AddChild(s);
                return s;
            }
            else
                return foundwithsamesig;
        }
        //change CurrentScope to s
        public void EnterScope(Scope s)
        {
            CurrentScope = s;
        }
        public void GoToRoot()
        {
            CurrentScope = RootScope;
        }
        //change CurrentScope to its parent
        public void LeaveScope()
        {
            CurrentScope = CurrentScope.Parent;
        }
        private Scope _rootscope;
        public Scope RootScope
        {
            get { return _rootscope; }
        }
        public Scope CurrentScope { get; set; }

        //List<string> _structs = Scope.FindChildWithType(, c=>c==SType.STRUCT);
        List<string> _structnames = null;
        List<string> _types=null;
        List<string> _boringtypes=null;
        void GetAllStruct()
        {
            List<Scope>  scopes = Scope.FindChildWithType(this.RootScope, c => (c == SType.STRUCT||c==SType.ENUM));
            _structnames = new List<string>();
            foreach (Scope s in scopes)
            {
                _structnames.Add(s.Name);
            }
        }
        void GetAllTypes()
        {
            _types = new List<string>();
            List<Scope> scopes = Scope.FindChildWithType(this.RootScope, 
                c => ScopeCategory.GeneralTypes.Contains(c));
            foreach (Scope s in scopes)
            {
                _types.Add(s.Property[PROPERTYKEY.URL]);
            }
        }
        void GetBoringTypes()
        {
            _boringtypes=new List<string>();
            foreach (TypeCode t in Enum.GetValues(typeof(TypeCode)))
            {
                _boringtypes.Add(t.ToString());
                _boringtypes.Add(t.ToString().ToLower());
                _boringtypes.Add(t.ToString()+"[]");
                _boringtypes.Add(t.ToString().ToLower() + "[]");
            }
            _boringtypes.Add("int");
        }
        delegate bool IsScopeType(Scope s);
        //used as a test to check if scope is struct
        bool _STATEMENT_COMPOSTION_To_DEL(Scope s)
        {
            if (s.Type == SType.STATEMENT_COMPOSTION)
                if (!_structnames.Contains(s.Name))
                    return true;
            return false;
        }
        //used as a test to check if scope is detected
        bool _IRRELEVANT_TO_DEL(Scope s)
        {
            if (s.IsGeneralRelationship())
            {
                if (_types.Contains(s.Name))
                    return false;
                else
                    return true;
            }
            return false;

        }
        //used as a test to check if scope is built-in type
        bool _BORING_TO_DEL(Scope s)
        {
            if (s.IsGeneralRelationship())
            {
                if (_boringtypes.Contains(s.Name.Replace(" ","")))
                {
                    return true;
                }
            }
            return false;
        }
        //remove scope satisfying the sctest
        private void _PruneInvalid(Scope s,IsScopeType sctest)
        {
            if (s.IsGeneralClass())
            {
                List<Scope> indextodelete = new List<Scope>();
                for (int i = 0; i < s.Child.Count; i++)
                {
                    Scope child = s.Child[i];
                    if (sctest(child))
                        indextodelete.Add(child);
                }
                for (int i = 0; i < indextodelete.Count; i++)
                {
                    s.Child.Remove(indextodelete[i]);
                }
            }
            foreach (Scope child in s.Child)
            {
                _PruneInvalid(child, sctest);
            }
        }
        // remove all non-struct compositions
        public void PruneInvalidComposing()
        {
            using ((System.IDisposable)_structnames)
            {
                GetAllStruct();
                _PruneInvalid(this.RootScope, _STATEMENT_COMPOSTION_To_DEL);
            }
        }
        //remove relationships with built-in types
        public void PruneBoring()
        {
            using ((System.IDisposable)_boringtypes)
            {
                GetBoringTypes();
                _PruneInvalid(this.RootScope, _BORING_TO_DEL);
            }
        }
        //remove relationships with types that are not found
        public void PruneIrrelevant()
        {
            using ((System.IDisposable)_types)
            {
                GetAllTypes();
                _PruneInvalid(this.RootScope, _IRRELEVANT_TO_DEL);
            }
        }
        //get name without the array sign [] and replace every <yada,ydada> by the same form <T,T>
        public string GetPureName(string str)
        {
            StringBuilder ret=new StringBuilder();
            bool InTemp = false, InBracket = false;
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i] == '[') InBracket = true;
                if (InTemp == false && InBracket == false)
                    ret.Append(str[i]);
                if (str[i] == ']') InBracket = false;
                
                if (str[i] == '<')
                {
                    InTemp = true;
                }
                if (InTemp == true)
                { 
                    if(str[i]==',') ret.Append("T,");
                    if (str[i] == '>')
                    {
                        ret.Append("T>");
                        InTemp = false;
                    }
                }

            }
            return ret.ToString();
        }
        //AAAA.BBBB.CCCC   it will divide this string into AAAA and .BBBB.CCCC
        public void GetNameToFind(string fullarg,out string tofind,out string nametoconcat)
        {
            StringBuilder strtofind = new StringBuilder();
            StringBuilder strconcat = new StringBuilder();
            int cursor = 0;
            for (; cursor < fullarg.Length; cursor++)
            {
                if (fullarg[cursor] == '.')
                    break;
                strtofind.Append(fullarg[cursor]);
            }
            for (; cursor < fullarg.Length; cursor++)
            {
                strconcat.Append(fullarg[cursor]);
            }
            tofind = strtofind.ToString();
            nametoconcat = strconcat.ToString();
        }
        //Get the full name of str in the current scope
        public string GetUrl(string str)
        {
            List<string> urllist = new List<string>();
            urllist.Add(str);
            Scope tmp = CurrentScope;
            while (true)
            {
                if (tmp == null)
                    break;
                if(tmp.Name.Length>0)
                    urllist.Add(tmp.Name);
                tmp = tmp.Parent;
            }
            urllist.Reverse();
            string ret = String.Join(".", urllist);
            return ret;
        }
        //Complete Urls for scope s
        public void CompleteInfo(Scope s)
        {
            if (ScopeCategory.GeneralVariables.Contains(s.Type))
            {
                TypeRepository.Instance.EnterScope(s.Parent);
                string urlt = FindFullUrl(s.Property[PROPERTYKEY.TYPE]);
                if (urlt.Length > 0)
                    s.Property[PROPERTYKEY.TYPEURL] = urlt;
                s.Property[PROPERTYKEY.URL] = GetUrl(s.Name);
            }
            else 
            { 
            }
        }
        //recursively run completeInfo
        public void CompleteUrlForEachScope(Scope s)
        {
            CompleteInfo(s);
            foreach (Scope child in s.Child)
            {
                CompleteUrlForEachScope(child);
            }
        }
        //judge if a type is struct
        public void CompleteStructInfo(Scope s)
        {
            using ((System.IDisposable)_structnames)
            {
                GetAllStruct();
                _CompleteStructInfo(s);
            }
        }
        //recursively call ComleteStructInfo
        public void _CompleteStructInfo(Scope s)
        {
            if (ScopeCategory.GeneralVariables.Contains(s.Type))
            {
                if (_structnames.Contains(s.Property[PROPERTYKEY.TYPE]))
                    s.Property[PROPERTYKEY.ISSTRUCT] = true.ToString();
                else
                    s.Property[PROPERTYKEY.ISSTRUCT] = false.ToString();
            }   
            foreach(Scope child in s.Child)
            {
                _CompleteStructInfo(child);
            }
        }
        //public void _CompleteUrlForEachRelation(Scope s)
        //{
        //    if (s.IsGeneralRelationship())
        //    {
        //        TypeRepository.Instance.EnterScope(s.Parent);
        //        string urlt = FindFullUrl(s.Name);
        //        if (urlt.Length > 0)
        //            s.Name = urlt;
        //    }
        //    foreach (Scope child in s.Child)
        //    {
        //        _CompleteUrlForEachRelation(child);
        //    }
        //}

        //public void On
        public string GetUsingNamespace(Scope s)
        {
            while (true)
            {
                if (s == null)
                    break;
                if (s.IsGeneralClass())
                {
                    return s.Property[PROPERTYKEY.USINGNAMESPACE];
                }
                s = s.Parent;
            }
            return "";
        }
        //track a symbol by searching the scope hierarchy info.
        public string FindFullUrl(string str)
        {
            str = GetPureName(str);
            string strtofind,strtoconcat;

            GetNameToFind(str, out strtofind, out strtoconcat);
            Scope tmp = CurrentScope;
            Scope found = null;
            while (true)
            {
                if (tmp == null)
                    break;
                if (GetPureName(tmp.Name) == strtofind)
                    found = tmp;
                foreach (Scope child in tmp.Child)
                {
                    if (!child.IsGeneralRelationship()
                        &&!ScopeCategory.GeneralVariables.Contains(child.Type))
                    {
                        if (GetPureName(child.Name) == strtofind)
                        {
                            found = child;
                        }
                    }
                }
                if (found != null)
                    break;
                tmp = tmp.Parent;
            }
            tmp = found;
            if (tmp == null)
            {
                string usingnamespace = GetUsingNamespace(CurrentScope);
                string[] splitnamespaces = usingnamespace.Split(',');
                List<Scope> namespacefound = Scope.FindChild(RootScope,
                    s => s.Type == SType.NAMESPACE && splitnamespaces.Contains(s.Name));
                if (namespacefound.Count > 0)
                {
                    List<Scope> foundchild = Scope.FindChild(namespacefound[0], s => s.IsGeneralClass() && s.Name == strtofind);
                    if (foundchild.Count > 0)
                    {
                        tmp = foundchild[0];
                    }
                }
            }
            if (tmp != null)
                return tmp.Url + strtoconcat;
            else
            return "";
        }
    }
    //not used yet
    delegate void CompleteFunc(Scope s);

    //Test Stub
#if(TEST_REPOSITORY)
    class TestRe
    {
        static void Main(string[] args)
        {
            TypeRepository tr = TypeRepository.Instance;
            Scope s = Scope.Make(SType.NAMESPACE, "SPACE1", 0, 0);
            tr.EnterScope(tr.AddScope(s));
            {
                Scope ss = Scope.Make(SType.CLASS, "CLASS1", 0, 0);
                tr.EnterScope(tr.AddScope(ss));

                Scope sss1 = Scope.Make(SType.FUNCTION, "FUNCTION1", 0, 0);
                tr.AddScope(sss1);

                Scope sss2 = Scope.Make(SType.FUNCTION, "FUNCTION2", 0, 0);
                tr.AddScope(sss2);

                tr.LeaveScope();

                Scope sss = Scope.Make(SType.CLASS, "CLASS2", 0, 0);
                tr.EnterScope(tr.AddScope(sss));
                tr.LeaveScope();
            }
            tr.LeaveScope();
            OutputManager.DisplayTypes(tr.CurrentScope);

            System.Console.WriteLine("");
            System.Console.WriteLine("");

            string mm="Dictionary<string,string>[]";
            System.Console.WriteLine(mm);
            System.Console.WriteLine(tr.GetPureName(mm));
            System.Console.ReadLine();
        }        
    }
#endif
}
