
using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;

namespace SeleniumTests
{
    /// <summary>
    /// This class is responsible for creating a bug work item in Azure DevOps via the REST API.
    /// </summary>
    public class AzureDevOpsBugCreator
    {
        private string azureDevOpsUrl;
        private string project;
        private string personalAccessToken;

        /// <summary>
        /// Constructor reads configuration from appsettings.json and initializes Azure DevOps connection details.
        /// </summary>
        public AzureDevOpsBugCreator()
        {
            // Read the configuration JSON file
            var config = File.ReadAllText("appsettings.json");
            // Deserialize JSON to dynamic object to access settings
            dynamic settings = JsonConvert.DeserializeObject(config);
            azureDevOpsUrl = settings.AzureDevOpsUrl;
            project = settings.Project;
            personalAccessToken = settings.PersonalAccessToken;
        }

        /// <summary>
        /// Creates a new bug in Azure DevOps with the specified title.
        /// </summary>
        /// <param name="bugTitle">The title of the bug to be created.</param>
        public void CreateBug(string bugTitle)
        {
            // Create a RestClient with the appropriate Azure DevOps API endpoint for bug creation
            var client = new RestClient($"{azureDevOpsUrl}/{project}/_apis/wit/workitems/$Bug?api-version=6.0");

            // Create a POST request and set required headers
            var request = new RestRequest(Method.POST);
            request.AddHeader("Content-Type", "application/json-patch+json");
            // Create an authentication token from the personal access token (PAT)
            string authToken = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{personalAccessToken}"));
            request.AddHeader("Authorization", $"Basic {authToken}");

            // Prepare the data payload for the bug work item
            var bugData = new[]
            {
                new { op = "add", path = "/fields/System.Title", value = bugTitle },
                new { op = "add", path = "/fields/System.Description", value = "Bug created automatically due to failed Selenium test." },
                new { op = "add", path = "/fields/System.AssignedTo", value = "shahab@tecoholic.com" },
                new { op = "add", path = "/fields/Microsoft.VSTS.TCM.ReproSteps", value = "See attached logs for detailed error." }
            };

            // Add the bug data as the request body
            request.AddParameter("application/json-patch+json", JsonConvert.SerializeObject(bugData), ParameterType.RequestBody);

            // Execute the request and get the response
            IRestResponse response = client.Execute(request);

            // Output the result based on whether the request was successful
            if (response.IsSuccessful)
            {
                Console.WriteLine("Bug created successfully in Azure DevOps.");
            }
            else
            {
                Console.WriteLine("Failed to create bug: " + response.ErrorMessage);
            }
        }
    }
}
