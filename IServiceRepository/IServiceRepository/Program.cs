using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.ServiceModel;
using System.Runtime.Serialization;
using System.Net.NetworkInformation;
using MySql.Data.MySqlClient;

namespace Contracts
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = 0;
            string MyConnectionString = "Server=localhost;Database=test;Uid=root;Pwd=;";
            string [] nazwa = new string[9];
            MySqlDataReader reader = null;
            MySqlConnection connection = null;

            try
            {
                connection = new MySqlConnection(MyConnectionString);
                connection.Open();
                string cmdText = "SELECT nazwa FROM service;";
                MySqlCommand cmd = new MySqlCommand(cmdText, connection);
                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    nazwa[i] = reader.GetString(0);
                    i++;
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

            i = 0;
            ServiceRepository serviceRepository = new ServiceRepository();

            while (nazwa[i] != null)
            {
                serviceRepository.isAlive(nazwa[i]);
                i++;
            }
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
            int pingCounter = 0;
            bool pingable = false;
            Ping pinger = new Ping();

            while (pingable == false)
            {
                try
                {
                    PingReply reply = pinger.Send(getServiceAddress(serviceName));
                    pingable = reply.Status == IPStatus.Success;
                }
                
                catch (PingException)
                {
               
                }

                if (pingable == false) pingCounter++;
                
                if (pingCounter == 3)
                {
                    unregisterService(serviceName);
                    break;
                }
            }
        }
    }
}
