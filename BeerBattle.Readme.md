# Beer Battle
Beer battle is a game in which different groups of people compete to drink the most.

## General behavior
When people in a group order drinks a number of points is assigned to that groups depending on what they ordered. The chart window gives an overview of the total number of points each group has earned so far and updates with every order.

## Usage
All you need to run `Beer Battle` is the executable itself and a file named `Data.json` that is placed right next to it. This second file contains the list of groups that participate in the game, the products/categories you are going to sell and some other configuration. Check the section below for a thorough explanation off all configuration options. An example configuration file is included when you download the app.

To start using the app you need to:
* Prepare the configuration file.
* Launch the app.
* Drag the chart window to the big screen.
* Click the `(De)maximize chart window` button on the input window to make the chart window full screen.
* Click the `Reset` button on the input window to ensure all values are properly set unless you want to continue from where you last left off.
* For every order, enter the amounts of the products ordered. You will see the total points for convenience. To confirm the order, click the `Commit` button of the group to which the points should be assigned.

You can at all times reset everything to the initial state by clicking the `Reset` button.
If you restart the app without resetting it will continue where it left off.

## Configurations
The values described below are used to control the behavior of the app. Any values that are not mentioned here should never be changed by hand.

* `windowBackgroundColor`  
Type: string in hexadecimal RGB color format, e.g. "#FF0000" for red  
The normal background color of the windows.

* `totalPointsNumberOfDecimals`  
Type: integer  
The number of decimals that the total points will be rounded to in the UI.  
This only affects the representation in the UI. Behind the scenes, all values maintain their full precision.

* `groups`  
A list of groups. Every group has the following configuration options:
	* `name`  
    Type: string  
	The name of the group as shown on all windows.

	* `color`  
	Type: string in hexadecimal RGB color format, e.g. "#FFFFFF" for white  
	The color of the group's chart and `Commit` button.

* `products`  
A list of products/categories. Every product has the following configuration options:
	* `name`  
    Type: string  
	The name of the product as shown on all windows.

	* `color`  
	Type: string in hexadecimal RGB color format, e.g. "#FFFFFF" for white  
	The color of the product's input card.

	* `pointsPerItem`  
	Type: floating point  
	The number of points every item of this product is worth.