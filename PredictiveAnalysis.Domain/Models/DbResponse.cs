using System;
using System.Collections.Generic;
using System.Linq;

namespace HL.Domain.Model
{
    public class DbResponse
    {
        //public Dictionary<string, object> Records { get; set; }
        public List<Record> Records { get; set; }

        public DbResponse()
        {
            Records = new List<Record>();
        }
    }

    public class Record
    {
        private List<Field> fields { get; set; }

        public Record()
        {
            fields = new List<Field>();
        }

        public int FieldCount
        {
            get
            {
                return fields.Count;
            }
        }

        public object this[string key]
        {
            get
            {
                return fields.Where(r => r.Key.ToLower() == key.ToLower()).SingleOrDefault().Value;
            }
        }

        public void Add(string key, object value)
        {
            fields.Add(new Field() { Key = key, Value = value });
        }

        public void Remove(string key)
        {
            Field field = fields.Where(f => f.Key.ToLower() == key.ToLower()).SingleOrDefault();
            if (field == null)
                throw new ArgumentException();

            fields.Remove(field);
        }
    }

    public class Field
    {
        public string Key { get; set; }
        public object Value { get; set; }
    }
}