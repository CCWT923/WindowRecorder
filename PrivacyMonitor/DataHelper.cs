using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace PrivacyMonitor
{
    class DataHelper
    {
        SQLiteConnection Connection = null;
        SQLiteCommand Command = null;
        string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Recorder";
        string DataFile;
        /// <summary>
        /// 构造函数
        /// </summary>
        public DataHelper()
        {
            //数据库文件路径
#if DEBUG
            {
                DataFile = AppDataPath + @"\Data_Debug.db";
            }
#else
            {
                DataFile = AppDataPath + @"\Data.db";                   
            }
#endif
        }

        /// <summary>
        /// 初始化数据库
        /// </summary>
        public void InitDatabase()
        {
            CreateDatabase(DataFile);
            CreateTables();
        }
        /// <summary>
        /// 打开数据库连接
        /// </summary>
        public void Open()
        {
            Connection = new SQLiteConnection("data source = " + DataFile);
            Connection.Open();
            Command = new SQLiteCommand(Connection);
        }
        /// <summary>
        /// 创建数据库
        /// </summary>
        /// <param name="dbName">数据库名称</param>
        private void CreateDatabase(string dbName)
        {
            if(!Directory.Exists(AppDataPath))
            {
                Directory.CreateDirectory(AppDataPath);
            }
            //备份数据库
            if(File.Exists(dbName))
            {
                if(!Directory.Exists(AppDataPath + @"\Recorder\Backup"))
                {
                    Directory.CreateDirectory("Backup");
                }
                File.Copy(dbName, "Backup/" + Path.GetFileNameWithoutExtension(dbName) + "_bak_" + 
                    DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetExtension(dbName));
            }
            //如果文件正在被另一程序访问
            try
            {
                SQLiteConnection.CreateFile(dbName);
            }
            catch(Exception)
            {
                
            }

            Open();
        }

        /// <summary>
        /// 创建表
        /// </summary>
        private void CreateTables()
        {
            if(Connection.State == System.Data.ConnectionState.Open)
            {
                Command = new SQLiteCommand();
                Command.Connection = Connection;
                //创建Record表
                Command.CommandText = "CREATE TABLE IF NOT EXISTS Record(LogDate TEXT, WindowCaption TEXT, " + 
                    " ClassName TEXT, Process TEXT, ProcessPath TEXT, StartTime TEXT, EndTime TEXT, Duration INTEGER, Remarks TEXT);";
                Command.ExecuteNonQuery();

                //创建SystemInfo表
                Command.CommandText = "CREATE TABLE IF NOT EXISTS SystemInfo(Code TEXT PRIMARY KEY, Value TEXT, Remark TEXT);";
                Command.ExecuteNonQuery();

                //创建SpecialItems表
                Command.CommandText = "CREATE TABLE IF NOT EXISTS SpecialItems(Process TEXT , title TEXT );";
                Command.ExecuteNonQuery();

                //创建ErrorLog表
                Command.CommandText = "CREATE TABLE IF NOT EXISTS Log(Date, Event, Description);";
                Command.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// 返回Command对象
        /// </summary>
        /// <returns></returns>
        public void GetCommand(ref SQLiteCommand command)
        {
            command = Command;
        }
        /// <summary>
        /// 获取数据库名称
        /// </summary>
        public string GetDataFileName
        {
            get { return DataFile; }
        }
        /// <summary>
        /// 向指定表中插入数据
        /// </summary>
        /// <param name="TableName"></param>
        /// <param name="Data"></param>

        public void InsertData(string TableName, string[] data)
        {
            //参数化查询
            if(TableName == "Log") //Date, Event, Description
            {
                Command.CommandText = "INSERT INTO Log VALUES(@date,@event,@description);";
                Command.Parameters.Add("date", System.Data.DbType.DateTime);
                Command.Parameters.Add("event", System.Data.DbType.String);
                Command.Parameters.Add("description", System.Data.DbType.String);
                Command.Parameters["date"].Value = data[0].ToString();
                Command.Parameters["event"].Value = data[1].ToString();
                Command.Parameters["description"].Value = data[2].ToString();
            }
            //ID , LogDate, WindowCaption ， ClassName， Process， ProcessPath ，StartTime ，EndTime ，Duration ，Remarks 
            else if (TableName == "Record")
            {
                Command.CommandText = "INSERT INTO Record VALUES(@logdate,@windowcaption," +
                    "@classname,@process,@processpath,@starttime,@endtime,@duration,@remarks);";

                Command.Parameters.Add("logdate", System.Data.DbType.DateTime);
                Command.Parameters.Add("windowcaption", System.Data.DbType.String);
                Command.Parameters.Add("classname", System.Data.DbType.String);
                Command.Parameters.Add("process", System.Data.DbType.String);
                Command.Parameters.Add("processpath", System.Data.DbType.String);
                Command.Parameters.Add("starttime", System.Data.DbType.DateTime);
                Command.Parameters.Add("endtime", System.Data.DbType.DateTime);
                Command.Parameters.Add("duration", System.Data.DbType.Int32);
                Command.Parameters.Add("remarks", System.Data.DbType.String);

                Command.Parameters["logdate"].Value = data[0];
                Command.Parameters["windowcaption"].Value = data[1];
                Command.Parameters["classname"].Value = data[2];
                Command.Parameters["process"].Value = data[3];
                //DebugOut(Command.Parameters["process"].Value);
                Command.Parameters["processpath"].Value = data[4];
                //DebugOut(Command.Parameters["processpath"].Value);
                Command.Parameters["starttime"].Value = data[5];
                //DebugOut(Command.Parameters["starttime"].Value);
                Command.Parameters["endtime"].Value = data[6];
                //DebugOut(Command.Parameters["endtime"].Value);
                Command.Parameters["duration"].Value = Int32.Parse(data[7]);
                //DebugOut(Command.Parameters["duration"].Value);
                Command.Parameters["remarks"].Value = data[8];
                //DebugOut(Command.Parameters["remarks"].Value);
            }
            else
            {
                return;
            }
            
            Command.ExecuteNonQuery();
        }

#if DEBUG
        private void DebugOut(object msg)
        {
            System.Diagnostics.Debug.Write(msg.ToString() + "\n");
        }
#endif
        /// <summary>
        /// 数据库是否已经初始化
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                if(File.Exists(DataFile))
                {
                    SQLiteConnection sQLiteConnection = null;
                    try
                    {
                        sQLiteConnection = new SQLiteConnection("Data Source = " + DataFile);
                        sQLiteConnection.Open();
                        SQLiteCommand cmd = new SQLiteCommand(sQLiteConnection);
                        cmd.CommandText = "select count(1) from Record;";
                        cmd.ExecuteNonQuery();
                        sQLiteConnection.Close();
                        cmd.Dispose();
                        //sQLiteConnection.Dispose();
                        return true;
                    }
                    catch(Exception)
                    {
                        if(sQLiteConnection.State == System.Data.ConnectionState.Open)
                        {
                            sQLiteConnection.Close();
                        }
                        return false;
                    }
                    finally
                    {
                        sQLiteConnection.Dispose();
                    }
                }
                return false;
            }
        }
    }
}
