using System;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace RestrictDeactivationofPaidInvoices
{
    public class RestrictDeactivation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Obtain the execution context from the service provider.
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

            // Check if the message is Deactivate or Delete
            if (context.MessageName.ToLower() == "setstate" || context.MessageName.ToLower() == "setstatedynamicentity" || context.MessageName.ToLower() == "delete")
            {
                // Obtain the organization service reference
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                // Retrieve the entity ID and logical name
                Guid entityId = context.PrimaryEntityId;
                string entityLogicalName = context.PrimaryEntityName;

                // Retrieve the record to check the mc_invoicestatus and statuscode
                Entity record = service.Retrieve(entityLogicalName, entityId, new ColumnSet("mc_invoicestatus", "statuscode"));

                if (record != null)
                {
                    // Get the boolean value of mc_invoicestatus and the OptionSet value of statuscode
                    bool mcInvoiceStatus = record.GetAttributeValue<bool>("mc_invoicestatus");
                    OptionSetValue statusCode = record.GetAttributeValue<OptionSetValue>("statuscode");

                    // Check if mc_invoicestatus is true and statuscode is not null and has a value of 1
                    if (mcInvoiceStatus && statusCode != null && statusCode.Value == 1)
                    {
                        // Prevent deactivation or deletion
                        throw new InvalidPluginExecutionException("Error Code M0003: Please note that the invoice cannot be deactivated or deleted while its status is marked as 'Paid.' To proceed, first update the fee status to 'Unpaid.'");
                    }
                }
            }
        }
    }
}
