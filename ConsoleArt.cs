/*
 *   Shikai Jin 
 *   sjin02@syr.edu
 *   SUID 844973756
 */
/*NOT IN USE not complete
 * Manual Page:
 * 
 * ConsoleArt ca=new ConsoleArt(); //used to draw tables or diagram on console
 * 
 * 
 * Maintenance History:
 * 
 * Ver 1.0  Oct. 8     2014 
 * ConsoleArt created by Shikai Jin
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
 * 
 */
namespace CodeAnalyzer
{
    /*
     * designed to be show complex structure on the console
     */
    class ConsoleArt
    {

    }
    /*
     * designed to be show forms on the console
     */

    class ConsoleForm
    {
        class Column
        {
            List<string> _items = new List<string>();
            //row item
            public List<string> Item
            {
                get { return _items; }
            }
            //add row
            public void AddItem(string str)
            {
                _items.Add(str);
            }
            //largest width
            public int GetMaximumWidth()
            {
                int ret = -1;
                for (int i = 0; i < _items.Count; i++)
                {
                    if (_items[i].Length > ret)
                        ret = _items[i].Length;
                }
                return ret;
            }
            //make long string short
            public string AbbreviateString(string a, int width)
            {
                if (a.Length < width)
                    return a;
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < width - 1; i++)
                {
                    if (i >= a.Length)
                        break;
                    sb.Append(a[i]);
                }
                sb.Append('~');
                return sb.ToString();
            }
            //make long string to a specific width
            public void AbbreviateToWidth(int width)
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    _items[i] = AbbreviateString(_items[i],width);
                }
            }
            public bool IsAbbreviatable = false;
        }
        List<Column> _columns = new List<Column>();
        List<int> _widthforeachcolumn = new List<int>();
        public bool DisplayVerticalLine =false; 
        public bool DisplayHorizontalLine = false;
        public bool IsStretch = false;
        public int ConsoleWidth = 80;
        public int Indent = 0;
        public int Padding = 0;
        public int Width { get { return ConsoleWidth - Indent - Padding; } }
        public double AbbrThreshold = 0.3;
        public int RealWidth = 0;
        //add columns
        public void SetColumnNum(int num)
        {
            _columns.Clear();
            for (int i = 0; i < num;i++ )
                _columns.Add(new Column());
        }
        //set column abbreviatable
        public void SetColumnAbbreviatable(int index,bool isabbr=true)
        {
            if (index < _columns.Count)
            {
                _columns[index].IsAbbreviatable = isabbr;
            }
        }
        //add row
        public void AddRow(List<string> strs)
        { 
            //int colnum=Math.Min(strs.Count,_columns.Count);
            for (int i = 0; i < _columns.Count; i++)
            {
                if (i < strs.Count)
                    _columns[i].AddItem(strs[i]);
                else
                    _columns[i].AddItem("-");
            }
        }
        //evaluate proper width for each column
        int EvalRealWidth()
        {
            int total = 0;
            for (int i = 0; i < _widthforeachcolumn.Count; i++)
            {
                total += _widthforeachcolumn[i];
            }
            return total;
        }
        //evaluate proper width for each column
        void EvalWidthForEachColumn()
        {
            _widthforeachcolumn.Clear();
            List<int> maxwidths = new List<int>();
            List<bool> canabbr = new List<bool>();
            for (int i = 0; i < _columns.Count; i++)
            {
                maxwidths.Add(_columns[i].GetMaximumWidth());
                canabbr.Add(_columns[i].IsAbbreviatable);
            }
            int roomforabbr = Width;
            int abbrcount=0;
            for(int i=0;i<_columns.Count;i++)
            {
                if (!_columns[i].IsAbbreviatable)
                {
                    roomforabbr -= (maxwidths[i]+1);
                }
                else
                {
                    abbrcount++;
                }
            }
            int roomforeachabbr =0;
            if (abbrcount>0)
                roomforeachabbr = roomforabbr / abbrcount - 1;
            int abbrnum = 0;
            int totalitemnum = 0;
            for (int i = 0; i < _columns.Count; i++)
            {
                if (_columns[i].IsAbbreviatable)
                { 
                    foreach(string aitem in _columns[i].Item)
                    {
                        if (aitem.Length > roomforeachabbr)
                            abbrnum += 1;
                        totalitemnum += 1;
                    }
                }
            }
            double percentabbr = 0;
            if (totalitemnum > 0)
                percentabbr = (double)abbrnum / (double)totalitemnum;
            for (int i = 0; i < _columns.Count; i++)
            {
                if (!_columns[i].IsAbbreviatable||percentabbr>=AbbrThreshold)
                    _widthforeachcolumn.Add(maxwidths[i]+1);
                else 
                {
                    if (maxwidths[i] + 1 < roomforeachabbr + 1)
                        _widthforeachcolumn.Add(maxwidths[i] + 1);
                    else
                        _widthforeachcolumn.Add(roomforeachabbr + 1);
                    _columns[i].AbbreviateToWidth(roomforeachabbr);
                }
            }
            if (IsStretch)
            {
                double total = (double)EvalRealWidth();
                if (total > 0)
                {
                    for (int i = 0; i < _widthforeachcolumn.Count; i++)
                    {
                        _widthforeachcolumn[i] = (int)(((double)_widthforeachcolumn[i] / total)*(double)Width);
                    }
                }
            }
            RealWidth = EvalRealWidth(); 

        }
        public void DisplayRow(int index,int indent=0)
        {
            for (int j = 0; j < Math.Max(Indent,indent); j++)
                System.Console.Write(" ");
            for (int i = 0; i < _columns.Count; i++)
            {
                string item = "";
                if(index<_columns[i].Item.Count)
                    item=_columns[i].Item[index];
                item = item.PadRight(_widthforeachcolumn[i]);
                System.Console.Write(item);
            }
            if (RealWidth<80)
            System.Console.WriteLine("");
        }
        public void DoneAdding()
        {
            EvalWidthForEachColumn();
        }
        public void Display()
        {
            EvalWidthForEachColumn();
            if (_columns.Count == 0)
                return;
            for (int i = 0; i < _columns[0].Item.Count; i++)
            {
                DisplayRow(i);
            }
        }
    }
    //TEST STUB!
#if(TEST_CONSOLEART)
    class TestCA
    {
        static void Main(string[] args)
        {
            //Analyzer a = new Analyzer();
            //a.Start(args);

            //List<Scope> func = Scope.FindChildWithType(TypeRepository.Instance.RootScope, c => c == SType.FUNCTION);
            ConsoleForm cf = new ConsoleForm();
            cf.SetColumnNum(3);
            cf.SetColumnAbbreviatable(1);
            cf.Indent = 0;

            cf.AddRow(new List<string>() { "asdsdsd", "sss", "sadad" });
            cf.AddRow(new List<string>() { "sccs", "s", "asdsdsd" });
            cf.AddRow(new List<string>() { "nnnnnnnnn", "asassdsdsd", "ss" });
            cf.AddRow(new List<string>() { "efsefrsdf", "vvvv", "ssss" });

            cf.IsStretch = false;
            cf.Display();
            System.Console.ReadLine();
        }
    }

#endif
}
