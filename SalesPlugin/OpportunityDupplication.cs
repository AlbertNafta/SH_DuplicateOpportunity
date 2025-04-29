using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace SalesPlugin
{
    public class OpportunityDupplication : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            tracingService.Trace("Outside of the plugin");

            try
            {
                // Extract the execution context
                IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));

                // Get the organization service
                IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
                IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

                // Check if the plugin is running in the right context
                if (context.InputParameters.Contains("Target") &&
                    context.InputParameters["Target"] is EntityReference)
                {
                    // Get the opportunity reference from the context
                    EntityReference opportunityRef = (EntityReference)context.InputParameters["Target"];

                    tracingService.Trace($"{opportunityRef}");

                    // Ensure we're working with an opportunity entity
                    if (opportunityRef.LogicalName.Equals("opportunity", StringComparison.OrdinalIgnoreCase))
                    {
                        // Use FetchXML to retrieve the opportunity details
                        string fetchXml = $@"
                            <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                              <entity name='opportunity'>
                                <attribute name='name' />
                                <attribute name='customerid' />
                                <attribute name='estimatedvalue' />
                                <attribute name='estimatedclosedate' />
                                <attribute name='description' />
                                <attribute name='stepname' />
                                <attribute name='purchasetimeframe' />
                                <attribute name='budgetstatus' />
                                <attribute name='purchaseprocess' />
                                <attribute name='opportunityratingcode' />
                                <attribute name='currentsituation' />
                                <attribute name='customerneed' />
                                <attribute name='proposedsolution' />
                                <filter type='and'>
                                  <condition attribute='opportunityid' operator='eq' value='{opportunityRef.Id}' />
                                </filter>
                              </entity>
                            </fetch>";

                        EntityCollection results = service.RetrieveMultiple(new FetchExpression(fetchXml));
                        Console.WriteLine("Result: ", results);
                        tracingService.Trace($"Result: {results}");
                        if (results.Entities.Count > 0)
                        {
                            Entity originalOpportunity = results.Entities[0];
                            tracingService.Trace($"Retrieved original opportunity: {originalOpportunity.GetAttributeValue<string>("name")}");

                            // Create a duplicate opportunity
                            Entity duplicateOpportunity = new Entity("opportunity");

                            // Copy attributes from the original opportunity
                            foreach (var attribute in originalOpportunity.Attributes)
                            {
                                // Skip primary key and read-only system attributes
                                if (attribute.Key != "opportunityid" &&
                                    attribute.Key != "createdon" &&
                                    attribute.Key != "createdby" &&
                                    attribute.Key != "modifiedon" &&
                                    attribute.Key != "modifiedby" &&
                                    attribute.Key != "statecode" && attribute.Key != "statuscode" &&
                                    attribute.Key != "versionnumber")
                                {
                                    duplicateOpportunity[attribute.Key] = attribute.Value;
                                }
                            }

                            // Update the name to indicate it's a duplicate
                            string originalName = originalOpportunity.GetAttributeValue<string>("name");
                            duplicateOpportunity["name"] = $"Cloned - {originalName}";

                            // Create the duplicate opportunity record
                            tracingService.Trace("Creating duplicate opportunity");
                            Guid duplicateOpportunityId = service.Create(duplicateOpportunity);

                            // Set the output parameter with the ID of the new opportunity
                            context.OutputParameters["DuplicatedOpportunityId"] = duplicateOpportunityId;
                            context.OutputParameters["output"] = duplicateOpportunityId;

                            tracingService.Trace($"Successfully created duplicate opportunity with ID: {duplicateOpportunityId}");
                        }
                        else
                        {
                            tracingService.Trace("No opportunity found with the specified ID");
                            throw new InvalidPluginExecutionException("The specified opportunity could not be found.");
                        }
                    }
                    else
                    {
                        tracingService.Trace("Plugin executed on a non-opportunity entity");
                        throw new InvalidPluginExecutionException("This plugin can only be executed on opportunity entities.");
                    }
                }
                else
                {
                    tracingService.Trace("Expected target entity reference not found in the context");
                    throw new InvalidPluginExecutionException("Expected target entity reference not found in the context.");
                }
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Exception: {ex.Message}");
                throw new InvalidPluginExecutionException($"An error occurred in OpportunityDuplicatePlugin: {ex.Message}", ex);
            }
        }
    }
}
