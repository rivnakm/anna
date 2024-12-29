Feature: Health Check

  Scenario: Check Service Health
    When I make a GET request to /healthcheck
    Then The response status code should be 200
