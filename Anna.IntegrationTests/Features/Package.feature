Feature: Package Publishing and Retrieval

  Scenario: Upload a package
    Given I have a .nupkg file for Microsoft.Extensions.DependencyInjection@8.0.1
    When I upload the package Microsoft.Extensions.DependencyInjection@8.0.1
    Then the response status code should be 202

  Scenario: Download a package
    Given I have uploaded the package Microsoft.Extensions.DependencyInjection@8.0.1
    When I download the package Microsoft.Extensions.DependencyInjection@8.0.1
    Then the response status code should be 200

  Scenario: Download a package spec
    Given I have uploaded the package Microsoft.Extensions.DependencyInjection@8.0.1
    When I download the package spec for Microsoft.Extensions.DependencyInjection@8.0.1
    Then the response status code should be 200

  Scenario: Unlist a package
    Given I have uploaded the package Microsoft.Extensions.DependencyInjection@8.0.0
    When I unlist the package Microsoft.Extensions.DependencyInjection@8.0.0
    Then the response status code should be 204

  Scenario: Relist a package
    Given I have uploaded the package Microsoft.Extensions.DependencyInjection@9.0.0
    And I have unlisted the package Microsoft.Extensions.DependencyInjection@9.0.0
    When I relist the package Microsoft.Extensions.DependencyInjection@9.0.0
    Then the response status code should be 200
