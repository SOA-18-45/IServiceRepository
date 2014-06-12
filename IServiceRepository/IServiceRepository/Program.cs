using IServiceRepository;
using IServiceRepository.Domain;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Contracts
{
    public class Program
    {
        public static ServiceRepository serviceRepo = new ServiceRepository();
        
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            LoadNHibernateCfg();

            /*
            ServiceHost sh = new ServiceHost(serviceRepo, new Uri[] { new Uri("net.tcp://192.168.0.102:50000/IServiceRepository") });
            sh.AddServiceEndpoint(typeof(IServiceRepository), new NetTcpBinding(SecurityMode.None), "net.tcp://192.168.0.102:50000/IServiceRepository");
            */
            ServiceHost sh = new ServiceHost(serviceRepo, new Uri[] { new Uri("net.tcp://localhost:12345/IServiceRepository") });
            sh.AddServiceEndpoint(typeof(IServiceRepository), new NetTcpBinding(SecurityMode.None), "net.tcp://localhost:12345/IServiceRepository");
            sh.Open();
            
            Logger.log.Info("IServiceRepository has started!");

            /*
            var binding = new NetTcpBinding(SecurityMode.None);
            binding.MaxReceivedMessageSize = 1000000;
            binding.MaxBufferSize = 1000000;
            binding.MaxBufferPoolSize = 1000000;
            sh.AddServiceEndpoint(typeof(IBank), binding, "net.tcp://localhost:50000/IBank");
            
            ServiceMetadataBehavior metadata = sh.Description.Behaviors.Find<ServiceMetadataBehavior>();
            if (metadata == null)
            {
                metadata = new ServiceMetadataBehavior();
                sh.Description.Behaviors.Add(metadata);
            }
            metadata.MetadataExporter.PolicyVersion = PolicyVersion.Policy15;
            sh.AddServiceEndpoint(ServiceMetadataBehavior.MexContractName, MetadataExchangeBindings.CreateMexTcpBinding(), "mex");
            */

            Timer timer = new Timer(1000 * 5);
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(RefreshServicesList);
            timer.Start();

            /*
            serviceRepo.registerService("TestRepository2", "net.tcp://localhost:50000/TestRepository1", "net.tcp");
            serviceRepo.registerService("TestRepository2", "http://localhost:50000/TestRepository2", "http");
            serviceRepo.registerService("TestRepository2", "udp://localhost:50000/TestRepository3", "udp");                       
            timer.Elapsed += new ElapsedEventHandler(TestMethod);
            */
            
            Console.ReadLine();
            sh.Close();
            timer.Stop();
            Logger.log.Info("IServiceRepository has ended!");
        }

        /*
        public static void TestMethod(object sender, EventArgs e)
        {
            //serviceRepo.unregisterService("TestRepository2");
            serviceRepo.isAlive("TestRepository2");
        }
        */ 

        public static void LoadNHibernateCfg()
        {
            var cfg = new Configuration();
            cfg.Configure();
            cfg.AddAssembly(typeof(Services).Assembly);
            new SchemaExport(cfg).Execute(true, true, false);
        }

        /* Tutaj zmienić te CreateQuery() na session.delete() oraz session.update(), w zależności od spełnionego warunku, ale dla wielu rekordów, nie tylko dla jednego */
        public static void RefreshServicesList(object sender, EventArgs e)
        {            
            // DELETE & UPDATE
            using (ISession session = NHibernateHelper.OpenSession())
            {
                Services services = session.Query<Services>().SingleOrDefault();
                services.Counter = services.Counter - 1;
                session.Update(services);

                if (services.Counter == 0) Logger.log.Info("There are no services!");
                else Logger.log.Info("There are " + services.Counter + " services!");
                /*var updateQuery = session.CreateQuery("UPDATE Services SET Counter = Counter - 1");
                int updatedRecordNumber = updateQuery.ExecuteUpdate();
                if (updatedRecordNumber == 0) Logger.log.Info("There are no services!");
                else Logger.log.Info("There are " + updatedRecordNumber + " services!");*/       

                if (services.Counter != 0)
                {
                    var services_del = session.Query<Services>().Where(x => x.Counter <= 0);
                    foreach (Services s in services_del)
                        session.Delete(s);

                    //if (servi != 0) Logger.log.Info(deletedRecordNumber + " services have been deleted!");
                    /*var deleteQuery = session.CreateQuery("DELETE FROM Services WHERE Counter <= 0");
                    int deletedRecordNumber = deleteQuery.ExecuteUpdate();
                    if (deletedRecordNumber != 0) Logger.log.Info(deletedRecordNumber + " services have been deleted!");*/
                }
            }
        }
    }

    public class Logger 
    {
        internal static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }

    [DataContract]
    public class AccountInfo
    {
        [DataMember]
        public string ServiceName { get; set; }

        [DataMember]
        public string ServiceAddress { get; set; }

        [DataMember]
        public string BindingType { get; set; }
    }

    [ServiceContract]
    public interface IServiceRepository
    {
        [OperationContract]
        void registerService(string serviceName, string serviceAddress, string bindingType);

        [OperationContract]
        void unregisterService(string serviceName);

        [OperationContract]
        string getServiceAddress(string serviceName, string bindingType);

        [OperationContract]
        void isAlive(string serviceName);
    }

    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ServiceRepository : IServiceRepository
    {
        public void registerService(string serviceName, string serviceAddress, string bindingType)
        {
            // CREATE
            using (ISession session = NHibernateHelper.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {                  
                    Services serviceExists = session.QueryOver<Services>().Where(x => x.ServiceName == serviceName && x.BindingType == bindingType).SingleOrDefault();
                    Services service = new Services { ServiceName = serviceName, ServiceAddress = serviceAddress, BindingType = bindingType, Counter = 3 };
                    if (serviceExists == null) session.Save(service);                   
                    else session.Update(service);                                          
                    transaction.Commit();
                  
                    Logger.log.Info(serviceName + " on " + serviceAddress + " using a " + bindingType + " protocol was registered!");
                }
            }           
        }

        /* Tutaj zmienić to CreateQuery() na session.delete(), ale dla wielu rekordów, nie tylko dla jednego */
        public void unregisterService(string serviceName)
        {
            // DELETE
            using (ISession session = NHibernateHelper.OpenSession())
                using (ITransaction transaction = session.BeginTransaction())
                {
                    var services = session.Query<Services>().Where(x => x.ServiceName == serviceName);
                    foreach (Services s in services)
                        session.Delete(s);

                    transaction.Commit();
                    Logger.log.Info(serviceName + " was unregistered!");
                    
                    //var ts = session.QueryOver<Services>().Where(x => x.ServiceName == serviceName).SingleOrDefault();

                    //foreach (var bla in ts)
                    //foreach (Services nowy in service)

                    //(session.Query<Services>().Where(x => x.ServiceName == serviceName)).ForEach(t => session.Delete(t));
                    
                    /*var deleteQuery = session.CreateQuery("DELETE FROM Services WHERE ServiceName = :serviceName");
                    deleteQuery.SetString("serviceName", serviceName).ExecuteUpdate();                
                    Logger.log.Info(serviceName + " was unregistered!");   */           
                
                    /*
                    using (ITransaction transaction = session.BeginTransaction())
                    {
                        session.Delete(session.QueryOver<Services>().Where(x => x.ServiceName == serviceName));
                        transaction.Commit();
                        Logger.log.Info(serviceName + " was unregistered!");
                    }
                    */
                }         
        }

        public string getServiceAddress(string serviceName, string bindingType)
        {
            // READ
            using (ISession session = NHibernateHelper.OpenSession())
            {
                Services service = session.QueryOver<Services>().Where(x => x.ServiceName == serviceName && x.BindingType == bindingType).SingleOrDefault();
                if (service == null) return serviceName + " using a " + bindingType + " protocol is not registered!";
                else
                {
                    Logger.log.Info(service.ServiceAddress + " was resolved for " + serviceName + " using a " + bindingType + " protocol");
                    return service.ServiceAddress;
                }
            }           
        }

        /* Tutaj zmienić to CreateQuery() na session.update(), ale dla wielu rekordów, nie tylko dla jednego */
        public void isAlive(string serviceName)
        {
            // UPDATE
            using (ISession session = NHibernateHelper.OpenSession())
                using (ITransaction transaction = session.BeginTransaction())
                {
                    Services services = session.Query<Services>().Where(x => x.ServiceName == serviceName).SingleOrDefault();
                    services.Counter = 3;
                    //foreach (Services s in services)
                    session.Update(services);

                    transaction.Commit();
                    Logger.log.Info(serviceName + " is alive!");
                    
                    /*var updateQuery = session.CreateQuery("UPDATE Services SET Counter = 3 WHERE ServiceName = :serviceName");
                    updateQuery.SetString("serviceName", serviceName).ExecuteUpdate();
                    Logger.log.Info(serviceName + " is alive!");*/

                    /*
                    Services service = session.QueryOver<Services>().Where(x => x.ServiceName == serviceName).SingleOrDefault();               
                    if (service != null)
                    {
                        service.Counter = 3;

                        using (ITransaction transaction = session.BeginTransaction())
                        {
                            session.Update(service);
                            transaction.Commit();
                            Logger.log.Info(serviceName + " is alive!");
                        }
                    }
                    */ 
                }
        }
    }
}