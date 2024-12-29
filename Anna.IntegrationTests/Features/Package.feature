Feature: Package Publishing and Retrieval

  Scenario: Upload and download a package
    Given I have a .nupkg file for Microsoft.Extensions.DependencyInjection@8.0.1
    And I set the request content to the package Microsoft.Extensions.DependencyInjection@8.0.1
    When I make a PUT request to /api/v2/package
    Then The response status code should be 202
    When I make a GET request to /v3-flatcontainer/microsoft.extensions.dependencyinjection/index.json
    Then The response status code should be 200
    And The response should be a list of versions
    And The list of versions should contain 8.0.1
    When I make a GET request to /v3-flatcontainer/microsoft.extensions.dependencyinjection/8.0.1/microsoft.extensions.dependencyinjection.8.0.1.nupkg
    Then The response status code should be 200
    And The response content type should be application/octet-stream
    When I make a GET request to /v3-flatcontainer/microsoft.extensions.dependencyinjection/8.0.1/microsoft.extensions.dependencyinjection.8.0.1.nuspec
    Then The response status code should be 200
    And The response content type should be application/xml

  Scenario: Download a nonexistent package
    When I make a GET request to /v3-flatcontainer/microsoft.extensions.foobar/index.json
    Then The response status code should be 404
    When I make a GET request to /v3-flatcontainer/microsoft.extensions.foobar/8.0.1/microsoft.extensions.foobar.8.0.1.nupkg
    Then The response status code should be 404
    When I make a GET request to /v3-flatcontainer/microsoft.extensions.foobar/8.0.1/microsoft.extensions.foobar.8.0.1.nuspec
    Then The response status code should be 404

  Scenario: Delete (unlist) a package
    Given I have a .nupkg file for Microsoft.Extensions.DependencyInjection@9.0.0
    And I set the request content to the package Microsoft.Extensions.DependencyInjection@9.0.0
    When I make a PUT request to /api/v2/package
    Then The response status code should be 202
    When I make a DELETE request to /api/v2/package/Microsoft.Extensions.DependencyInjection/9.0.0
    Then The response status code should be 204

  Scenario: Hard-delete a package
    Given I have a .nupkg file for Microsoft.Extensions.DependencyInjection@7.0.0
    And I set the request content to the package Microsoft.Extensions.DependencyInjection@7.0.0
    When I make a PUT request to /api/v2/package
    Then The response status code should be 202
    Given I set the request header x-anna-hard-delete to true
    When I make a DELETE request to /api/v2/package/Microsoft.Extensions.DependencyInjection/7.0.0
    Then The response status code should be 204
    When I make a GET request to /v3-flatcontainer/microsoft.extensions.dependencyinjection/index.json
    Then The response status code should be 200
    And The response should be a list of versions
    Then The list of versions should not contain 7.0.0

  Scenario: Unlist and re-list a package
    Given I have a .nupkg file for Microsoft.Extensions.DependencyInjection@6.0.2
    And I set the request content to the package Microsoft.Extensions.DependencyInjection@6.0.2
    When I make a PUT request to /api/v2/package
    Then The response status code should be 202
    When I make a DELETE request to /api/v2/package/Microsoft.Extensions.DependencyInjection/6.0.2
    Then The response status code should be 204
    When I make a POST request to /api/v2/package/Microsoft.Extensions.DependencyInjection/6.0.2
    Then The response status code should be 200

  Scenario: Delete a nonexistent package
    When I make a DELETE request to /api/v2/package/Microsoft.Extensions.FooBar/6.0.2
    Then The response status code should be 404

  Scenario: Relist a nonexistent package
    When I make a POST request to /api/v2/package/Microsoft.Extensions.FooBar/6.0.2
    Then The response status code should be 404
