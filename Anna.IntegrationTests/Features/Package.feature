Feature: Package Publishing and Retrieval

  Scenario: Upload and download a package
    Given I have a .nupkg file for Microsoft.Extensions.DependencyInjection@8.0.1
    When I publish the package Microsoft.Extensions.DependencyInjection@8.0.1
    Then The response status code should be 202
    When I get the list of versions for the package Microsoft.Extensions.DependencyInjection
    Then The response status code should be 200
    And The list of versions should contain 8.0.1
    When I download the .nupkg file for the package Microsoft.Extensions.DependencyInjection@8.0.1
    Then The response status code should be 200
    And The response content type should be application/octet-stream
    When I download the .nuspec file for the package Microsoft.Extensions.DependencyInjection@8.0.1
    Then The response status code should be 200
    And The response content type should be application/xml
