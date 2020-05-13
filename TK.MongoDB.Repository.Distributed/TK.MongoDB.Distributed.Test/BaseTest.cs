using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TK.MongoDB.Distributed.Data;
using TK.MongoDB.Distributed.Test.Models;

namespace TK.MongoDB.Distributed.Test
{
    public class BaseTest
    {
        private IContainer autofacContainer;
        protected IContainer AutofacContainer
        {
            get
            {
                if (autofacContainer == null)
                {
                    var builder = new ContainerBuilder();

                    builder.RegisterType<MasterRepository>()
                        .As<IMasterRepository>()
                        .InstancePerLifetimeScope();

                    builder.RegisterGeneric(typeof(Repository<>))
                        .As(typeof(IRepository<>))
                        .InstancePerLifetimeScope();

                    var container = builder.Build();
                    autofacContainer = container;
                }

                return autofacContainer;
            }
        }

        protected IMasterRepository MasterRepository
        {
            get
            {
                return AutofacContainer.Resolve<IMasterRepository>();
            }
        }

        protected IRepository<Message> MessageRepository
        {
            get
            {
                return AutofacContainer.Resolve<IRepository<Message>>();
            }
        }

        protected IRepository<CompanyMessage> CompanyMessageRepository
        {
            get
            {
                return AutofacContainer.Resolve<IRepository<CompanyMessage>>();
            }
        }
    }
}
