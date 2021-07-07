using CsharpModelCreator.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;

namespace CsharpModelCreator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<ColumnDetail> columnDetails = new List<ColumnDetail>();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            bool mssql = mssqlcb.IsChecked ?? false;
            bool mysql = mysqlcb.IsChecked ?? false;
            if (mssql)
            {
                string connectionString = @"Data Source=" + ServerTextBox.Text.Trim() + ";Initial Catalog=" + databaseTextbox.Text.Trim()
                + ";User ID=" + usernameTextbox.Text.Trim() + ";Password=" + pwdBox.Password.Trim();
                SqlConnection cnn;
                SqlCommand command;
                SqlDataReader dataReader;
                string query = "";
                string all = "";
                if (alltbl.IsChecked == true)
                {
                    List<string> tblnames = new List<string>();
                    all = "SELECT distinct isc.TABLE_NAME " +
                            " FROM INFORMATION_SCHEMA.COLUMNS isc inner join sys.all_columns c on isc.COLUMN_NAME = c.name " +
                            " inner join sys.all_objects o on c.object_id = o.object_id " +
                            " WHERE(o.type = 'U' or o.type = 'V')";

                    //logbox.AppendText(query + "\n\n");
                    cnn = new SqlConnection(connectionString);
                    cnn.Open();

                    command = new SqlCommand(all, cnn);
                    dataReader = command.ExecuteReader();

                    while (dataReader.Read())
                    {
                        try
                        {
                            if (Convert.ToString(dataReader.GetValue(0)) != "NULL")
                            {
                                string TABLE_NAME = Convert.ToString(dataReader.GetValue(0));
                                tblnames.Add(TABLE_NAME);
                            }
                        }
                        catch (Exception ex)
                        { }
                    }

                    foreach (string tbl in tblnames)
                    {
                        query = "SELECT isc.TABLE_NAME, isc.COLUMN_NAME, isc.ORDINAL_POSITION, isc.DATA_TYPE, "
                                                    + "isc.CHARACTER_MAXIMUM_LENGTH, isc.NUMERIC_PRECISION, isc.NUMERIC_SCALE, isc.DATETIME_PRECISION, c.is_nullable, c.is_identity "
                                                    + "FROM INFORMATION_SCHEMA.COLUMNS isc inner join sys.all_columns c on isc.COLUMN_NAME = c.name "
                                                    + "inner join sys.all_objects o on c.object_id = o.object_id "
                                                    + "WHERE o.name = '" + tbl.Trim() + "' and isc.TABLE_NAME = '" + tbl.Trim() + "' and (o.type = 'U' or o.type='V') ";

                        QueryColumns(tbl);
                    }
                }
                else
                {
                    QueryColumns(tableTextbox.Text);

                }
            }
            if (mysql)
            {
                List<string> tablenames = new List<string>();
                string connectionString = "SERVER=" + ServerTextBox.Text + ";" + "DATABASE=" + databaseTextbox.Text + ";" + "UID=" + usernameTextbox.Text + ";" + "PASSWORD=" + pwdBox.Password + ";";
                MySqlConnection connection = new MySqlConnection(connectionString);
                try
                {
                    connection.Open();
                    string query = "";
                    if (tableTextbox.Text != "")
                    {
                        query = "SELECT TABLE_NAME AS tablename FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE() and TABLE_NAME = '"+tableTextbox.Text+"'; ";
                    }
                    else
                    {
                        query = "SELECT TABLE_NAME AS tablename FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = DATABASE()";
                    }
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader dataReader = cmd.ExecuteReader();
                    while (dataReader.Read())
                    {
                        tablenames.Add(dataReader["tablename"].ToString());
                    }
                    foreach (string tbl in tablenames)
                    {
                        QueryColumnsMySQL(tbl);
                    }
                    dataReader.Close();
                }
                catch(Exception ex)
                {
                    logbox.AppendText(ex.ToString() + "\n\n");
                    connection.Close();
                }
                finally
                {
                    connection.Close();
                }

            }
                //logbox.AppendText(query + "\n\n");
                
        }

        private void QueryColumnsMySQL(string tbl)
        {
            columnDetails = new List<ColumnDetail>();
            string query = "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE table_name = '" + tbl + "'";
            string connectionString = "server=" + ServerTextBox.Text + ";" + "database=" + databaseTextbox.Text + ";" + "user=" + usernameTextbox.Text + ";" + "password=" + pwdBox.Password + ";";
            logbox.AppendText(connectionString+"\n\n");
            MySqlConnection connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader = cmd.ExecuteReader();
                while (dataReader.Read())
                {
                    ColumnDetail cd = new ColumnDetail();
                    cd.COLUMN_NAME = dataReader["COLUMN_NAME"].ToString();
                    cd.DATA_TYPE = dataReader["DATA_TYPE"].ToString();
                    columnDetails.Add(cd);
                }
                dataReader.Close();
            }
            catch(Exception ex)
            {
                logbox.AppendText(ex.ToString() + "\n\n");
                connection.Close();
            }
            finally
            {
                connection.Close();
            }

            generateClassMySQL(tbl);

        }

        private void generateClassMySQL(string tbl)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("using System;\n");
                sb.Append("namespace " + namespaceBox.Text.Trim() + "\n");
                sb.Append("{" + "\n");
                sb.Append("\tpublic class " + tbl.Trim() + "\n");
                sb.Append("\t{" + "\n");

                foreach (ColumnDetail c in columnDetails)
                {
                    sb.Append("\t\tpublic ");
                    //STRING
                    if (string.Equals(c.DATA_TYPE, "CHAR", StringComparison.OrdinalIgnoreCase) || string.Equals(c.DATA_TYPE, "VARCHAR", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(c.DATA_TYPE, "VARBINARY", StringComparison.OrdinalIgnoreCase) || string.Equals(c.DATA_TYPE, "TINYBLOB", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(c.DATA_TYPE, "TINYTEXT", StringComparison.OrdinalIgnoreCase) || string.Equals(c.DATA_TYPE, "TEXT", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(c.DATA_TYPE, "BLOB", StringComparison.OrdinalIgnoreCase) || string.Equals(c.DATA_TYPE, "MEDIUMTEXT", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(c.DATA_TYPE, "MEDIUMBLOB", StringComparison.OrdinalIgnoreCase) || string.Equals(c.DATA_TYPE, "LONGTEXT", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(c.DATA_TYPE, "LONGBLOB", StringComparison.OrdinalIgnoreCase) || string.Equals(c.DATA_TYPE, "ENUM", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(c.DATA_TYPE, "SET", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append("string");
                    }

                    //NUMERIC
                    if (string.Equals(c.DATA_TYPE, "BIT", StringComparison.OrdinalIgnoreCase) || string.Equals(c.DATA_TYPE, "TINYINT", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(c.DATA_TYPE, "SMALLINT", StringComparison.OrdinalIgnoreCase) || string.Equals(c.DATA_TYPE, "MEDIUMINT", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(c.DATA_TYPE, "INT", StringComparison.OrdinalIgnoreCase) || string.Equals(c.DATA_TYPE, "INTEGER", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append("int");
                    }
                    if(string.Equals(c.DATA_TYPE, "DECIMAL", StringComparison.OrdinalIgnoreCase) || string.Equals(c.DATA_TYPE, "DEC", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append("decimal");
                    }
                    if(string.Equals(c.DATA_TYPE, "BIGINT", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append("long");
                    }
                    if(string.Equals(c.DATA_TYPE, "FLOAT", StringComparison.OrdinalIgnoreCase)){
                        sb.Append("float");
                    }
                    if(string.Equals(c.DATA_TYPE, "DOUBLE", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append("double");
                    }
                    if(string.Equals(c.DATA_TYPE, "BOOL", StringComparison.OrdinalIgnoreCase) || string.Equals(c.DATA_TYPE, "BOOLEAN", StringComparison.OrdinalIgnoreCase)){
                        sb.Append("bool");
                    }
                    if(string.Equals(c.DATA_TYPE, "DATE", StringComparison.OrdinalIgnoreCase) || string.Equals(c.DATA_TYPE, "DATETIME", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(c.DATA_TYPE, "TIMESTAMP", StringComparison.OrdinalIgnoreCase) || string.Equals(c.DATA_TYPE, "TIME", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(c.DATA_TYPE, "YEAR", StringComparison.OrdinalIgnoreCase))
                    {
                        sb.Append("DateTime");
                    }
                    sb.Append(" " + c.COLUMN_NAME + " { get; set; } \n");
                }
                sb.Append("\t}\n");
                sb.Append("}");

                string filename = "\\" + tbl.Trim() + ".cs";
                string path = pathbox.Text.Trim();

                /*SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.InitialDirectory = @"C:\Logs";
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.DefaultExt = "cs";
                if(saveFileDialog.ShowDialog().Value)
                {
                    path = System.IO.Path.GetDirectoryName(saveFileDialog.FileName);
                }*/
                //write file
                if (File.Exists(path + filename))
                {
                    File.Delete(path + filename);
                }
                using (FileStream fs = File.Create(path + filename))
                {
                    byte[] contents = new UTF8Encoding(true).GetBytes(sb.ToString());
                    fs.Write(contents, 0, contents.Length);
                }
                logbox.AppendText(path + filename + " successfully generated!\n");
            }
            catch(Exception ex)
            {
                logbox.AppendText(ex.ToString() + "\n\n");
            }
        }

        private void QueryColumns(string tbl)
        {
            columnDetails = new List<ColumnDetail>();
            string query = "SELECT isc.TABLE_NAME, isc.COLUMN_NAME, isc.ORDINAL_POSITION, isc.DATA_TYPE, "
                                                + "isc.CHARACTER_MAXIMUM_LENGTH, isc.NUMERIC_PRECISION, isc.NUMERIC_SCALE, isc.DATETIME_PRECISION, c.is_nullable, c.is_identity "
                                                + "FROM INFORMATION_SCHEMA.COLUMNS isc inner join sys.all_columns c on isc.COLUMN_NAME = c.name "
                                                + "inner join sys.all_objects o on c.object_id = o.object_id "
                                                + "WHERE o.name = '" + tbl.Trim() + "' and isc.TABLE_NAME = '" + tbl.Trim() + "' and (o.type = 'U' or o.type='V') ";
            SqlConnection cnn;
            SqlCommand command;
            SqlDataReader dataReader;
            string connectionString = @"Data Source=" + ServerTextBox.Text.Trim() + ";Initial Catalog=" + databaseTextbox.Text.Trim()
                + ";User ID=" + usernameTextbox.Text.Trim() + ";Password=" + pwdBox.Password.Trim();

            cnn = new SqlConnection(connectionString);
            cnn.Open();

            command = new SqlCommand(query, cnn);
            dataReader = command.ExecuteReader();

            while (dataReader.Read())
            {
                ColumnDetail cd = new ColumnDetail();
                try
                {
                    if (Convert.ToString(dataReader.GetValue(0)) != "NULL")
                    {
                        cd.TABLE_NAME = Convert.ToString(dataReader.GetValue(0));
                    }
                }
                catch (Exception ex)
                { }

                try
                {
                    if (Convert.ToString(dataReader.GetValue(1)) != "NULL")
                    {
                        cd.COLUMN_NAME = Convert.ToString(dataReader.GetValue(1));
                    }
                }
                catch (Exception ex) { }

                try
                {
                    if (Convert.ToString(dataReader.GetValue(2)) != "NULL")
                    {
                        cd.ORDINAL_POSITION = Convert.ToString(dataReader.GetValue(2));
                    }
                }
                catch (Exception ex)
                { }
                try
                {
                    if (Convert.ToString(dataReader.GetValue(3)) != "NULL")
                    {
                        cd.DATA_TYPE = Convert.ToString(dataReader.GetValue(3));
                    }

                }
                catch (Exception ex)
                { }
                try
                {
                    if (Convert.ToString(dataReader.GetValue(4)) != "NULL")
                    {
                        cd.CHARACTER_MAXIMUM_LENGTH = Convert.ToInt32(dataReader.GetValue(4));
                    }
                }
                catch (Exception ex)
                { }
                try
                {
                    if (Convert.ToString(dataReader.GetValue(5)) != "NULL")
                    {
                        cd.CHARACTER_MAXIMUM_LENGTH = Convert.ToInt32(dataReader.GetValue(5));
                    }
                }
                catch (Exception ex) { }
                try
                {
                    if (Convert.ToString(dataReader.GetValue(6)) != "NULL")
                    {
                        cd.NUMERIC_SCALE = Convert.ToInt32(dataReader.GetValue(6));
                    }
                }
                catch (Exception ex)
                { }
                try
                {
                    if (Convert.ToString(dataReader.GetValue(7)) != "NULL")
                    {
                        cd.DATETIME_PRECISION = Convert.ToInt32(dataReader.GetValue(7));
                    }
                }
                catch (Exception ex)
                { }
                cd.is_nullable = Convert.ToBoolean(dataReader.GetValue(8));

                cd.is_identity = Convert.ToBoolean(dataReader.GetValue(9));

                columnDetails.Add(cd);
            }
            dataReader.Close();
            command.Dispose();
            cnn.Close();


            generateClass(tbl);
        }

        private void generateClass(string tbl)
        {
            //to generate class file
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("using System;\n");
                sb.Append("namespace "+namespaceBox.Text.Trim()+"\n");
                sb.Append("{" + "\n");
                sb.Append("\tpublic class "+ tbl.Trim() + "\n");
                sb.Append("\t{" + "\n");
                foreach(ColumnDetail c in columnDetails)
                {   //int, varchar,datetime, decimal, bit
                    sb.Append("\t\tpublic ");
                    if (c.DATA_TYPE == "int" || c.DATA_TYPE=="decimal")
                    {
                        sb.Append(c.DATA_TYPE);
                    }
                    else if (c.DATA_TYPE == "bigint")
                    {
                        sb.Append("long");
                    }
                    else if(c.DATA_TYPE == "varchar" || c.DATA_TYPE=="nvarchar")
                    {
                        sb.Append("string");
                    }
                    else if (c.DATA_TYPE == "datetime" || c.DATA_TYPE== "datetime2" || c.DATA_TYPE == "date")
                    {
                        sb.Append("DateTime");
                    }
                    else if(c.DATA_TYPE== "datetimeoffset")
                    {
                        sb.Append("DateTimeOffset");
                    }
                    else if(c.DATA_TYPE == "bit" || c.DATA_TYPE=="smallint" || c.DATA_TYPE == "tinyint")
                    {
                        sb.Append("bool");
                    }
                    else if (c.DATA_TYPE=="real")
                    {
                        sb.Append("decimal");
                    }
                    sb.Append(" "+c.COLUMN_NAME +" { get; set; } \n");
                }
                sb.Append("\t}\n");
                sb.Append("}");

                string filename = "\\"+tbl.Trim()+".cs";
                string path = pathbox.Text.Trim();

                /*SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.InitialDirectory = @"C:\Logs";
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.DefaultExt = "cs";
                if(saveFileDialog.ShowDialog().Value)
                {
                    path = System.IO.Path.GetDirectoryName(saveFileDialog.FileName);
                }*/
                //write file
                if (File.Exists(path+filename))
                {
                    File.Delete(path+filename);
                }
                using(FileStream fs = File.Create(path+filename))
                {
                    byte[] contents = new UTF8Encoding(true).GetBytes(sb.ToString());
                    fs.Write(contents, 0, contents.Length);
                }
                logbox.AppendText(path + filename+" successfully generated!\n");
            }
            catch(Exception ex)
            {
                logbox.AppendText(ex.ToString()+"\n\n");
            }
        }
    }
}
