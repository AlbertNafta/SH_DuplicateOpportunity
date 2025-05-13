var Example = window.Example || {};
(function () {
    this.formOnLoad = function (primaryControl) {
        const formContext = primaryControl;
        const entityId = formContext.data.entity.getId();
        // const entityId = formContext.data.entity.getId().replace("{", "").replace("}", "");
        console.log("> Hello World", entityId)
        var execute_nhan_opportunity_Request = {
            // Parameters
            entity: { entityType: "opportunity", id: entityId }, // entity
            getMetadata: function () {
                return {
                    boundParameter: "entity",
                    parameterTypes: {
                        entity: { typeName: "mscrm.opportunity", structuralProperty: 5 }
                    },
                    operationType: 0, operationName: "nhan_opportunity"
                };
            }
        };
        Xrm.WebApi.execute(execute_nhan_opportunity_Request).then(
            function success(response) {
                if (response.ok) { return response.json(); }
            }
        ).then(function (responseBody) {
            var result = responseBody;
            console.log(result);
            // Return Type: mscrm.nhan_opportunityResponse
            // Output Parameters
            var output = result["output"]; // Edm.String
            console.log(output)
        }).catch(function (error) {
            debugger
            console.log(error.message);
        });
    }
}).call(Example);
