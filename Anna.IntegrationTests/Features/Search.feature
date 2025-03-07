Feature: Package search

    Scenario: Search with an empty query
        Given I have uploaded the following packages
          | Id          | Version    |
          | Azure.Core  | 1.45.0     |
          | Azure.Core  | 1.44.1     |
          | Azure.Core  | 1.44.0     |
          | AWSSDK.Core | 3.7.402.14 |
          | AWSSDK.Core | 3.7.402.13 |
        When I perform a search with query:
        Then the response status code should be 200
        And the search results should contain 2 items and 2 total hits

    Scenario: Search with a query matching a package name
        Given I have uploaded the following packages
          | Id          | Version    |
          | Azure.Core  | 1.45.0     |
          | AWSSDK.Core | 3.7.402.13 |
        When I perform a search with query: Azure.Core
        Then the response status code should be 200
        And the search results should contain 1 item and 1 total hit

    Scenario: Search excludes prereleases by default
        Given I have uploaded the following packages
          | Id          | Version         |
          | Azure.Core  | 1.45.0          |
          | Azure.Core  | 1.44.0          |
          | AWSSDK.Core | 4.0.0-preview.2 |
          | AWSSDK.Core | 4.0.0-preview   |
        When I perform a search with query:
        Then the response status code should be 200
        And the search results should contain 1 item and 1 total hit

    Scenario: Search with prereleases included
        Given I have uploaded the following packages
          | Id          | Version         |
          | Azure.Core  | 1.45.0          |
          | Azure.Core  | 1.44.0          |
          | AWSSDK.Core | 4.0.0-preview.2 |
          | AWSSDK.Core | 4.0.0-preview   |
        And I have a search query:
        And I set the search parameter prerelease to true
        When I perform the search
        Then the response status code should be 200
        And the search results should contain 2 items and 2 total hits

    Scenario: Search returns all versions of each result
        Given I have uploaded the following packages
          | Id          | Version    |
          | Azure.Core  | 1.45.0     |
          | Azure.Core  | 1.44.1     |
          | Azure.Core  | 1.44.0     |
          | AWSSDK.Core | 3.7.402.14 |
          | AWSSDK.Core | 3.7.402.13 |
        When I perform a search with query:
        Then the response status code should be 200
        And the search results should contain 2 items and 2 total hits
        And the search result for Azure.Core should contain 3 versions
        And the search result for AWSSDK.Core should contain 2 versions

    Scenario: Search returns all versions except prereleases
        Given I have uploaded the following packages
          | Id          | Version       |
          | Azure.Core  | 1.45.0        |
          | Azure.Core  | 1.44.1        |
          | Azure.Core  | 1.44.0        |
          | AWSSDK.Core | 3.7.402.14    |
          | AWSSDK.Core | 3.7.402.13    |
          | AWSSDK.Core | 4.0.0-preview |
        When I perform a search with query:
        Then the response status code should be 200
        And the search results should contain 2 items and 2 total hits
        And the search result for Azure.Core should contain 3 versions
        And the search result for AWSSDK.Core should contain 2 versions

    Scenario: Search returns all versions including prereleases
        Given I have uploaded the following packages
          | Id          | Version       |
          | Azure.Core  | 1.45.0        |
          | Azure.Core  | 1.44.1        |
          | Azure.Core  | 1.44.0        |
          | AWSSDK.Core | 3.7.402.14    |
          | AWSSDK.Core | 3.7.402.13    |
          | AWSSDK.Core | 4.0.0-preview |
        And I have a search query:
        And I set the search parameter prerelease to true
        When I perform the search
        Then the response status code should be 200
        And the search results should contain 2 items and 2 total hits
        And the search result for Azure.Core should contain 3 versions
        And the search result for AWSSDK.Core should contain 3 versions
