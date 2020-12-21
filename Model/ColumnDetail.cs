using System;

namespace CsharpModelCreator.Model
{
    class ColumnDetail
    {
        public string TABLE_NAME { get; set; }
        public string COLUMN_NAME { get; set; }
        public string ORDINAL_POSITION { get; set; }
        public string DATA_TYPE { get; set; }
        public int CHARACTER_MAXIMUM_LENGTH { get; set; }
        public int NUMERIC_PRECISION { get; set; }
        public int NUMERIC_SCALE { get; set; }
        public int DATETIME_PRECISION { get; set; }
        public bool is_nullable { get; set; }
        public bool is_identity { get; set; }
        public DateTime datetime { get; set; }
    }
}
