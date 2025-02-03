Feature: Health Check

  Scenario: Check Service Health
    When I make a GET request to /healthcheck
    Then the response status code should be 200
