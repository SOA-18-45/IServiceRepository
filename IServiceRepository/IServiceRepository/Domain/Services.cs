using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IServiceRepository.Domain
{
    public class Services
    {
        public virtual Guid Id { get; set; }
        public virtual string ServiceName { get; set; }
        public virtual string ServiceAddress { get; set; }
        public virtual string BindingType { get; set; }
        public virtual int Counter { get; set; }
    }
}
