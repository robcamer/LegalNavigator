﻿Feature: TopicsAndResources
	In order to avoid silly mistakes
	As a math idiot
	I want to be told the sum of two numbers

Scenario: Navigate to topics page from home page
	Given I am on the Access2Justice website with state set
	| State  |
	| Alaska |
	And I am on the home page
	When I press See More Topics button in the section named More Information, Videos, and Links to Resources
	Then I should directed to the Topics and Resources page

Scenario: Navigate to topics page by clicking on lower navigation bar
	Given I am on the Access2Justice website with state set
	| State  |
	| Alaska |
	When I click on Topics & Resources on the lower navigation bar
	Then I should directed to the Topics and Resources page