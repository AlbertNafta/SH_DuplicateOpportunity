var Example = window.Example || {};
(function () {
    this.formOnLoad = function (primaryControl) {
        const formContext = primaryControl;
        const entityId = formContext.data.entity.getId();
        console.log("> Hello World", entityId);

        // Display the spinner
        //spinner is not work :<
        showSpinner();

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
                if (response.ok) {
                    return response.json();
                }
            }
        ).then(function (responseBody) {
            var result = responseBody;
            console.log(result);
            // Return Type: mscrm.nhan_opportunityResponse
            // Output Parameters
            var output = result["output"]; // Edm.String
            console.log(output);

            // Hide the spinner after the cloning process is finished
            hideSpinner();

            //  the ID of the cloned opportunity
            if (output) {
                redirectToOpportunity(output);
            }
        }).catch(function (error) {
            // Hide the spinner if there's an error
            hideSpinner();
            console.log(error.message);
        });
    };

    // Function to show a spinner
    function showSpinner() {
        const spinner = document.createElement('div');
        spinner.id = 'spinner';
        spinner.style.position = 'fixed';
        spinner.style.top = '50%';
        spinner.style.left = '50%';
        spinner.style.transform = 'translate(-50%, -50%)';
        spinner.style.zIndex = '9999';
        spinner.innerHTML = '<div class="ms-Spinner ms-Spinner--large ms-u-slideDownIn100 ms-u-slideUpOut100"></div>';
        document.body.appendChild(spinner);
    }

    // Function to hide the spinner
    function hideSpinner() {
        const spinner = document.getElementById('spinner');
        if (spinner) {
            document.body.removeChild(spinner);
        }
    }

    // Function to redirect to the cloned opportunity
    function redirectToOpportunity(opportunityId) {
        const entityFormOptions = {
            entityName: "opportunity",
            entityId: opportunityId
        };
    
        Xrm.Navigation.openForm(entityFormOptions).then(
            function success() {
                console.log("Redirected to the cloned opportunity successfully.");
            },
            function error(error) {
                console.log("Error redirecting to the cloned opportunity:", error.message);
            }
        );
    }
    
}).call(Example);
