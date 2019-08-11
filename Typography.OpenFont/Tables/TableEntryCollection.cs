//Apache2, 2017-present, WinterDev
//Apache2, 2014-2016, Samuel Carlsson, WinterDev

using System.Collections.Generic;
namespace Typography.OpenFont.Tables
{
    class TableEntryCollection
    {
        Dictionary<string, TableEntry> _tables = new Dictionary<string, TableEntry>();
        public TableEntryCollection() { }
        public void AddEntry(TableEntry en)
        {
            _tables.Add(en.Name, en);
        }

        public bool TryGetTable(string tableName, out TableEntry entry)
        {
            return _tables.TryGetValue(tableName, out entry);
        }

        public void ReplaceTable(TableEntry table)
        {
            _tables[table.Name] = table;
        }
    }
}
