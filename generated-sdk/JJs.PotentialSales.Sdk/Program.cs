using JJs.PotentialSales.Sdk.Prospects;
using JJs.PotentialSales.Sdk.Common;

namespace JJs.PotentialSales.Sdk.Tests;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== JJs Potential Sales SDK - VSA Demo ===");
        Console.WriteLine();

        // Demo 1: Create a prospect request
        var createRequest = new CreateProspectRequest
        {
            PotentialSaleNumber = "PSL0001234",
            SiteNumber = "SITE001",
            Assignee = "John Smith",
            ProspectTypeId = "NEW_BUSINESS",
            SourceId = "WEBSITE",
            CustomerStatus = CustomerStatus.Prospect,
            TradingName = "ACME Corporation",
            ProspectStatus = ProspectStatus.Open,
            Interest = Interest.Yes,
            AddressLine = "123 Business St, Business City, BC 12345",
            ContactEmail = "contact@acme.com",
            ContactFirstName = "Jane",
            ContactLastName = "Doe",
            Description = "Potential customer interested in our waste management services"
        };

        Console.WriteLine("1. Created Prospect Request:");
        Console.WriteLine($"   Prospect Number: {createRequest.PotentialSaleNumber}");
        Console.WriteLine($"   Trading Name: {createRequest.TradingName}");
        Console.WriteLine($"   Status: {createRequest.ProspectStatus}");
        Console.WriteLine($"   Interest: {createRequest.Interest}");
        Console.WriteLine();

        // Demo 2: Validate the request using FluentValidation
        var validator = new CreateProspectValidator();
        var validationResult = await validator.ValidateAsync(createRequest);

        Console.WriteLine("2. FluentValidation Result:");
        if (validationResult.IsValid)
        {
            Console.WriteLine("   ✓ Validation passed");
        }
        else
        {
            Console.WriteLine("   ✗ Validation failed:");
            foreach (var error in validationResult.Errors)
            {
                Console.WriteLine($"     - {error.PropertyName}: {error.ErrorMessage}");
            }
        }
        Console.WriteLine();

        // Demo 3: Convert to entity using extension methods
        if (validationResult.IsValid)
        {
            var entity = createRequest.ToEntity();
            Console.WriteLine("3. Converted to Entity (using extension methods):");
            Console.WriteLine($"   Prospect ID: {entity.ProspectId}");
            Console.WriteLine($"   Created At: {entity.CreatedAt}");
            Console.WriteLine();

            // Demo 4: Convert back to response
            var response = entity.ToResponse();
            Console.WriteLine("4. Converted to Response:");
            Console.WriteLine($"   Prospect ID: {response.ProspectId}");
            Console.WriteLine($"   Trading Name: {response.TradingName}");
            Console.WriteLine($"   Status: {response.ProspectStatus}");
            Console.WriteLine();

            // Demo 5: Create API result
            var apiResult = entity.ToApiResult();
            Console.WriteLine("5. API Result Pattern:");
            Console.WriteLine($"   Success: {apiResult.IsSuccess}");
            Console.WriteLine($"   Data Type: {apiResult.Data?.GetType().Name}");
            Console.WriteLine();
        }

        // Demo 6: Show feature folder organization
        Console.WriteLine("6. VSA Feature Folder Structure:");
        Console.WriteLine("   JJs.PotentialSales.Sdk/");
        Console.WriteLine("   ├── Prospects/              (Feature Folder - Vertical Slice)");
        Console.WriteLine("   │   ├── CreateProspectRequest.cs");
        Console.WriteLine("   │   ├── ProspectResponse.cs");
        Console.WriteLine("   │   ├── GetProspectRequest.cs");
        Console.WriteLine("   │   ├── CreateProspectValidator.cs");
        Console.WriteLine("   │   ├── ProspectExtensions.cs");
        Console.WriteLine("   │   ├── ProspectResult.cs");
        Console.WriteLine("   │   └── ProspectEnums.cs");
        Console.WriteLine("   └── Common/");
        Console.WriteLine("       ├── ApiResult.cs");
        Console.WriteLine("       └── ValidationExtensions.cs");
        Console.WriteLine();

        Console.WriteLine("✓ VSA SDK Generation Complete!");
        Console.WriteLine("✓ All components are organized by feature (Prospects) not technical layer");
        Console.WriteLine("✓ FluentValidation only, Records only, Extension methods for mapping");
        Console.WriteLine("✓ Ready for production use!");
    }
}
