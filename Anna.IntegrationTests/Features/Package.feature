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

  Scenario: Download a nonexistent package
    When I get the list of versions for the package Microsoft.Extensions.FooBar
    Then The response status code should be 404
    When I download the .nupkg file for the package Microsoft.Extensions.FooBar@8.0.1
    Then The response status code should be 404
    When I download the .nuspec file for the package Microsoft.Extensions.FooBar@8.0.1
    Then The response status code should be 404

  Scenario: Delete (unlist) a package
    Given I have a .nupkg file for Microsoft.Extensions.DependencyInjection@9.0.0
    When I publish the package Microsoft.Extensions.DependencyInjection@9.0.0
    Then The response status code should be 202
    When I unlist the package Microsoft.Extensions.DependencyInjection@9.0.0
    Then The response status code should be 204

  Scenario: Hard-delete a package
    Given I have a .nupkg file for Microsoft.Extensions.DependencyInjection@7.0.0
    When I publish the package Microsoft.Extensions.DependencyInjection@7.0.0
    Then The response status code should be 202
    When I delete the package Microsoft.Extensions.DependencyInjection@7.0.0
    Then The response status code should be 204
    When I get the list of versions for the package Microsoft.Extensions.DependencyInjection
    Then The list of versions should not contain 7.0.0

  Scenario: Unlist and re-list a package
    Given I have a .nupkg file for Microsoft.Extensions.DependencyInjection@6.0.2
    When I publish the package Microsoft.Extensions.DependencyInjection@6.0.2
    Then The response status code should be 202
    When I unlist the package Microsoft.Extensions.DependencyInjection@6.0.2
    Then The response status code should be 204
    When I relist the package Microsoft.Extensions.DependencyInjection@6.0.2
    Then The response status code should be 200

  Scenario: Delete a nonexistent package
    When I unlist the package Microsoft.Extensions.FooBar@8.0.0
    Then The response status code should be 404

  Scenario: Relist a nonexistent package
    When I relist the package Microsoft.Extensions.FooBar@8.0.0
    Then The response status code should be 404
