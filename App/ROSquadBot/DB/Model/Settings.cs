using ROTGBot.Db.Attributes;

namespace ROTGBot.Db.Model
{

    [TableName("settings")]
    public class Settings
    {
        [ColumnName("id")]
        public int Id { get; set; }

        [ColumnName("param_name")]
        public string ParamName { get; set; } = "";

        [ColumnName("param_value")]
        public string ParamValue { get; set; } = "";
    }

    [TableName("task_settings")]
    public class TaskSettings
    {
        [ColumnName("id")]
        public int Id { get; set; }
        [ColumnName("code")]
        public int Code { get; set; }

        [ColumnName("name")]
        public string Name { get; set; } = "";

        [ColumnName("value")]
        public string Value { get; set; } = "";
    }
}