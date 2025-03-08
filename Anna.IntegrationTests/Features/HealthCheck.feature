Feature: Health Check

    Scenario: Check Service Health
        When I check the service health
        Then the service should be healthy