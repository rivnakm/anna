Feature: Package Registration Index

  Scenario: Get registration index for a package
    Given I have uploaded the package Microsoft.Extensions.Logging@7.0.0
    When I get the registration index for the package Microsoft.Extensions.Logging
    Then the response status code should be 200
    And the registration index should contain 1 page
    And the 1st inline registration index page should contain the version 7.0.0

