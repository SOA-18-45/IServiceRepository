using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.ServiceModel;
using System.Runtime.Serialization;
using Npgsql;
using MySql.Data.MySqlClient;


namespace Contracts
{
    class Program
    {
        static void Main(string[] args)
        {
            //dodac oczekiwanie na polaczenie od innych serwisow

        }
    }

    [DataContract]
    public class AccountInfo
    {
        [DataMember]
        public string ServiceName { get; set; }

        [DataMember]
        public string ServiceAddress { get; set; }
    }

    [ServiceContract]
    public interface IServiceRepository
    {
        [OperationContract]
        void registerService(string serviceName, string serviceAddress);

        [OperationContract]
        void unregisterService(string serviceName);

        [OperationContract]
        string getServiceAddress(string serviceName);

        [OperationContract]
        void isAlive(string serviceName);
    }

    public class ServiceRepository : IServiceRepository
    {
        public void registerService(string serviceName, string serviceAddress)
        {
            string MyConnectionString = "Server=localhost;Database=test;Uid=root;Pwd=;";
            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection(MyConnectionString);
                connection.Open();
                string cmdText = "INSERT INTO service VALUES(@name,@adres);";
                MySqlCommand cmd = new MySqlCommand(cmdText, connection);
                cmd.Prepare();
                cmd.Parameters.AddWithValue("@name", serviceName);
                cmd.Parameters.AddWithValue("@adres", serviceAddress);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException err)
            {
                Console.WriteLine("Error: " + err.ToString());
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }
        }

        public void unregisterService(string serviceName)
        {
            string MyConnectionString = "Server=localhost;Database=test;Uid=root;Pwd=;";
            MySqlConnection connection = null;
            try
            {
                connection = new MySqlConnection(MyConnectionString);
                connection.Open();
                string cmdText = "DELETE FROM service WHERE nazwa = '" + serviceName + "';";
                MySqlCommand cmd = new MySqlCommand(cmdText, connection);
                cmd.ExecuteNonQuery();
            }
            catch (MySqlException err)
            {
                Console.WriteLine("Error: " + err.ToString());
            }
            finally
            {
                if (connection != null)
                {
                    connection.Close();
                }
            }
        }

        public string getServiceAddress(string serviceName)
        {
            string MyConnectionString = "Server=localhost;Database=test;Uid=root;Pwd=;";
            string adres = null;
            MySqlDataReader reader = null;
            MySqlConnection connection = null;

            try
            {
                connection = new MySqlConnection(MyConnectionString);
                connection.Open();
                string cmdText = "SELECT adres FROM service WHERE nazwa = '"+serviceName+"';";
                MySqlCommand cmd = new MySqlCommand(cmdText, connection);
                reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    adres = reader.GetString(0);
                }
            }

            catch (MySqlException err)
            {
                Console.WriteLine("Error: " + err.ToString());
            }

            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (connection != null)
                {
                    connection.Close();
                }
            }

            return adres;
        }

        public void isAlive(string serviceName)
        {
            string MyConnectionString = "Server=localhost;Database=test;Uid=root;Pwd=;";
            string adres = null;
            MySqlDataReader reader = null;
            MySqlConnection connection = null;

            try
            {
                connection = new MySqlConnection(MyConnectionString);
                connection.Open();
                string cmdText = "SELECT adres FROM service WHERE nazwa = '" + serviceName + "';";
                MySqlCommand cmd = new MySqlCommand(cmdText, connection);
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    adres = reader.GetString(0);
                }
                //w tym miejscu raczej dodac pingowanie adresu i w przypadku braku odpowiedzi wywolac unregister
            }

            catch (MySqlException err)
            {
                Console.WriteLine("Error: " + err.ToString());
            }

            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (connection != null)
                {
                    connection.Close();
                }
            }
        }
    }
    
}