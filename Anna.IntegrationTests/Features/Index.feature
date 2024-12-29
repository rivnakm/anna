Feature: Service Index

  Scenario: Get NuGet service index
    When I make a GET request to /v3/index.json
    Then The response status code should be 200
    And The response content type should be application/json
    And The index should be version 3.0.0
    And The index should contain a PackageBaseAddress/3.0.0 resource
    And The index should contain a PackagePublish/2.0.0 resource
