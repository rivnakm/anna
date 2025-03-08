Feature: Package Publishing and Retrieval

    Scenario: Upload a package
        Given I have a .nupkg file for Microsoft.Extensions.DependencyInjection@8.0.1
        Then I can upload the package Microsoft.Extensions.DependencyInjection@8.0.1

    Scenario: Download a package
        Given I have uploaded the package Microsoft.Extensions.DependencyInjection@8.0.1
        Then I can download the package Microsoft.Extensions.DependencyInjection@8.0.1

    Scenario: Download a package spec
        Given I have uploaded the package Microsoft.Extensions.DependencyInjection@8.0.1
        Then I can download the package spec for Microsoft.Extensions.DependencyInjection@8.0.1

    Scenario: Unlist a package
        Given I have uploaded the package Microsoft.Extensions.DependencyInjection@8.0.0
        Then I can unlist the package Microsoft.Extensions.DependencyInjection@8.0.0

    Scenario: Relist a package
        Given I have uploaded the package Microsoft.Extensions.DependencyInjection@9.0.0
        And I have unlisted the package Microsoft.Extensions.DependencyInjection@9.0.0
        Then I can relist the package Microsoft.Extensions.DependencyInjection@9.0.0