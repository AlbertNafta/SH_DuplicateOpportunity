using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Client;

namespace SalesPlugin
{
    /// <summary>
    /// Plugin to duplicate an Opportunity record and related stakeholders
    /// </summary>
    public class OpportunityDupplication : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Extract the tracing service for use in debugging
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

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
                    tracingService.Trace($"Starting duplication for Opportunity ID: {opportunityRef.Id}");

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
                                    attribute.Key != "statecode" &&
                                    attribute.Key != "statuscode" &&
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

                            // Clone the stakeholder relationships
                            CloneOpportunityStakeholders(service, tracingService, opportunityRef.Id, duplicateOpportunityId);

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
                throw new InvalidPluginExecutionException($"An error occurred in OpportunityDupplication: {ex.Message}", ex);
            }
        }


        private void CloneOpportunityStakeholders(IOrganizationService service, ITracingService tracingService, Guid originalOpportunityId, Guid duplicateOpportunityId)
        {
            try
            {
                tracingService.Trace("Starting to clone stakeholder relationships");

                // Fetch the stakeholder relationships for the original opportunity
                string fetchXml = $@"
                    <fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                       <entity name=""nhan_stakeholder_opportunity"">
                         <attribute name=""nhan_stakeholder_opportunityid"" />
                         <attribute name=""nhan_cre97_stakeholder"" />
                         <attribute name=""nhan_opportunity"" />
                         <filter type='and'>
                          <condition attribute='nhan_opportunity' operator='eq' value='{originalOpportunityId}' />
                        </filter>
                    </entity>
                    </fetch>";

                EntityCollection stakeholderRelationships = service.RetrieveMultiple(new FetchExpression(fetchXml));

                tracingService.Trace($"Found {stakeholderRelationships.Entities.Count} stakeholder relationships to clone");

                // Create new intersect records for each stakeholder with the duplicate opportunity
                foreach (var relationship in stakeholderRelationships.Entities)
                {
                    // Get the stakeholder ID from the relationship
                    EntityReference stakeholderRef = relationship.GetAttributeValue<EntityReference>("nhan_cre97_stakeholder");

                    if (stakeholderRef != null)
                    {
                        tracingService.Trace($"Creating relationship for stakeholder ID: {stakeholderRef.Id}");

                        // Create a new record in the intersect entity
                        Entity newRelationship = new Entity("nhan_stakeholder_opportunity");

                        // Set the references to both the stakeholder and the new opportunity
                        newRelationship["nhan_cre97_stakeholder"] = stakeholderRef;
                        newRelationship["nhan_opportunity"] = new EntityReference("opportunity", duplicateOpportunityId);

                        // If there are any other attributes in the relationship that need to be copied
                        // you can add them here by iterating through relationship.Attributes
                        // Example:
                        // foreach (var attribute in relationship.Attributes)
                        // {
                        //     if (attribute.Key != "nhan_stakeholder_opportunityid" && 
                        //         attribute.Key != "nhan_cre97_stakeholder" && 
                        //         attribute.Key != "nhan_opportunity")
                        //     {
                        //         newRelationship[attribute.Key] = attribute.Value;
                        //     }
                        // }

                        // Create the new relationship record
                        Guid newRelationshipId = service.Create(newRelationship);
                        tracingService.Trace($"Created new stakeholder relationship record with ID: {newRelationshipId}");
                    }
                }

                tracingService.Trace("Finished cloning stakeholder relationships");
            }
            catch (Exception ex)
            {
                tracingService.Trace($"Error cloning stakeholder relationships: {ex.Message}");
                throw new InvalidPluginExecutionException($"Error cloning stakeholder relationships: {ex.Message}", ex);
            }
        }
    }
}