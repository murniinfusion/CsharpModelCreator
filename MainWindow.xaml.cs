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
            columnDetails = new List<ColumnDetail>();
            string connectionString = @"Data Source="+ServerTextBox.Text.Trim()+";Initial Catalog="+databaseTextbox.Text.Trim()
                +";User ID="+usernameTextbox.Text.Trim()+";Password="+pwdBox.Password.Trim();
            SqlConnection cnn;
            SqlCommand command;
            SqlDataReader dataReader;
            
                string query = "SELECT isc.TABLE_NAME, isc.COLUMN_NAME, isc.ORDINAL_POSITION, isc.DATA_TYPE, "
                                + "isc.CHARACTER_MAXIMUM_LENGTH, isc.NUMERIC_PRECISION, isc.NUMERIC_SCALE, isc.DATETIME_PRECISION, c.is_nullable, c.is_identity "
                                + "FROM INFORMATION_SCHEMA.COLUMNS isc inner join sys.all_columns c on isc.COLUMN_NAME = c.name "
                                + "inner join sys.all_objects o on c.object_id = o.object_id "
                                + "WHERE o.name = '"+tableTextbox.Text.Trim()+"' and isc.TABLE_NAME = '"+tableTextbox.Text.Trim()+ "' and (o.type = 'U' or o.type='V') ";
                //logbox.AppendText(query + "\n\n");
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
                catch(Exception ex)
                {}
                try
                {
                    if (Convert.ToString(dataReader.GetValue(1)) != "NULL")
                    {
                        cd.COLUMN_NAME = Convert.ToString(dataReader.GetValue(1));
                    }
                }catch(Exception ex) { }
                try
                {
                    if (Convert.ToString(dataReader.GetValue(2)) != "NULL")
                    {
                        cd.ORDINAL_POSITION = Convert.ToString(dataReader.GetValue(2));
                    }
                }
                catch (Exception ex)
                {}
                try
                {
                    if (Convert.ToString(dataReader.GetValue(3)) != "NULL")
                    {
                        cd.DATA_TYPE = Convert.ToString(dataReader.GetValue(3));
                    }
                    
                }
                catch (Exception ex)
                {}
                try
                {
                    if (Convert.ToString(dataReader.GetValue(4)) != "NULL")
                    {
                        cd.CHARACTER_MAXIMUM_LENGTH = Convert.ToInt32(dataReader.GetValue(4));
                    }
                }
                catch (Exception ex)
                {}
                try
                {
                    if (Convert.ToString(dataReader.GetValue(5)) != "NULL")
                    {
                        cd.CHARACTER_MAXIMUM_LENGTH = Convert.ToInt32(dataReader.GetValue(5));
                    }
                }
                catch(Exception ex) { }
                try
                {
                    if (Convert.ToString(dataReader.GetValue(6)) != "NULL")
                    {
                        cd.NUMERIC_SCALE = Convert.ToInt32(dataReader.GetValue(6));
                    }
                }
                catch (Exception ex)
                {}
                try
                {
                    if (Convert.ToString(dataReader.GetValue(7)) != "NULL")
                    {
                        cd.DATETIME_PRECISION = Convert.ToInt32(dataReader.GetValue(7));
                    }
                }
                catch (Exception ex)
                {}
                cd.is_nullable = Convert.ToBoolean(dataReader.GetValue(8));
                    
                    cd.is_identity = Convert.ToBoolean(dataReader.GetValue(9));
                    
                    columnDetails.Add(cd);
                }
                dataReader.Close();
                command.Dispose();
                cnn.Close();
            

            generateClass();
        }

        private void generateClass()
        {
            //to generate class file
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("using System;\n");
                sb.Append("namespace "+namespaceBox.Text.Trim()+"\n");
                sb.Append("{" + "\n");
                sb.Append("\tpublic class "+tableTextbox.Text.Trim() + "Model\n");
                sb.Append("\t{" + "\n");
                foreach(ColumnDetail c in columnDetails)
                {   //int, varchar,datetime, decimal, bit
                    sb.Append("\t\tpublic ");
                    if (c.DATA_TYPE == "int" || c.DATA_TYPE=="decimal")
                    {
                        sb.Append(c.DATA_TYPE);
                    }
                    else if(c.DATA_TYPE == "varchar" || c.DATA_TYPE=="nvarchar")
                    {
                        sb.Append("string");
                    }
                    else if (c.DATA_TYPE == "datetime")
                    {
                        sb.Append("DateTime");
                    }
                    else if(c.DATA_TYPE == "bool")
                    {
                        sb.Append("bit");
                    }
                    sb.Append(" "+c.COLUMN_NAME +" {get; set;} \n");
                }
                sb.Append("\t}\n");
                sb.Append("}");

                string filename = "\\"+tableTextbox.Text.Trim()+"Model.cs";
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
