using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HL.Domain.Models
{
    public enum DataType
    { 
        String,
        Integer,
        Datetime,
        Boolean
    }
    public sealed class DbParameter
    {
        public string Name { get; set; }
        public DataType Type { get; set; }
        public object Value { get; set; }
    }
}
