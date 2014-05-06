using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using Npgsql;

// Czemu wybrales akurat PostgreSQL, nie lepiej bylo wziac MySQL? Akurat z Postgre nie mialem jeszcze doczynienia.

//Wstepne szkic, musze troche lepiej zapoznac sie z operowaniem na bazach danych

namespace Contracts
{
    class Program
    {
        static void Main(string[] args)
        {
            //testowanie polaczenia z bazą danych, póki co ma problem z otwarciem połączenia conn.Open()
            NpgsqlConnection conn = new NpgsqlConnection("Server=borg.kis.agh.edu.pl;Port=5432;User Id=rafaplon;Password=12345678;Database=rafaplon;");
            conn.Open();
            string ask = "delete from czekoladki where idczkoladki = 'x99'";
            NpgsqlCommand command = new NpgsqlCommand(ask, conn);

            try
            {
                int rowsaffected = command.ExecuteNonQuery();
                Console.WriteLine("test", rowsaffected);
            }

            finally
            {
                conn.Close();
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
        //schemat dzialania ze strony   cplus.about.com/od/howtodothingsinc/a/How-To-access-PostgreSQL-from-Csharp.htm 
        public void registerService(string serviceName, string serviceAddress)
        {
            NpgsqlConnection conn = new NpgsqlConnection("Server=149.156.207.22;Port=5432;User Id=rafaplon;Password=4pNUqT4d;Database=rafaplon;");
            conn.Open();
            //tabele adresy trzeba dopiero utworzyc w jakiejs bazie danych, tam beda sie zapisywac adresy z nazwami
            string ask = "insert into adresy values('" + serviceName + "','" + serviceAddress + "');";
            NpgsqlCommand command = new NpgsqlCommand(ask, conn);

            try
            {
                int rowsaffected = command.ExecuteNonQuery();
                Console.WriteLine("insert", rowsaffected);
            }

            finally
            {
                conn.Close();
            }
        }

        public void unregisterService(string serviceName)
        {
            NpgsqlConnection conn = new NpgsqlConnection("Server=149.156.207.22;Port=5432;User Id=rafaplon;Password=4pNUqT4d;Database=rafaplon;");
            conn.Open();
            string ask = "delete from adresy where serviceName = '" + serviceName + "';";
            NpgsqlCommand command = new NpgsqlCommand(ask, conn);

            try
            {
                int rowsaffected = command.ExecuteNonQuery();
                Console.WriteLine("delete", rowsaffected);
            }

            finally
            {
                conn.Close();
            }
        }

        public string getServiceAddress(string serviceName)
        {
            NpgsqlConnection conn = new NpgsqlConnection("Server=149.156.207.22;Port=5432;User Id=rafaplon;Password=4pNUqT4d;Database=rafaplon;");
            conn.Open();
            string ask = "select address from adresy where serviceName = '" + serviceName + "';";
            NpgsqlCommand command = new NpgsqlCommand(ask, conn);
            string address;
            address = command.ToString(); //strzal, sprawdzic czy takie cos zadziala

            try
            {
                int rowsaffected = command.ExecuteNonQuery();
                Console.WriteLine("get", rowsaffected);
            }

            finally
            {
                conn.Close();
            }
            return address;
        }

        public void isAlive(string serviceName)
        {
            //trzeba przeleciec po wszystkich adresach bazy i wyslac pinga??? do kazdego, jesli nie odpowie to wywolac metode unregisterService dla niego????
        }
    }
    
}
